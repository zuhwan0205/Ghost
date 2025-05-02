using UnityEngine;

public class StandingMan : MonoBehaviour
{
    [Header("StandingMan 상태")]
    private Vector3 lastPlayerPos; // 마지막으로 기록한 플레이어 위치
    private bool isChasing = false; // 추적 시작 여부
    private bool isPaused = false; // 몬스터가 멈춰있는지 여부

    private float offScreenTimer = 0f; // 화면 밖에 있었던 시간 측정

    [Header("StandingMan 설정")]
    [SerializeField] private float moveSpeed = 4f; // 몬스터 이동 속도
    [SerializeField] private float spawnDistance = 3.0f; // 플레이어 뒤에 생성할 거리
    [SerializeField] private float detectDistance = 6f; // 플레이어 감지 거리
    [SerializeField] private float wallCheckDistance = 0.5f; // 벽 감지 거리
    [SerializeField] private float despawnTime = 6f; // 화면 밖에서 삭제되기까지 걸리는 시간
    [SerializeField] private LayerMask playerLayer; // 플레이어 감지용 LayerMask
    [SerializeField] private LayerMask wallLayer; // 벽 감지용 LayerMask

    private Renderer rend; // 몬스터의 Renderer 컴포넌트
    private Rigidbody2D rb; // 몬스터의 Rigidbody2D 컴포넌트
    private Transform playerTransform; // 플레이어 Transform

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponent<Renderer>();

        Player player = FindFirstObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("[StandingMan] Player를 찾을 수 없습니다.");
            return;
        }

        // 플레이어 뒤쪽에 스폰
        float direction = playerTransform.localScale.x > 0 ? -1f : 1f;
        transform.position = playerTransform.position + new Vector3(spawnDistance * direction, 0f, 0f);

        lastPlayerPos = playerTransform.position;
    }

    private void Update()
    {
        if (isPaused || playerTransform == null) return;

        DetectPlayerMovement();
        HandleDespawnTimer();
        WallCheck();
    }

    // 플레이어 이동 감지 및 추적
    private void DetectPlayerMovement()
    {
        Vector3 currentPlayerPos = playerTransform.position;

        if (!isChasing && Vector3.Distance(currentPlayerPos, lastPlayerPos) > 0.01f)
        {
            isChasing = true;
            Debug.Log("플레이어 이동 감지: 추적 시작!");
        }

        if (isChasing && PlayerInFront())
        {
            MoveToTarget(playerTransform.position);
            LookAt(playerTransform.position);
        }

        lastPlayerPos = currentPlayerPos;
    }

    // 타겟 위치로 이동
    private void MoveToTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    // 플레이어 방향 바라보기
    private void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

    // 플레이어가 앞에 있는지 감지
    private bool PlayerInFront()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectDistance, playerLayer);
        Debug.DrawRay(transform.position, direction * detectDistance, Color.red);

        if (hit.collider != null && hit.collider.GetComponent<Player>() != null)
        {
            return true;
        }

        return false;
    }

    // 벽 감지
    private void WallCheck()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, wallLayer);
        Debug.DrawRay(transform.position, direction * wallCheckDistance, Color.blue);

        if (hit.collider != null)
        {
            Flip();
            Debug.Log("벽 감지: 방향 전환");
        }
    }

    // 몬스터 방향 반전
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 화면 밖에 오래 있으면 삭제
    private void HandleDespawnTimer()
    {
        if (rend == null) return;

        if (rend.isVisible)
        {
            offScreenTimer = 0f;
        }
        else
        {
            offScreenTimer += Time.deltaTime;
            if (offScreenTimer >= despawnTime)
            {
                Destroy(gameObject);
            }
        }
    }

    // 디버그용 감지 거리 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * detectDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, direction * wallCheckDistance);
    }

    // 플레이어와 트리거 충돌 감지
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[StandingMan] 플레이어와 트리거 충돌 발생!");
            OnPlayerTrigger(collision);
        }
    }

    // 플레이어와 트리거 충돌 시 처리
    private void OnPlayerTrigger(Collider2D collision)
    {
        rb.linearVelocity = Vector2.zero; // 몬스터 멈추기
        isChasing = false;
        isPaused = true;

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(10f);

            // 플레이어 이동속도, 대쉬속도 모두 0으로 설정
            player.moveSpeed = 0f;
            player.dashSpeed = 0f;

            // 3초 후 플레이어 이동 복구
            Invoke(nameof(EnablePlayerMovement), 3.0f);
        }

        // 5초 후 몬스터 이동 복구
        Invoke(nameof(ResumeStandingManMovement), 5.0f);
    }

    // 플레이어 이동 복구
    private void EnablePlayerMovement()
    {
        if (playerTransform != null)
        {
            Player player = playerTransform.GetComponent<Player>();
            if (player != null)
            {
                Debug.Log("[StandingMan] 3초 후 플레이어 이동 재개!");
                player.moveSpeed = player.originalMoveSpeed;
                player.dashSpeed = player.originalDashSpeed;
            }
        }
    }

    // 몬스터 이동 복구
    private void ResumeStandingManMovement()
    {
        isPaused = false;
        lastPlayerPos = transform.position;
        Debug.Log("[StandingMan] 5초 후 몬스터 이동 재개!");
    }
}
