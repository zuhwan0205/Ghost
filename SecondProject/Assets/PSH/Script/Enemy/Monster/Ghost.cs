using UnityEngine;

public class Ghost : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float shortDetectRange = 3f;
    [SerializeField] private float longDetectRange = 30f;
    [SerializeField] private float chaseDetectRange = 6f;

    [Header("레이어 설정")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleAndPlayerLayer;

    private Rigidbody2D rb;
    private Player player;
    private GhostRoomChase ghostRoomChase;

    [SerializeField] private bool isChasing = false;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private bool isPlayerInRange = false;
    private float chaseTimer = 0f;
    private float chaseDuration = 5f;
    private float baseDetectRange;
    private float baseMoveSpeed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
        ghostRoomChase = GetComponent<GhostRoomChase>();
        baseDetectRange = shortDetectRange;
        baseMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isPaused || player == null) return;

        DetectPlayerSide();         // 움직인 경우, 방향 감지만 수행
        DetectPlayerShortRange();   // 추적 모드 전환 감지
        HandleChaseTimer();         // 추적 지속 시간 처리
        Move();                     // 실제 이동
        WallCheck();                // 벽 충돌 시 Flip
    }

    //  움직인 플레이어만 감지하여 방향 전환
    private void DetectPlayerSide()
    {
        if (!player.DidMoveOrInteract) return;

        Vector2 origin = transform.position;

        bool leftOnlyWall = false;
        bool rightOnlyWall = false;

        // 왼쪽 감지
        RaycastHit2D leftHit = Physics2D.Raycast(origin, Vector2.left, longDetectRange, obstacleAndPlayerLayer);
        Debug.DrawRay(origin, Vector2.left * longDetectRange, Color.green);
        if (leftHit.collider != null)
        {
            if (leftHit.collider.CompareTag("Wall"))
            {
                leftOnlyWall = true; // 왼쪽은 벽만 감지
                Debug.Log("[Ghost] 왼쪽 모두 벽만 감지되었습니다1.");
            }
            else if (leftHit.collider.CompareTag("Player"))
            {
                if (transform.localScale.x > 0)
                    Flip();
            }
        }

        // 오른쪽 감지
        RaycastHit2D rightHit = Physics2D.Raycast(origin, Vector2.right, longDetectRange, obstacleAndPlayerLayer);
        Debug.DrawRay(origin, Vector2.right * longDetectRange, Color.green);
        if (rightHit.collider != null)
        {
            if (rightHit.collider.CompareTag("Wall"))
            {
                rightOnlyWall = true; // 오른쪽도 벽만 감지
                Debug.Log("[Ghost] 오른쪽 모두 벽만 감지되었습니다2.");
            }
            else if (rightHit.collider.CompareTag("Player"))
            {
                if (transform.localScale.x < 0)
                    Flip();
            }
        }

        // 왼쪽도 벽, 오른쪽도 벽일 때만 로그 출력
        if (leftOnlyWall && rightOnlyWall)
        {
            Debug.Log("[Ghost] 양쪽 모두 벽만 감지되었습니다.");
            
            if(ghostRoomChase != null)
            {
                ghostRoomChase.TryChaseOnBlocked();
                Debug.Log("맵이동을합니다");
            }

        }
    }

    //  앞쪽 짧은 거리 레이로 감지, 추적 모드 진입 조건
    private void DetectPlayerShortRange()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float currentDetectRange = isChasing ? chaseDetectRange : shortDetectRange;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, currentDetectRange, obstacleAndPlayerLayer);
        Debug.DrawRay(transform.position, direction * currentDetectRange, isChasing ? Color.yellow : Color.red);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (player.DidMoveOrInteract)
            {
                if (!isChasing)
                {
                    isChasing = true;
                    moveSpeed = chaseSpeed;
                }
                isPlayerInRange = true;
                chaseTimer = 0f;
            }
            else
            {
                isPlayerInRange = false;
            }
        }
        else
        {
            isPlayerInRange = false;
        }
    }

    // 추적 유지 시간 체크 (5초 경과 시 종료)
    private void HandleChaseTimer()
    {
        if (isChasing)
        {
            if (!isPlayerInRange)
            {
                chaseTimer += Time.deltaTime;
                if (chaseTimer >= chaseDuration)
                {
                    isChasing = false;
                    chaseTimer = 0f;
                    moveSpeed = baseMoveSpeed;
                }
            }
            else
            {
                chaseTimer = 0f;
            }
        }
    }

    //  이동
    private void Move()
    {
        rb.linearVelocity = new Vector2(moveSpeed * transform.localScale.x, rb.linearVelocity.y);
    }

    // 벽 충돌 시 방향 반전
    private void WallCheck()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(transform.position, direction, 0.5f, LayerMask.GetMask("Wall"));
        Debug.DrawRay(transform.position, direction * 0.5f, Color.blue);

        if (wallHit.collider != null)
        {
            Flip();
        }
    }

    // 방향 전환
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 추적 모드 중이 아니면 무시
        if (!isChasing) return;

        // 플레이어와 충돌했을 때만 반응
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[Ghost] 플레이어와 추적 중 트리거 충돌 발생!");
            OnPlayerTrigger(collision);
        }
    }

    // 플레이어와 충돌했을 때 처리
    private void OnPlayerTrigger(Collider2D collision)
    {
        rb.linearVelocity = Vector2.zero; // 고스트 정지
        isPaused = true;
        isChasing = false;

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.moveSpeed = 0f;
            player.dashSpeed = 0f;
            Invoke(nameof(EnablePlayerMovement), 3.0f); // 3초 후 플레이어 이동 복구
        }

        Invoke(nameof(ResumeGhostMovement), 5.0f); // 5초 후 고스트 이동 복구
    }

    // 3초 후 플레이어 이동 복구
    private void EnablePlayerMovement()
    {
        if (player != null)
        {
            player.moveSpeed = player.originalMoveSpeed;
            player.dashSpeed = player.originalDashSpeed;
            Debug.Log("[Ghost] 3초 후 플레이어 이동 재개!");
        }
    }

    // 5초 후 고스트 이동 복구
    private void ResumeGhostMovement()
    {
        isPaused = false;
        moveSpeed = baseMoveSpeed;
        Debug.Log("[Ghost] 5초 후 고스트 이동 재개!");
    }
}
