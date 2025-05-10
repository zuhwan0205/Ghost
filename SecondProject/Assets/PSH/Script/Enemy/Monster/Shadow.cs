using UnityEngine;

public class Shadow : MonoBehaviour
{
    [Header("설정값")]
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private float detectDistance = 10f;
    [SerializeField] private float despawnDelay = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GhostRoomChase roomChase;

    [Header("사운드")]
    [SerializeField] private string hitPlayerSound = "shadow_hit";
    [SerializeField] private AudioSource[] idleSources;
    [SerializeField] private AudioSource[] chaseSources;

    [Header("데미지")]
    [SerializeField] private float damage;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;

    private Player player;
    private Transform playerTransform;
    private Animator anim;
    private Rigidbody2D rb;

    private bool isChasing = false;
    private float despawnTimer = 0f;
    private bool hasTriggered = false;
    private bool hasBeenSeen = false;
    private bool isPaused = false;

    private void Awake()
    {
        if (roomChase == null)
            roomChase = GetComponent<GhostRoomChase>();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<Player>();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        StopAudioGroup(chaseSources);
        PlayAudioGroup(idleSources);

        if (playerTransform != null)
        {
            Vector3 spawnPosition = playerTransform.position - playerTransform.right * spawnDistance;
            transform.position = spawnPosition;
            LookAt(playerTransform.position);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        Player playerComp = playerTransform.GetComponent<Player>();
        bool didMove = playerComp != null && playerComp.DidMoveOrInteract;
        bool sameRoom = IsSameRoom();

        // 애니메이션 및 사운드 처리
        if (didMove)
        {
            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
            StopAudioGroup(idleSources);
            PlayAudioGroup(chaseSources);
        }
        else
        {
            anim?.SetBool("Tracking", false);
            anim?.SetBool("Idle", true);
            StopAudioGroup(chaseSources);
            PlayAudioGroup(idleSources);
        }

        // 디스폰 타이머
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
        {
            Destroy(gameObject);
        }

        // 추적 개시
        if (!hasBeenSeen && PlayerLookingAtMe() && sameRoom)
        {
            hasBeenSeen = true;
            isChasing = true;
            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
        }

        // 방이 다르면 텔레포트 추적
        if (!sameRoom && roomChase != null)
        {
            roomChase.TryChaseOnBlocked();
            anim?.SetBool("Tracking", true);
            anim?.SetBool("Idle", false);
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null || isPaused) return;

        bool sameRoom = IsSameRoom();

        if (isChasing && sameRoom)
        {
            float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
            float distanceError = distanceToPlayer - spawnDistance;

            if (Mathf.Abs(distanceError) > 0.2f)
            {
                Vector2 dir = ((Vector2)playerTransform.position - rb.position).normalized;
                float playerSpeed = player != null ? player.CurrentSpeed : moveSpeed;
                float dynamicSpeed = Mathf.Max(playerSpeed * 1.2f, 1.5f);
                rb.linearVelocity = dir * Mathf.Sign(distanceError) * dynamicSpeed;
                LookAt(playerTransform.position);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (!isChasing)
        {
            if (!player.DidMoveOrInteract)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector3 idleTarget = playerTransform.position - playerTransform.right * spawnDistance;
            float distance = Vector2.Distance(rb.position, idleTarget);

            if (distance > 0.2f)
            {
                Vector2 dir = ((Vector2)idleTarget - rb.position).normalized;
                rb.linearVelocity = dir * moveSpeed;
                LookAt(idleTarget);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
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

            Player pl = collision.GetComponent<Player>();
            if (pl != null)
            {
                pl.isHiding = true;
                pl.isInteractionLocked = true;
                pl.ResetHold();
                pl.TakeDamage(damage);

                anim?.SetBool("Tracking", true);
                anim?.SetBool("Idle", false);
                anim?.SetBool("Grab", true);

                StopAudioGroup(chaseSources);
                StopAudioGroup(idleSources);
                AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);

                Invoke(nameof(EnablePlayer), 3.0f);
            }

            Destroy(gameObject, 3.0f);
        }
    }

    private void EnablePlayer()
    {
        if (player != null)
        {
            player.isHiding = false;
            player.isInteractionLocked = false;
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
        return roomChase != null && roomChase.playerRoom != null &&
               roomChase.currentRoomID == roomChase.playerRoom.currentRoomID;
    }

    private void LookAt(Vector3 target)
    {
        float dirX = target.x - transform.position.x;
        if (Mathf.Abs(dirX) < 0.1f) return;

        Vector3 scale = transform.localScale;
        float originalX = Mathf.Abs(scale.x);
        bool shouldLookLeft = dirX < 0;
        if ((shouldLookLeft && scale.x > 0) || (!shouldLookLeft && scale.x < 0))
        {
            scale.x = shouldLookLeft ? -originalX : originalX;
            transform.localScale = scale;
        }
    }

    private void StopAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && source.isPlaying)
                source.Stop();
        }
    }

    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && !source.isPlaying)
                source.Play();
        }
    }
}
