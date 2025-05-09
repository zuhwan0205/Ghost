using UnityEngine;
using System.Collections;

public class Mannequin : MonoBehaviour
{
    [Header("마네킹 상태")]
    [SerializeField] private bool isActive = true; // 마네킹이 활성 상태인지 여부
    [SerializeField] private bool isGrabbed = false; // 플레이어를 잡았는지 여부

    [Header("마네킹 기믹")]
    private int escapeCount = 0; // E 키 누른 횟수
    private int escapeRequiredCount = 15; // 탈출에 필요한 E 키 입력 횟수

    [Header("플레이어 데미지 설정")]
    [SerializeField] private float damageInterval = 1.0f; // 데미지를 주는 간격
    [SerializeField] private float damageAmount = 1.0f; // 한번에 주는 데미지량

    [Header("마네킹 초기화 설정")]
    [SerializeField] private float resetTime = 10.0f; // 초기화까지 걸리는 시간

    private Player player;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Player_Grabbed();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[마네킹 충돌 감지] 충돌한 객체 태그: {other.tag}");

        if (isActive && !isGrabbed && other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();

            if (player != null)
            {
                isGrabbed = true;

                player.isHiding = true; // 이동 불가

                // 플레이어에게 지속 데미지 시작
                StartCoroutine(DamagePlayer());

                Debug.Log("마네킹이 플레이어를 잡았습니다!");
            }
            else
            {
                Debug.LogWarning("Player 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }

    // 플레이어를 잡았을 때 호출되는 함수
    private void Player_Grabbed()
    {
        if (isGrabbed && player != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                escapeCount++;
                Debug.Log($"E 키 입력 횟수: {escapeCount}/{escapeRequiredCount}");

                if (escapeCount >= escapeRequiredCount)
                {
                    EscapeFromMannequin();
                }
            }
        }
    }

    // 플레이어가 마네킹에서 벗어났을 때 호출되는 함수
    private void EscapeFromMannequin()
    {
        Debug.Log("플레이어가 마네킹에서 벗어났습니다!");

        isGrabbed = false;
        isActive = false;

        if (player != null)
        {
            player.isHiding = false; // 이동 가능
        }

        // 마네킹 Collider 비활성화
        GetComponent<Collider2D>().enabled = false;

        // 데미지 코루틴 중단
        StopAllCoroutines();

        // 일정 시간 후 다시 활성화
        StartCoroutine(ResetMannequin(resetTime));
    }

    // 플레이어가 데미지를 입는 함수
    private IEnumerator DamagePlayer()
    {
        while (isGrabbed && player != null)
        {
            player.TakeDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    // 마네킹을 다시 활성화시키는 함수
    private IEnumerator ResetMannequin(float delay)
    {
        yield return new WaitForSeconds(delay);

        isActive = true;
        isGrabbed = false;
        escapeCount = 0;
        GetComponent<Collider2D>().enabled = true;

        Debug.Log("마네킹이 다시 활성화되었습니다!");
    }
}
