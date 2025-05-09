using UnityEngine;

public class Standing_Man : MonoBehaviour
{
    [Header("설정값")]
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private float detectDistance = 10f;
    [SerializeField] private float despawnDelay = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GhostRoomChase roomChase; // 방 추적 컴포넌트

    [Header("오디오")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip chaseClip;
    private bool isPlayingChaseSound = false;

    private Player player;
    private Transform playerTransform;
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

        // idle 소리 재생
        if (audioSource != null && idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.loop = true;
            audioSource.Play();
        }

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

        if (playerTransform.GetComponent<Player>().DidMoveOrInteract)
        {
            Vector3 targetPosition = playerTransform.position - playerTransform.right * spawnDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 2f);
            LookAt(playerTransform.position);
        }

        despawnTimer += Time.deltaTime;
        if (despawnTimer >= despawnDelay)
        {
            Destroy(gameObject);
        }

        // 디버그 로그 추가
        bool isLooking = PlayerLookingAtMe();
        bool sameRoom = IsSameRoom();
        Debug.Log($"[Standing_Man] isLooking: {isLooking}, sameRoom: {sameRoom}, isChasing: {isChasing}");

        // 플레이어가 나를 한 번이라도 바라봤다면 기억함
        if (!hasBeenSeen && isLooking && sameRoom)
        {
            Debug.Log("[Standing_Man] 플레이어가 나를 봤음 → 추적 시작 준비");
            hasBeenSeen = true;
            isChasing = true;
        }

        // 추적 사운드로 전환
        if (isChasing && !isPlayingChaseSound && audioSource != null && chaseClip != null)
        {
            audioSource.Stop();
            audioSource.clip = chaseClip;
            audioSource.loop = true;
            audioSource.Play();
            isPlayingChaseSound = true;
        }

        if (isChasing && sameRoom)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, Time.deltaTime * 5f);
            LookAt(playerTransform.position);
        }

        // 다른 방에 있을 경우 포탈 추적 시도
        if (!sameRoom && roomChase != null)
        {
            roomChase.TryChaseOnBlocked();
        }
    }

    private bool PlayerLookingAtMe()
    {
        Vector2 toEnemy = (transform.position - playerTransform.position).normalized;

        // 정확한 바라보는 방향 계산
        Vector2 playerLookDir = playerTransform.right;
        if (playerTransform.localScale.x < 0)
            playerLookDir *= -1;

        float angle = Vector2.Angle(playerLookDir, toEnemy);

        Debug.Log($"[Standing_Man] PlayerLookAngle: {angle}");

        return angle < 60f;
    }

    private bool IsSameRoom()
    {
        if (roomChase == null)
        {
            Debug.Log("[Standing_Man] roomChase가 연결되지 않음!");
            return false;
        }

        if (roomChase.playerRoom == null)
        {
            Debug.Log("[Standing_Man] playerRoom이 설정되지 않음!");
            return false;
        }

        Debug.Log($"[Standing_Man] currentRoom: {roomChase.currentRoomID}, playerRoom: {roomChase.playerRoom.currentRoomID}");
        return roomChase.currentRoomID == roomChase.playerRoom.currentRoomID;
    }

    private void LookAt(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        scale.x = target.x < transform.position.x ? -1 : 1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;

            Player player = collision.GetComponent<Player>();
            if (player != null)
            {
                player.isHiding = true;             // 이동불가
                player.isInteractionLocked = true; // E 키 차단
                player.ResetHold();                // 홀드 상호작용 즉시 취소
                Invoke(nameof(EnablePlayer), 3.0f);
            }

            Destroy(gameObject);
        }
    }

    // 플레이어 이동 복구
    private void EnablePlayer()
    {
        if (player != null)
        {
            player.isHiding = false;             // 이동 가능
            player.isInteractionLocked = false;
            Debug.Log("[Ghost] 3초 후 플레이어 E키 다시 허용!");
            Debug.Log("[Ghost] 3초 후 플레이어 이동 재개!");
        }
    }
}
