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

    [Header("오디오 설정")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip chaseClip;
    [SerializeField] private float audioRange = 20f;
    private bool isPlayingChaseSound = false;

    [Header("숨소리 설정")]
    [SerializeField] private AudioClip breathingClip;
    [SerializeField] private float breathingRange = 20f;
    private AudioSource breathingSource;

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

        InitAudio();
        InitBreathing();
    }

    private void InitAudio()
    {
        if (audioSource != null && idleClip != null)
        {
            audioSource.clip = idleClip;
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;         // 3D 사운드
            audioSource.minDistance = 1f;
            audioSource.maxDistance = audioRange;
            audioSource.rolloffMode = AudioRolloffMode.Custom;

            // 거리 감쇠 곡선 설정
            AnimationCurve rolloffCurve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(audioRange * 0.5f, 0.5f),
                new Keyframe(audioRange, 0f)
            );
            audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloffCurve);

            audioSource.Play();
        }
    }

    private void InitBreathing()
    {
        breathingSource = gameObject.AddComponent<AudioSource>();
        breathingSource.clip = breathingClip;
        breathingSource.loop = true;
        breathingSource.playOnAwake = false;
        breathingSource.spatialBlend = 1f;
        breathingSource.minDistance = 1f;
        breathingSource.maxDistance = breathingRange;
        breathingSource.rolloffMode = AudioRolloffMode.Logarithmic;

        AnimationCurve curve = new AnimationCurve(
            new Keyframe(breathingRange, 1f),
            new Keyframe(breathingRange, 0.5f),
            new Keyframe(breathingRange, 0f)
        );
        breathingSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve);
    }

    private void Update()
    {
        if (isPaused || player == null) return;

        HandleBreathingSound();

        DetectLureObject();

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

    private void HandleBreathingSound()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= breathingRange)
        {
            if (!breathingSource.isPlaying)
                breathingSource.Play();
        }
        else
        {
            if (breathingSource.isPlaying)
                breathingSource.Stop();
        }
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
            lureTargetPos = null;
            lureTargetObject = null;
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
    }

    private void ChaseToLure(Vector2 targetPos)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

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
        if (leftHit.collider != null)
        {
            if (leftHit.collider.CompareTag("Wall")) leftOnlyWall = true;
            else if (leftHit.collider.CompareTag("Player") && transform.localScale.x > 0) Flip();
        }

        RaycastHit2D rightHit = Physics2D.Raycast(origin, Vector2.right, longDetectRange, obstacleAndPlayerLayer);
        if (rightHit.collider != null)
        {
            if (rightHit.collider.CompareTag("Wall")) rightOnlyWall = true;
            else if (rightHit.collider.CompareTag("Player") && transform.localScale.x < 0) Flip();
        }

        if (leftOnlyWall && rightOnlyWall)
        {
            ghostRoomChase?.TryChaseOnBlocked();
        }
    }

    private void DetectPlayerShortRange()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float currentDetectRange = isChasing ? chaseDetectRange : shortDetectRange;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, currentDetectRange, obstacleAndPlayerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (player.DidMoveOrInteract)
            {
                if (!isChasing)
                {
                    isChasing = true;
                    moveSpeed = chaseSpeed;

                    if (audioSource != null && chaseClip != null && !isPlayingChaseSound)
                    {
                        audioSource.Stop();
                        audioSource.clip = chaseClip;
                        audioSource.loop = true;
                        audioSource.Play();
                        isPlayingChaseSound = true;
                    }
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

                if (audioSource != null && idleClip != null)
                {
                    audioSource.Stop();
                    audioSource.clip = idleClip;
                    audioSource.loop = true;
                    audioSource.Play();
                    isPlayingChaseSound = false;
                }
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
            rb.linearVelocity = Vector2.zero;
            isPaused = true;
            isChasing = false;

            Player pl = collision.GetComponent<Player>();
            if (pl != null)
            {
                pl.moveSpeed = 0f;
                pl.dashSpeed = 0f;
                pl.isInteractionLocked = true;
                pl.ResetHold();
                Invoke(nameof(ResumePlayerControl), 3.0f);
            }

            Invoke(nameof(ResumeGhostMovement), 5.0f);
        }
    }

    private void ResumePlayerControl()
    {
        if (player != null)
        {
            player.moveSpeed = player.originalMoveSpeed;
            player.dashSpeed = player.originalDashSpeed;
            player.isInteractionLocked = false;
        }
    }

    private void ResumeGhostMovement()
    {
        isPaused = false;
        moveSpeed = baseMoveSpeed;
    }
}
