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
    [SerializeField] private LayerMask targetObjectLayer;

    [Header("유도 오브젝트")]
    [SerializeField] private float lureDetectRange = 20f;
    private Vector2? lureTargetPos = null;
    private GameObject lureTargetObject = null;

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


        DetectLureObject();

        // 타겟이 있으면 그쪽으로만 이동하고 다른 행동은 모두 중지
        if (lureTargetPos.HasValue)
        {
            rb.linearVelocity = Vector2.zero;
            ChaseToLure(lureTargetPos.Value);
            return;
        }

        DetectPlayerSide();
        DetectPlayerShortRange();
        HandleChaseTimer();
        Move();
        WallCheck();
    }

    // 유도 오브젝트 좌우 감지 + 거리 비교로 더 가까운 쪽 선택
    private void DetectLureObject()
    {
        Vector2 origin = transform.position;

        RaycastHit2D leftHit = Physics2D.Raycast(origin, Vector2.left, lureDetectRange, targetObjectLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(origin, Vector2.right, lureDetectRange, targetObjectLayer);

        bool foundLeft = leftHit.collider != null;
        bool foundRight = rightHit.collider != null;

        if (!foundLeft && !foundRight)
        {
            lureTargetPos = null;
            lureTargetObject = null;
            return;
        }

        if (foundLeft && !foundRight)
        {
            lureTargetPos = leftHit.point;
            lureTargetObject = leftHit.collider.gameObject;
            Debug.Log("[Ghost] 좌측 유도 오브젝트 감지 → 이동");
        }
        else if (!foundLeft && foundRight)
        {
            lureTargetPos = rightHit.point;
            lureTargetObject = rightHit.collider.gameObject;
            Debug.Log("[Ghost] 우측 유도 오브젝트 감지 → 이동");
        }
        else
        {
            float distLeft = Vector2.Distance(origin, leftHit.point);
            float distRight = Vector2.Distance(origin, rightHit.point);

            if (distLeft <= distRight)
            {
                lureTargetPos = leftHit.point;
                lureTargetObject = leftHit.collider.gameObject;
                Debug.Log("[Ghost] 좌측(가까움) 유도 오브젝트 선택");
            }
            else
            {
                lureTargetPos = rightHit.point;
                lureTargetObject = rightHit.collider.gameObject;
                Debug.Log("[Ghost] 우측(가까움) 유도 오브젝트 선택");
            }
        }

        isChasing = false;
        isPaused = false;
    }

    // 유도 오브젝트 위치로 이동
    private void ChaseToLure(Vector2 targetPos)
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if ((targetPos.x < transform.position.x && transform.localScale.x > 0) ||
            (targetPos.x > transform.position.x && transform.localScale.x < 0))
        {
            Flip();
        }
    }

    private void DetectPlayerSide()
    {
        if (!player.DidMoveOrInteract) return;

        Vector2 origin = transform.position;
        bool leftOnlyWall = false;
        bool rightOnlyWall = false;

        RaycastHit2D leftHit = Physics2D.Raycast(origin, Vector2.left, longDetectRange, obstacleAndPlayerLayer);
        Debug.DrawRay(origin, Vector2.left * longDetectRange, Color.green);
        if (leftHit.collider != null)
        {
            if (leftHit.collider.CompareTag("Wall")) leftOnlyWall = true;
            else if (leftHit.collider.CompareTag("Player") && transform.localScale.x > 0) Flip();
        }

        RaycastHit2D rightHit = Physics2D.Raycast(origin, Vector2.right, longDetectRange, obstacleAndPlayerLayer);
        Debug.DrawRay(origin, Vector2.right * longDetectRange, Color.green);
        if (rightHit.collider != null)
        {
            if (rightHit.collider.CompareTag("Wall")) rightOnlyWall = true;
            else if (rightHit.collider.CompareTag("Player") && transform.localScale.x < 0) Flip();
        }

        if (leftOnlyWall && rightOnlyWall)
        {
            Debug.Log("[Ghost] 양쪽 모두 벽 감지됨");
            ghostRoomChase?.TryChaseOnBlocked();
        }
    }

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
            else isPlayerInRange = false;
        }
        else isPlayerInRange = false;
    }

    private void HandleChaseTimer()
    {
        if (isChasing && !isPlayerInRange)
        {
            chaseTimer += Time.deltaTime;
            if (chaseTimer >= chaseDuration)
            {
                isChasing = false;
                moveSpeed = baseMoveSpeed;
                chaseTimer = 0f;
            }
        }
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveSpeed * transform.localScale.x, rb.linearVelocity.y);
    }

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

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isChasing)
        {
            Debug.Log("[Ghost] 플레이어와 추적 중 트리거 충돌 발생!");
            OnPlayerTrigger(collision);
            return;
        }

        if (((1 << collision.gameObject.layer) & targetObjectLayer) != 0)
        {
            Debug.Log("[Ghost] 유도 오브젝트와 충돌 → 오브젝트 제거 및 5초 정지");
            Destroy(collision.gameObject);
            lureTargetPos = null;
            lureTargetObject = null;

            rb.linearVelocity = Vector2.zero;
            isPaused = true;

            Invoke(nameof(ResumeGhostMovement), 5.0f);
        }
    }

    private void OnPlayerTrigger(Collider2D collision)
    {
        rb.linearVelocity = Vector2.zero;
        isPaused = true;
        isChasing = false;

        Player player = collision.GetComponent<Player>();
        if (player != null)
        {
            player.moveSpeed = 0f;
            player.dashSpeed = 0f;
            Invoke(nameof(EnablePlayerMovement), 3.0f);
        }

        Invoke(nameof(ResumeGhostMovement), 5.0f);
    }

    private void EnablePlayerMovement()
    {
        if (player != null)
        {
            player.moveSpeed = player.originalMoveSpeed;
            player.dashSpeed = player.originalDashSpeed;
            Debug.Log("[Ghost] 3초 후 플레이어 이동 재개!");
        }
    }

    private void ResumeGhostMovement()
    {
        isPaused = false;
        moveSpeed = baseMoveSpeed;
        Debug.Log("[Ghost] 5초 후 고스트 이동 재개!");
    }
}
