using UnityEngine;
using System.Collections;

public class MonsterTelePort : MonoBehaviour
{
    [Header("텔레포트 설정")]
    public GameObject teleportDestination; // 이동할 목적지 포탈
    public GameObject destinationRoom;      // 이동할 목적지 방 (다음 맵 오브젝트)

    [Header("설정값")]
    [SerializeField] private float teleportChance = 1f; // 텔레포트 확률 (0~1)
    [SerializeField] private LayerMask enemyLayer;        // 적 레이어 마스크
    [SerializeField] private float teleportDelay = 1f;     // 텔레포트 지연 시간 (초)

    private Mutation mutationScript; // 몬스터 AI 스크립트 참조
    private Renderer rend; // 몬스터 렌더러
    private bool playerTeleportedRecently = false; // 플레이어가 최근 포탈을 탔는지 여부
    private bool teleportCooldown = false; // 텔레포트 직후 쿨다운 상태

    [Header("사운드 설정")]
    [SerializeField] private AudioSource[] teleportSources;


    private void Start()
    {
        // Mutation 스크립트 가져오기
        mutationScript = GetComponent<Mutation>();
        if (mutationScript == null)
            Debug.LogError("[MonsterTelePort] Mutation 스크립트를 찾을 수 없습니다!");

        // 렌더러 가져오기
        rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 쿨다운 중이면 무시
        if (teleportCooldown) return;

        // Door 태그가 아니면 무시
        if (!collision.CompareTag("Door")) return;

        // 부딪힌 포탈 오브젝트에서 TelePort 스크립트 가져오기
        TelePort portal = collision.GetComponent<TelePort>();
        if (portal == null)
        {
            Debug.LogWarning("[MonsterTelePort] 부딪힌 오브젝트에 TelePort 스크립트가 없습니다.");
            return;
        }

        // 부딪힌 포탈의 목적지와 방 정보 읽기
        teleportDestination = portal.teleport;
        destinationRoom = portal.box2;

        if (teleportDestination == null || destinationRoom == null)
        {
            Debug.LogWarning("[MonsterTelePort] 포탈 설정이 완전하지 않습니다.");
            return;
        }

        // 추격 상태이면 무조건 텔레포트
        if (mutationScript.IsChasingPlayer() && playerTeleportedRecently)
        {
            StartCoroutine(TeleportWithDelay(teleportDelay));
            return;
        }

        // 일반 상태일 때 확률적 텔레포트 시도
        if (!mutationScript.IsChasingPlayer())
        {
            if (RoomHasEnemy(destinationRoom))
            {
                Debug.Log("[MonsterTelePort] 다음 방에 몬스터가 있어서 텔레포트 취소.");
                return;
            }

            float randomValue = Random.Range(0f, 1f);
            if (randomValue <= teleportChance)
            {
                StartCoroutine(TeleportWithDelay(teleportDelay));
            }
            else
            {
                Debug.Log("[MonsterTelePort] 확률 실패로 텔레포트 취소.");
            }
        }
    }

    // 1초 동안 사라졌다가 텔레포트하는 코루틴
    private IEnumerator TeleportWithDelay(float delay)
    {
        // 몬스터 렌더러 끄기 (사라지게)
        if (rend != null)
            rend.enabled = false;

        yield return new WaitForSeconds(delay);

        // 텔레포트 수행
        Teleport();

        // 몬스터 렌더러 다시 켜기 (등장하게)
        if (rend != null)
            rend.enabled = true;

        // 텔레포트 직후 쿨다운 설정
        teleportCooldown = true;
        yield return new WaitForSeconds(1.0f);
        teleportCooldown = false;
    }

    // 실제 텔레포트 수행
    private void Teleport()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("[MonsterTelePort] teleportDestination이 설정되지 않았습니다!");
            return;
        }

        transform.position = teleportDestination.transform.position;

        PlayAudioGroup(teleportSources);

        Debug.Log("[MonsterTelePort] 몬스터가 텔레포트되었습니다!");
    }

    // 목적지 방에 다른 몬스터가 있는지 검사
    private bool RoomHasEnemy(GameObject room)
    {
        if (room == null) return false;

        Collider2D box = room.GetComponent<Collider2D>();
        if (box == null)
        {
            Debug.LogWarning("[MonsterTelePort] Destination Room에 Collider2D가 없습니다.");
            return false;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(box.bounds.center, box.bounds.size, 0f, enemyLayer);
        return enemies.Length > 0;
    }

    // 플레이어가 포탈을 탔을 때 호출되는 함수
    public void NotifyPlayerTeleported()
    {
        playerTeleportedRecently = true;

        // 3초 후 자동 초기화
        Invoke(nameof(ResetPlayerTeleportedFlag), 3f);
    }

    // 플레이어 포탈 알림 초기화
    private void ResetPlayerTeleportedFlag()
    {
        playerTeleportedRecently = false;
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
