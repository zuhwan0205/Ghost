using UnityEngine;
using System.Collections;

public class Mannequin : MonoBehaviour
{
    [Header("마네킹 상태")]
    [SerializeField] private bool isActive = true; // 마네킹이 플레어를 붙잡을 수 있는 활성상태 
    [SerializeField] private bool isGrabbed = false; // 현재 플레이어를 잡고 있는지 여부 

    [Header("데미지")]
    [SerializeField] private float damage;

    [Header("재활성화 설정 ")]
    [SerializeField] private float resetTime = 10.0f; // 일정 시간 후 다시 활성화되는 시간


    [Header("사운드")]
    [SerializeField] private string hitPlayerSound = "mannequin_hit";
    [SerializeField] private AudioSource[] SpawnSources;


    private Player player;
    private Rigidbody2D rb;
    private Animator anim;
    
    public static Mannequin Instance;

    void Awake()
    {
        Instance = this;

        PlayAudioGroup(SpawnSources);

    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (isActive && !isGrabbed && other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();

            if (player != null)
            {
                isGrabbed = true;

                anim.SetBool("Idle", false); //
                anim.SetBool("Grab", true); // 잡기 애니메이션 

                AudioManager.Instance.PlayAt(hitPlayerSound, transform.position);
                LookAt(player.transform.position);


                player.isHiding = true;


                if (Random.value < 0.5f)
                {
                    QTEManager_LJH.Instance.StartSmashQte();
                    player.isHiding = true;
                    player.isInteractionLocked = true;
                    player.TakeGrab();
                    player.TakeDamage(damage);
                    player.ResetHold();

                }
                else
                {
                    QTEManager_LJH.Instance.StartStarForceQte();
                    player.isHiding = true;
                    player.isInteractionLocked = true;
                    player.TakeGrab();
                    player.TakeDamage(damage);
                    player.ResetHold();
                }

                Debug.Log("마네킹이 플레이어를 붙잡았습니다!");
            }
            else
            {
                Debug.LogWarning("Player 컴포넌트를 찾지 못했습니다.");
            }
        }
    }

    public void EscapeFromMannequin()
    {
        Debug.Log("플레이어가 마네킹으로부터 탈출했습니다!");

        isGrabbed = false;
        isActive = false;
        player.isHiding = false;
        player.isInteractionLocked = false;
        player.AfterGrab();

        anim.SetBool("Grab", false);
        anim.SetBool("Idle", true);



        // 마네킹 충돌 비활성화
        GetComponent<Collider2D>().enabled = false;

        // 모든 코루틴 종료
        StopAllCoroutines();

        // 일정 시간 후 다시 활성화
        StartCoroutine(ResetMannequin(resetTime));
    }

    // 마네킹을 다시 활성화시키는 코루틴
    private IEnumerator ResetMannequin(float delay)
    {
        yield return new WaitForSeconds(delay);

        isActive = true;
        isGrabbed = false;
        GetComponent<Collider2D>().enabled = true;
    }

    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
            if (source != null && !source.isPlaying)
                source.Play();
    }

    private void LookAt(Vector3 target)
    {
        Vector3 scale = transform.localScale;
        float originalX = Mathf.Abs(scale.x);
        scale.x = (target.x < transform.position.x) ? originalX : -originalX;
        transform.localScale = scale;
    }
}

