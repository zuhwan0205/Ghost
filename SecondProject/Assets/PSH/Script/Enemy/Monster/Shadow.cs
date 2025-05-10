using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("설정값")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private float despawnDelay = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GhostRoomChase roomChase;

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
    private bool isTrackingBehindPlayer = true;   // 플레이어의 뒤를 일정 거리 유지하며 따라가는 상태
    private bool hasTriggered = false;
    private bool hasBeenSeen = false;
    private bool isPaused = false;

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

        if (playerTransform != null)
        {
            Vector3 spawnPos = playerTransform.position - playerTransform.right * spawnDistance;
            transform.position = spawnPos;
            LookAt(playerTransform.position);
        }

        zoomController = Object.FindAnyObjectByType<CameraZoomController>();

        
    }

    private void Update()
    {
        if (playerTransform == null || isPaused) return;

        bool sameRoom = IsSameRoom();
        bool isLooking = PlayerLookingAtMe();

        // 플레이어가 유령을 바라보면 추적 모드 진입
        if (!isChasing && isLooking && sameRoom)
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

        // 방 다를 경우 텔레포트 시도
        if (!sameRoom && roomChase != null)
        {
            roomChase.TryChaseOnBlocked();

            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
        }

        // 일정 시간 후 제거
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isPaused || !IsSameRoom()) return;

        if (isChasing)
        {
            // 추적 모드: 플레이어를 직접 추적
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = dir * chaseSpeed;
        }
        else if (isTrackingBehindPlayer)
        {
            // 뒤쪽에서 일정 거리 유지
            Vector3 targetPos = playerTransform.position - playerTransform.right * spawnDistance;
            Vector2 dir = (targetPos - transform.position).normalized;

            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist > 0.1f)
                rb.linearVelocity = dir * moveSpeed;
            else
                rb.linearVelocity = Vector2.zero;
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
