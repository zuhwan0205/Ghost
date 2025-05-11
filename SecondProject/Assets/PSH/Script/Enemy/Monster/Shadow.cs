using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float spawnDistance;
    [SerializeField] private float despawnDelay;
    [SerializeField] private LayerMask playerLayer;

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
    private GhostRoomChase roomChase;
    private CameraZoomController zoomController;

    [Header("상태 확인")]
    [SerializeField] private bool isChasing = false;
    [SerializeField] private bool isTrackingBehindPlayer = true;
    [SerializeField] private bool isPaused = false;
    private bool hasTriggered = false;
    private bool hasBeenSeen = false;

    private float despawnTimer = 0f;
    private string lastKnownRoomID = "";

    private void Awake()
    {
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
            spawnPos.y += 1f;
            transform.position = spawnPos;
            LookAt(playerTransform.position);
        }

        zoomController = Object.FindAnyObjectByType<CameraZoomController>();
    }

    private void Update()
    {
        if (playerTransform == null || isPaused) return;

        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
        {
            Destroy(gameObject);
            return;
        }

        string currentRoom = roomChase?.currentRoomID;
        string playerRoom = roomChase?.playerRoom?.currentRoomID;

        // 포탈 통해 같은 방 도착 시 상태 초기화
        if (currentRoom != null && playerRoom != null)
        {
            if (currentRoom == playerRoom && lastKnownRoomID != currentRoom)
            {
                Debug.Log("[Shadow] 포탈 통해 같은 방 도달 → 상태 초기화");

                isChasing = false;
                isTrackingBehindPlayer = true;
                hasBeenSeen = false;

                anim?.SetBool("Tracking", false);
                anim?.SetBool("Idle", true);
                StopAudioGroup(chaseSources);
                PlayAudioGroup(idleSources);
            }

            lastKnownRoomID = currentRoom;
        }

        // 다른 방이면 포탈 추적 요청
        if (!IsSameRoom())
        {
            roomChase?.TryChaseOnBlocked();
            return;
        }

        // 추적 시작 조건
        if (!isChasing && !isPaused && !hasBeenSeen && PlayerLookingAtMe())
        {
            isChasing = true;
            isTrackingBehindPlayer = false;
            hasBeenSeen = true;

            Debug.Log("[Shadow] 추적 시작!");
            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
            StopAudioGroup(idleSources);
            PlayAudioGroup(chaseSources);
        }

        LookAt(playerTransform.position);
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isPaused) return;

        // 포탈 추적 이동
        if (!IsSameRoom())
        {
            if (roomChase != null && roomChase.ghostTelePort != null && roomChase.ghostTelePort.HasRoute())
            {
                Vector2 portalTarget = roomChase.ghostTelePort.GetCurrentTargetPortalPosition();
                Vector2 dirToPortal = (portalTarget - (Vector2)transform.position).normalized;
                float _movespeed = moveSpeed;

                RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dirToPortal, _movespeed * Time.fixedDeltaTime, LayerMask.GetMask("Wall"));
                if (wallHit.collider == null)
                {
                    rb.linearVelocity = dirToPortal * _movespeed;
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }

                anim?.SetBool("Tracking", true);
                anim?.SetBool("Idle", false);
            }

            return;
        }

        // 같은 방일 경우 기본 추적/추적대기 상태 처리
        Vector2 direction;
        float speed;

        if (isChasing)
        {
            direction = (playerTransform.position - transform.position).normalized;
            speed = chaseSpeed;
        }
        else if (isTrackingBehindPlayer)
        {
            Vector3 targetPos = playerTransform.position - playerTransform.right * spawnDistance;
            direction = (targetPos - transform.position).normalized;

            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            speed = distanceToPlayer >= 7f ? chaseSpeed : moveSpeed;

            float distToTarget = Vector2.Distance(transform.position, targetPos);
            if (distToTarget <= 0.1f)
            {
                rb.linearVelocity = Vector2.zero;
                anim?.SetBool("Tracking", false);
                anim?.SetBool("Idle", true);
                return;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 벽 충돌 검사
        float checkDist = Mathf.Max(speed * Time.fixedDeltaTime, 0.3f);
        RaycastHit2D wall = Physics2D.Raycast(transform.position, direction, checkDist, LayerMask.GetMask("Wall"));
        if (wall.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            anim?.SetBool("Tracking", false);
            anim?.SetBool("Idle", true);
            return;
        }

        rb.linearVelocity = direction * speed;
        anim?.SetBool("Tracking", true);
        anim?.SetBool("Idle", false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            isPaused = true;
            isChasing = false;
            isTrackingBehindPlayer = false;
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
        if (!IsSameRoom()) return false;

        Vector2 toEnemy = (transform.position - playerTransform.position).normalized;
        Vector2 playerLookDir = (playerTransform.localScale.x >= 0f) ? Vector2.right : Vector2.left;

        float angle = Vector2.Angle(playerLookDir, toEnemy);
        return angle < 60f;
    }

    private bool IsSameRoom()
    {
        return roomChase != null &&
               roomChase.playerRoom != null &&
               roomChase.currentRoomID == roomChase.playerRoom.currentRoomID;
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
