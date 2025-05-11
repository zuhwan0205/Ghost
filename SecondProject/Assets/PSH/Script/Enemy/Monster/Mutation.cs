using UnityEngine;
using System.Collections;

public class Mutation : MonoBehaviour
{
    [Header("플레이어 관련")]
    private Transform player; // 플레이어 Transform
    private Rigidbody2D rb; // 몬스터 Rigidbody2D
    private Renderer rend; // 몬스터 Renderer
    private Animator anim; // 몬스터 Animator

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

    [Header("사운드 설정")]
    [SerializeField] private string hitPlayerSound = "mutation_hit";
    [SerializeField] private string lureHitSound = "can_hit";
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private AudioSource[] walkingSources;
    [SerializeField] private AudioSource[] chasingSources;

    [Header("데미지")]
    [SerializeField] private float damage;

    private Vector3 originalScale;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponent<Renderer>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null)
            Debug.LogError("[Mutation] Player 태그를 찾을 수 없습니다!");

        ResetToDefaultStats();

        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (isPaused) return;

        // 1. 깡통(유도 오브젝트) 추적 중이면
        if (lureTargetPos.HasValue)
        {
            ChaseToLure(lureTargetPos.Value);
            return; // 플레이어 감지 및 기타 로직 완전 무시
        }

        // 2. 깡통이 없으면 유도 오브젝트 감지 시도
        DetectLureObject();

        // 3. 감지에 성공했다면 다음 프레임부터 이동
        if (lureTargetPos.HasValue)
            return;

        // 4. 깡통/유도 오브젝트 없으면 플레이어 추적 또는 순찰
        PlayerTracking();
        CheckForStuck();
    }

    private void LateUpdate()
    {
        // Flip 방향은 유지하면서 Y, Z는 원본 유지
        float direction = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(originalScale.x * direction, originalScale.y, originalScale.z);
    }

    // 깡통 위치를 외부에서 설정하는 메서드
    public void SetLureTarget(Vector2 targetPos, GameObject targetObject)
    {
        lureTargetPos = targetPos;
        lureTargetObject = targetObject;
        isChasing = false; // 플레이어 추적 중지
        isPaused = false;  // 멈춤 상태 해제
        Debug.Log($"[Mutation] 깡통 목표 설정: {targetPos}, 오브젝트: {targetObject.name}");
    }

    private void DetectLureObject()
    {
        Vector2 origin = transform.position;

        RaycastHit2D leftHit = Physics2D.Raycast(origin, Vector2.left, lureDetectRange, targetObjectLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(origin, Vector2.right, lureDetectRange, targetObjectLayer);

        bool foundLeft = leftHit.collider != null;
        bool foundRight = rightHit.collider != null;

        if (!foundLeft && !foundRight)
        {
            // 깡통이 이미 목표로 설정된 경우 유지
            return;
        }

        if (foundLeft && !foundRight)
        {
            lureTargetPos = leftHit.point;
            lureTargetObject = leftHit.collider.gameObject;
        }
        else if (!foundLeft && foundRight)
        {
            lureTargetPos = rightHit.point;
            lureTargetObject = rightHit.collider.gameObject;
        }
        else
        {
            float distLeft = Vector2.Distance(origin, leftHit.point);
            float distRight = Vector2.Distance(origin, rightHit.point);

            if (distLeft <= distRight)
            {
                lureTargetPos = leftHit.point;
                lureTargetObject = leftHit.collider.gameObject;
            }
            else
            {
                lureTargetPos = rightHit.point;
                lureTargetObject = rightHit.collider.gameObject;
            }
        }

        isChasing = false;
        isPaused = false;
        Debug.Log($"[Mutation] 유도 오브젝트 감지: {lureTargetObject.name}, 위치: {lureTargetPos}");
    }

    private void ChaseToLure(Vector2 targetPos)
    {
        // 물리 기반 이동
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * Enemy_Move_Speed, rb.linearVelocity.y);

        // 벽 충돌 체크
        if (WallAhead())
        {
            Flip();
            rb.linearVelocity = new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y);
            Debug.Log("[Mutation] 벽 감지: 방향 전환");
        }

        // 방향 전환
        if ((targetPos.x < transform.position.x && transform.localScale.x > 0) ||
            (targetPos.x > transform.position.x && transform.localScale.x < 0))
        {
            Flip();
        }

        // 애니메이션 및 사운드
        if (anim != null)
        {
            anim.SetBool("Tracking", true);
            anim.SetBool("Idle", false);
            PlayAudioGroup(chasingSources);
            StopAudioGroup(walkingSources);
        }
    }

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

            if (anim != null)
            {
                anim.SetBool("Tracking", true);
                anim.SetBool("Idle", false);
                PlayAudioGroup(chasingSources);
                StopAudioGroup(walkingSources);
            }
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

    private void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDirection * is_Enemy_Move_Speed, rb.linearVelocity.y);

        if (WallAhead())
        {
            Flip();
            patrolDirection *= -1;
        }
    }

    private void MoveToTarget(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position);
        if (direction.magnitude < 0.2f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        direction.Normalize();
        rb.linearVelocity = new Vector2(direction.x * is_Enemy_Move_Speed, rb.linearVelocity.y);
    }

    private void LookAt(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, 1, 1);
    }

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

    private Vector2 GetFacingDirection()
    {
        return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
    }

    private bool WallAhead()
    {
        float checkDistance = 0.5f;
        Vector2 direction = GetFacingDirection();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, LayerMask.GetMask("Wall"));
        Debug.DrawRay(transform.position, direction * checkDistance, Color.red);

        return hit.collider != null;
    }

    private void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("[Mutation] 플레이어와 트리거 충돌 발생!");
            OnPlayerTrigger(collision);
        }

        if (((1 << collision.gameObject.layer) & targetObjectLayer) != 0)
        {
            Debug.Log($"[Mutation] 유도 오브젝트와 충돌: {collision.gameObject.name} → 제거 및 5초 정지");
            AudioManager.Instance.PlayAt(lureHitSound, transform.position);
            Destroy(collision.gameObject);
            lureTargetPos = null;
            lureTargetObject = null;

            rb.linearVelocity = Vector2.zero;
            isPaused = true;

            if (anim != null)
            {
                anim.SetBool("Tracking", false);
                anim.SetBool("Idle", true);
            }

            Invoke(nameof(ResumeMutationMovement), 5.0f);
        }
    }

    private void OnPlayerTrigger(Collider2D collision)
    {
        anim.SetBool("Grab", true);
        anim.SetBool("Tracking", false);
        anim.SetBool("Idle", false);

        rb.linearVelocity = Vector2.zero;
        isChasing = false;
        isPaused = true;

        StopAudioGroup(audioSources);
        StopAudioGroup(chasingSources);
        StopAudioGroup(walkingSources);

        AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);

        Player pl = collision.GetComponent<Player>();
        if (pl != null)
        {
            pl.isHiding = true; // 이동 불가
            pl.isInteractionLocked = true;
            pl.ResetHold();
            pl.TakeDamage(damage); // 데미지 처리
            pl.TakeGrab(); // Grab 처리
            Invoke(nameof(EnablePlayer), 3.0f);
        }

        Invoke(nameof(ResumeMutationMovement), 5.0f);
    }

    private void EnablePlayer()
    {
        if (player != null)
        {
            Player pl = player.GetComponent<Player>();
            if (pl != null)
            {
                Debug.Log("[Mutation] 3초 후 플레이어 이동 재개!");
                pl.isHiding = false;
                pl.isInteractionLocked = false;
                pl.AfterGrab(); // Grab 해제
                anim.SetBool("Grab", false);
                anim.SetBool("Tracking", false);
                anim.SetBool("Idle", true);
            }
        }
    }

    private void ResumeMutationMovement()
    {
        isPaused = false;
        isPatrolling = true;
        isChasing = false;
        ResetToDefaultStats();

        PlayAudioGroup(audioSources);

        if (anim != null)
        {
            anim.SetBool("Grab", false);
            anim.SetBool("Tracking", false);
            anim.SetBool("Idle", true);
        }

        Debug.Log("[Mutation] 5초 후 몬스터 이동 재개!");
    }

    private IEnumerator WaitThenReturnToPatrol()
    {
        isChasing = false;
        isWaitingAfterChase = true;
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
        {
            anim.SetBool("Tracking", false);
            anim.SetBool("Idle", true);
        }

        yield return new WaitForSeconds(2f);

        isWaitingAfterChase = false;
        isPatrolling = true;
        ResetToDefaultStats();
    }

    private void ResetToDefaultStats()
    {
        is_Enemy_AwareDist = Enemy_AwareDist;
        is_Enemy_Move_Speed = Enemy_Move_Speed;
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Enemy_AwareDist); // 감지 반경

        Gizmos.color = Color.red;
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, direction * is_Enemy_AwareDist);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, lureDetectRange); // 유도 오브젝트 감지 범위
    }

    public bool IsChasingPlayer()
    {
        return isChasing;
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