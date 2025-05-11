using UnityEngine;
using UnityEngine.UIElements;

public class Ghost : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float chaseSpeed;
    [SerializeField] private float shortDetectRange;
    [SerializeField] private float longDetectRange;
    [SerializeField] private float chaseDetectRange;

    [Header("레이어 설정")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleAndPlayerLayer;
    [SerializeField] private LayerMask targetObjectLayer;

    [Header("유도 오브젝트")]
    [SerializeField] private float lureDetectRange = 20f;
    private Vector2? lureTargetPos = null;
    private GameObject lureTargetObject = null;


    [Header("사운드 이름 설정")]
    [SerializeField] private string hitPlayerSound = "ghost_hit";
    [SerializeField] private AudioSource[] idleSources;
    [SerializeField] private AudioSource[] chaseSources;

    [Header("데미지")]
    [SerializeField] private float damage;

    private Rigidbody2D rb;
    private Player player;
    private Animator anim;
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
        anim = GetComponent<Animator>();
        player = FindFirstObjectByType<Player>();
        ghostRoomChase = GetComponent<GhostRoomChase>();

        StopAudioGroup(chaseSources);
        PlayAudioGroup(idleSources);

        baseDetectRange = shortDetectRange;
        baseMoveSpeed = moveSpeed;
    }

    private void Update()
    {
        if (isPaused || player == null) return;

        HandleBreathingSound();

        DetectPlayerSide();
        DetectPlayerShortRange();
        HandleChaseTimer();
        Move();
        WallCheck();
    }

    private void HandleBreathingSound()
    {
        float distance = Vector2.Distance(transform.position, player.transform.position);
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

                    if (anim != null)
                    {
                        anim.SetBool("Tracking", true);
                        anim.SetBool("Idle", false);
                    }

                    StopAudioGroup(idleSources);
                    PlayAudioGroup(chaseSources);

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

                if (anim != null)
                {
                    anim.SetBool("Tracking", false);
                    anim.SetBool("Idle", true);

                    StopAudioGroup(chaseSources);
                    PlayAudioGroup(idleSources);    
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
            anim.SetBool("Grab",true);
            anim.SetBool("Tracking", false);
            anim.SetBool("Idle", false);

            StopAudioGroup(chaseSources);
            StopAudioGroup(idleSources);

            AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);


            Debug.Log("[Ghost] 플레이어와 추적 중 트리거 충돌 발생!");
            rb.linearVelocity = Vector2.zero;
            isPaused = true;
            isChasing = false;


            Player pl = collision.GetComponent<Player>();
            if (pl != null)
            {
                pl.isHiding = true;
                pl.isInteractionLocked = true;
                pl.ResetHold();
                pl.TakeDamage(damage);
                pl.TakeGrab();
                Invoke(nameof(ResumePlayerControl), 3.0f);
            }

            Invoke(nameof(ResumeGhostMovement), 5.0f);
        }
    }

    private void ResumePlayerControl()
    {
        if (player != null)
        {
            player.isHiding = false;
            player.isInteractionLocked = false;
            player.AfterGrab();
            anim.SetBool("Grab", false);
            anim.SetBool("Idle", true);
            anim.SetBool("Tracking", false);
        }
    }

    private void ResumeGhostMovement()
    {
        isPaused = false;
        moveSpeed = baseMoveSpeed;

        PlayAudioGroup(idleSources);

        if (anim != null)
        {
            anim.SetBool("Grab", false);
            anim.SetBool("Idle", true);
            anim.SetBool("Tracking", false);
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


