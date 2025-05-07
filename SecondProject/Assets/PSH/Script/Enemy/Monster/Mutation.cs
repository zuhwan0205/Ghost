using UnityEngine;
using System.Collections;

public class Mutation : MonoBehaviour
{
    [Header("플레이어 관련")]
    private Transform player; // 플레이어 Transform
    private Rigidbody2D rb; // 몬스터 Rigidbody2D
    private Renderer rend; // 몬스터 Renderer

    [Header("몬스터 기본 속성")]
    [SerializeField] private float Enemy_Hp; // 몬스터 체력
    [SerializeField] private float Enemy_Move_Speed; // 기본 이동 속도
    [SerializeField] private float Enemy_AwareDist; // 기본 감지 거리
    private float is_Enemy_AwareDist; // 현재 감지 거리
    private float is_Enemy_Move_Speed; // 현재 이동 속도

    [Header("추격 설정")]
    [SerializeField] private float Enemy_Chasing_Speed; // 추격 시 추가 속도
    [SerializeField] private float Enemy_Chasing_AwareDis; // 추격 시 감지 거리 증가량

    [Header("감지 설정")]
    [SerializeField] private LayerMask playerLayer; // 플레이어 감지용 레이어마스크

    [Header("감지 쿨타임 설정")]
    [SerializeField] private float reDetectDelay; // 재감지 대기 시간
    private float detectCooldown = 1f; // 현재 쿨다운 시간
    private bool isDetectable = true; // 감지 가능 여부

    [Header("상태 관리")]
    private bool isChasing = false; // 추격 중 여부
    private bool isWaitingAfterChase = false; // 추격 후 대기 상태
    private bool isPatrolling = true; // 순찰 상태
    private bool isPaused = false; // 멈춤 상태 여부
    private int patrolDirection = 1; // 순찰 방향 (1: 오른쪽, -1: 왼쪽)
    private float idleTimer = 0f; // 가만히 있는 시간
    [SerializeField] private float idleThreshold = 5f; // 순찰 복귀 기준 시간
    private float lastXPosition; // 마지막 기록된 X 위치
    private Vector3 lastSeenPosition; // 마지막 감지한 플레이어 위치

    [Header("유도 오브젝트")]
    [SerializeField] private float lureDetectRange = 20f;
    [SerializeField] private LayerMask targetObjectLayer;
    private Vector2? lureTargetPos = null;
    private GameObject lureTargetObject = null;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponent<Renderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogError("[Mutation] Player 태그를 찾을 수 없습니다!");

        SetEnemyState(100f, 2f, 3.5f, 4f, 5.5f, 2f);
        ResetToDefaultStats();
    }

    private void Update()
    {
        if (isPaused) return;

        DetectLureObject(); // 유도 오브젝트 감지 우선 처리

        if (lureTargetPos.HasValue)
        {
            rb.linearVelocity = Vector2.zero;
            ChaseToLure(lureTargetPos.Value);
            return;
        }
        else
        {
            // 감지되지 않았다면 플레이어 추적 또는 순찰
            PlayerTracking();
        }

        CheckForStuck(); // 이동 중 멈춤 상태 확인
    }

    // 유도 오브젝트 좌우 감지
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
        transform.position = Vector2.MoveTowards(transform.position, targetPos, Enemy_Move_Speed * Time.deltaTime);

        if ((targetPos.x < transform.position.x && transform.localScale.x > 0) ||
            (targetPos.x > transform.position.x && transform.localScale.x < 0))
        {
            Flip();
        }
    }
    // 플레이어 추적 및 순찰 관리
    private void PlayerTracking()
    {
        if (player == null || isWaitingAfterChase) return;

        if (!isDetectable)
        {
            detectCooldown -= Time.deltaTime;
            if (detectCooldown <= 0f)
                isDetectable = true;
        }

        if (isDetectable && PlayerInSight())
        {
            isChasing = true;
            isPatrolling = false;
            lastSeenPosition = player.position;
            is_Enemy_AwareDist = Enemy_AwareDist + Enemy_Chasing_AwareDis;
            is_Enemy_Move_Speed = Enemy_Move_Speed + Enemy_Chasing_Speed;
        }

        if (isChasing)
        {
            float distance = Vector2.Distance(transform.position, lastSeenPosition);
            if (distance > 0.2f)
            {
                MoveToTarget(lastSeenPosition);
                LookAt(lastSeenPosition);
                if (WallAhead()) Flip();
            }
            else
            {
                StartCoroutine(WaitThenReturnToPatrol());
            }
        }
        else if (isPatrolling)
        {
            Patrol();
        }
    }

    // 순찰 이동
    private void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDirection * is_Enemy_Move_Speed, 0);

        if (WallAhead())
        {
            Flip();
            patrolDirection *= -1;
        }
    }

    // 목표 지점으로 이동
    private void MoveToTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position);
        if (direction.magnitude < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        direction.Normalize();
        rb.linearVelocity = direction * is_Enemy_Move_Speed;
    }

    // 목표 방향 바라보기
    private void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

    // 플레이어가 감지 범위 안에 있는지 확인
    public virtual bool PlayerInSight()
    {
        float circleRadius = is_Enemy_AwareDist;
        Vector2 origin = (Vector2)transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, circleRadius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.transform.root.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    // 바라보는 방향 반환
    private Vector2 GetFacingDirection()
    {
        return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    }

    // 벽 감지
    private bool WallAhead()
    {
        float checkDistance = 0.5f;
        Vector2 direction = GetFacingDirection();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, LayerMask.GetMask("Wall"));
        Debug.DrawRay(transform.position, direction * checkDistance, Color.red);

        return hit.collider != null;
    }

    // 방향 반전
    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 트리거 충돌 처리
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[Mutation] 플레이어와 트리거 충돌 발생!");
            OnPlayerTrigger(collision);
        }

        if (((1 << collision.gameObject.layer) & targetObjectLayer) != 0)
        {
            Debug.Log("[Ghost] 유도 오브젝트와 충돌 → 오브젝트 제거 및 5초 정지");
            Destroy(collision.gameObject);
            lureTargetPos = null;
            lureTargetObject = null;

            rb.linearVelocity = Vector2.zero;
            isPaused = true;

            Invoke(nameof(ResumeMutationMovement), 5.0f);
        }
    }

    // 플레이어와 충돌 시 처리
    private void OnPlayerTrigger(Collider2D collision)
    {
        rb.linearVelocity = Vector2.zero;
        isChasing = false;
        isPaused = true;
        isDetectable = false;
        detectCooldown = reDetectDelay;

        Player playerScript = collision.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.moveSpeed = 0f;
            playerScript.dashSpeed = 0f;
            Invoke(nameof(EnablePlayerMovement), 3.0f);
        }

        Invoke(nameof(ResumeMutationMovement), 5.0f);
    }

    // 플레이어 이동 복구
    private void EnablePlayerMovement()
    {
        if (player != null)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                Debug.Log("[Mutation] 3초 후 플레이어 이동 재개!");
                playerScript.moveSpeed = playerScript.originalMoveSpeed;
                playerScript.dashSpeed = playerScript.originalDashSpeed;
            }
        }
    }

    // 몬스터 이동 복구
    private void ResumeMutationMovement()
    {
        isPaused = false;
        isPatrolling = true;
        isChasing = false;
        ResetToDefaultStats();
        Debug.Log("[Mutation] 5초 후 몬스터 이동 재개!");
    }

    // 감지 후 대기 후 순찰 복귀
    private IEnumerator WaitThenReturnToPatrol()
    {
        isChasing = false;
        isWaitingAfterChase = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(2f);

        isWaitingAfterChase = false;
        isPatrolling = true;
        ResetToDefaultStats();
    }

    // 이동속도 및 감지 거리 초기화
    private void ResetToDefaultStats()
    {
        is_Enemy_AwareDist = Enemy_AwareDist;
        is_Enemy_Move_Speed = Enemy_Move_Speed;
    }

    // 기본 몬스터 스탯 세팅
    private void SetEnemyState(float _enemy_Hp, float _enemy_Speed, float _enemy_AwareDist, float _enemy_Chasing_Speed, float _enemy_Chasing_AwareDis, float _reDetectDelay)
    {
        Enemy_Hp = _enemy_Hp;
        Enemy_Move_Speed = _enemy_Speed;
        Enemy_AwareDist = _enemy_AwareDist;
        Enemy_Chasing_Speed = _enemy_Chasing_Speed;
        Enemy_Chasing_AwareDis = _enemy_Chasing_AwareDis;
        reDetectDelay = _reDetectDelay;
    }

    // 추적 중 멈춘 경우 체크
    private void CheckForStuck()
    {
        if (isChasing && !isPatrolling && !isWaitingAfterChase)
        {
            float currentX = transform.position.x;

            if (Mathf.Approximately(currentX, lastXPosition))
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleThreshold)
                {
                    isPatrolling = true;
                    isWaitingAfterChase = false;
                    isChasing = false;
                    ResetToDefaultStats();
                    idleTimer = 0f;
                }
            }
            else
            {
                idleTimer = 0f;
                lastXPosition = currentX;
            }
        }
        else
        {
            idleTimer = 0f;
            lastXPosition = transform.position.x;
        }
    }

    // 디버그용 감지 범위 및 벽 감지 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Enemy_AwareDist); // 감지 반경

        // 플레이어 감지 방향 디버그 선
        Gizmos.color = Color.red;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * is_Enemy_AwareDist); // ★ 감지거리만큼 길게!
    }

    //현재 몬스터가 플레이어를 추격중인지 반환 
    public bool IsChasingPlayer()
    {
        return isChasing;
    }
}