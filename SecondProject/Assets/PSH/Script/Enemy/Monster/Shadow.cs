using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("설정값")]
    [SerializeField] private float moveSpeed = 2f;             // 기본 이동 속도
    [SerializeField] private float chaseSpeed = 7f;            // 추적 시 빠른 속도
    [SerializeField] private float spawnDistance = 5f;         // 플레이어 뒤 따라가는 거리
    [SerializeField] private float despawnDelay = 10f;         // 자동 제거 시간
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GhostRoomChase roomChase;         // 방 이동 로직

    [Header("사운드")]
    [SerializeField] private string hitPlayerSound = "shadow_hit";
    [SerializeField] private AudioSource[] idleSources;
    [SerializeField] private AudioSource[] chaseSources;

    [Header("데미지")]
    [SerializeField] private float damage;

    private Player player;
    private Transform playerTransform;
    private Animator anim;
    private Rigidbody2D rb;

    private bool isChasing = false;               // 플레이어를 직접 추적하는 상태
    private bool isTrackingBehindPlayer = true;   // 플레이어 뒤 일정 거리 유지하는 상태
    private bool hasTriggered = false;            // 트리거 처리 여부
    private bool hasBeenSeen = false;
    private bool isPaused = false;                // 이동 일시 정지

    private float despawnTimer = 0f;
    private CameraZoomController zoomController;

    private void Awake()
    {
        if (roomChase == null)
            roomChase = GetComponent<GhostRoomChase>();
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        player = FindFirstObjectByType<Player>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        StopAudioGroup(chaseSources);
        PlayAudioGroup(idleSources);

        // 플레이어 뒤에 생성
        if (playerTransform != null)
        {
            Vector3 spawnPos = playerTransform.position - playerTransform.right * spawnDistance;
            spawnPos.y += 2f;
            transform.position = spawnPos;
            LookAt(playerTransform.position);
        }

        zoomController = Object.FindAnyObjectByType<CameraZoomController>();
    }

    private void Update()
    {
        if (playerTransform == null || isPaused) return;

        bool sameRoom = IsSameRoom();

        if (!sameRoom)
        {
            // 다른 방에 있으면 방 이동만 시도
            roomChase?.TryChaseOnBlocked();
            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
            return;
        }

        // 같은 방일 때만 시선 감지 및 행동
        if (!isChasing && PlayerLookingAtMe())
        {
            isChasing = true;
            isTrackingBehindPlayer = false;

            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);

            StopAudioGroup(idleSources);
            PlayAudioGroup(chaseSources);
        }

        if (isChasing || isTrackingBehindPlayer)
            LookAt(playerTransform.position);

        // 제거 타이머
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isPaused || !IsSameRoom()) return;

        if (isChasing)
        {
            // 추적 모드: 플레이어 위치로 직진
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = dir * chaseSpeed;

            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
        }
        else if (isTrackingBehindPlayer)
        {
            // 기본 상태: 플레이어 뒤쪽 위치 따라가기
            Vector3 targetPos = playerTransform.position - playerTransform.right * spawnDistance;
            Vector2 dir = (targetPos - transform.position).normalized;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float moveSpeedToUse = distanceToPlayer >= 7f ? chaseSpeed : moveSpeed;

            float distToTarget = Vector2.Distance(transform.position, targetPos);

            if (distToTarget > 0.1f)
            {
                rb.linearVelocity = dir * moveSpeedToUse;
                anim?.SetBool("Tracking", true);
                anim?.SetBool("Idle", false);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                anim?.SetBool("Tracking", false);
                anim?.SetBool("Idle", true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            isPaused = true;
            rb.linearVelocity = Vector2.zero;

            anim?.SetBool("Grab", true);
            anim?.SetBool("Tracking", false);
            anim?.SetBool("Idle", false);

            StopAudioGroup(chaseSources);
            StopAudioGroup(idleSources);

            if (zoomController != null)
                zoomController.ZoomInThenOut();

            AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);

            Player pl = collision.GetComponent<Player>();
            if (pl != null)
            {
                pl.isHiding = true;
                pl.isInteractionLocked = true;
                pl.ResetHold();
                pl.TakeDamage(damage);
                pl.TakeGrab();
                Invoke(nameof(EnablePlayer), 3f);
            }

            Destroy(gameObject, 3f);
        }
    }

    private void EnablePlayer()
    {
        if (player != null)
        {
            player.isHiding = false;
            player.isInteractionLocked = false;
            player.AfterGrab();
        }
    }

    private bool PlayerLookingAtMe()
    {
        Vector2 toEnemy = (transform.position - playerTransform.position).normalized;
        Vector2 playerLookDir = playerTransform.right;
        if (playerTransform.localScale.x < 0)
            playerLookDir *= -1;

        float angle = Vector2.Angle(playerLookDir, toEnemy);
        return angle < 60f;
    }

    private bool IsSameRoom()
    {
        if (roomChase == null || roomChase.playerRoom == null)
            return false;

        return roomChase.currentRoomID == roomChase.playerRoom.currentRoomID;
    }

    private void LookAt(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        float originalX = Mathf.Abs(scale.x);
        scale.x = target.x < transform.position.x ? -originalX : originalX;
        transform.localScale = scale;
    }

    private void StopAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
            if (source != null && source.isPlaying)
                source.Stop();
    }

    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
            if (source != null && !source.isPlaying)
                source.Play();
    }
}
