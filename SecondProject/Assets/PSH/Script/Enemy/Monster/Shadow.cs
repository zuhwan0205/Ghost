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

    private Player player;
    private Transform playerTransform;
    private Animator anim;

    private bool isChasing = false;
    private float despawnTimer = 0f;
    private bool hasTriggered = false;
    private bool hasBeenSeen = false;

    private void Awake()
    {
        if (roomChase == null)
            roomChase = GetComponent<GhostRoomChase>();
    }

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<Player>();

        StopAudioGroup(chaseSources); // 추적 사운드 정지
        PlayAudioGroup(idleSources);  // 대기 사운드 재생

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

        if (didMove)
        {
            // 플레이어가 이동 중이면 유령도 따라가며 추적 애니메이션 및 사운드 전환
            Vector3 targetPosition = playerTransform.position - playerTransform.right * spawnDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 2f);
            LookAt(playerTransform.position);

            if (anim != null)
            {
                anim.SetBool("Tracking", true);
                anim.SetBool("Idle", false);
            }

            StopAudioGroup(idleSources);
            PlayAudioGroup(chaseSources);
        }
        else
        {
            // 플레이어가 멈추면 유령도 멈추고 Idle 애니메이션 및 사운드 전환
            if (anim != null)
            {
                anim.SetBool("Tracking", false);
                anim.SetBool("Idle", true);
            }

            StopAudioGroup(chaseSources);
            PlayAudioGroup(idleSources);
        }

        // 일정 시간이 지나면 자동 삭제
        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
        {
            Destroy(gameObject);
        }

        bool isLooking = PlayerLookingAtMe();
        bool sameRoom = IsSameRoom();

        // 플레이어가 유령을 바라보면 추적 시작
        if (!hasBeenSeen && isLooking && sameRoom)
        {
            hasBeenSeen = true;
            isChasing = true;

            if (anim != null)
            {
                anim.SetBool("Tracking", true);
                anim.SetBool("Idle", false);
            }
        }

        // 추적 상태이면 플레이어에게 접근
        if (isChasing && sameRoom)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, Time.deltaTime * 5f);
            LookAt(playerTransform.position);
        }

        // 다른 방에 있을 경우 방 이동 시도
        if (!sameRoom && roomChase != null)
        {
            roomChase.TryChaseOnBlocked();

            if (anim != null)
            {
                anim.SetBool("Tracking", true);
                anim.SetBool("Idle", false);
            }
        }
    }

    // 플레이어가 유령을 바라보고 있는지 확인
    private bool PlayerLookingAtMe()
    {
        Vector2 toEnemy = (transform.position - playerTransform.position).normalized;
        Vector2 playerLookDir = playerTransform.right;
        if (playerTransform.localScale.x < 0)
            playerLookDir *= -1;

        float angle = Vector2.Angle(playerLookDir, toEnemy);
        return angle < 60f;
    }

    // 플레이어와 유령이 같은 방에 있는지 확인
    private bool IsSameRoom()
    {
        if (roomChase == null || roomChase.playerRoom == null)
            return false;

        return roomChase.currentRoomID == roomChase.playerRoom.currentRoomID;
    }

    // 대상 방향으로 유령 회전
    private void LookAt(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        float originalX = Mathf.Abs(scale.x);
        scale.x = target.x < transform.position.x ? -originalX : originalX;
        transform.localScale = scale;
    }

    // 플레이어와 충돌 시 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                // 플레이어 행동 제한
                player.isHiding = true;
                player.isInteractionLocked = true;
                player.ResetHold();
                player.TakeDamage(damage);

                if (anim != null)
                {
                    anim.SetBool("Tracking", true);
                    anim.SetBool("Idle", false);
                    anim.SetBool("Grab", true);
                }

                // 사운드 정지 및 충돌 사운드 재생
                StopAudioGroup(chaseSources);
                StopAudioGroup(idleSources);
                AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);

                Invoke(nameof(EnablePlayer), 3.0f);
            }

            Destroy(gameObject, 3.0f);
        }
    }

    // 플레이어 이동 복구
    private void EnablePlayer()
    {
        if (player != null)
        {
            player.isHiding = false;
            player.isInteractionLocked = false;
        }
    }

    // 오디오 정지
    private void StopAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && source.isPlaying)
                source.Stop();
        }
    }

    // 오디오 재생
    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && !source.isPlaying)
                source.Play();
        }
    }
}
