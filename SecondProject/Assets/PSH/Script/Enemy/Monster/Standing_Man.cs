using UnityEngine;

public class Standing_Man : MonoBehaviour
{
    [Header("설정값")]
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private float detectDistance = 10f;
    [SerializeField] private float despawnDelay = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private GhostRoomChase roomChase; // 방 추적 컴포넌트

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

            Destroy(gameObject);
        }
    }
}
