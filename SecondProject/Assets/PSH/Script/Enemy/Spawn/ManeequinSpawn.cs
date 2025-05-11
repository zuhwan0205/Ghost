using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    public GameObject monsterPrefab;         // 생성할 몬스터 프리팹
    public Transform player;                 // 플레이어 Transform

    [Header("몬스터 소환조건")]
    [SerializeField] float spawnRange = 20f;     // 소환 최대 거리
    [SerializeField] float safeZone = 8f;        // 플레이어 주변 소환 금지 구간
    [SerializeField] private float despawnTime = 6f;
    [SerializeField] private float RespawnTime = 20f;
    public LayerMask wallLayer;

    private GameObject currentMonster;
    private Coroutine checkVisibilityCoroutine;
    private CinemachineCamera virtualCamera;

    void Start()
    {
        virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera를 찾을 수 없습니다.");
            return;
        }

        StartCoroutine(MonsterCycle());
    }

    void Update()
    {
        // G 키로 강제 테스트 소환
        if (Input.GetKeyDown(KeyCode.G) && currentMonster == null)
        {
            SpawnMonsterOnce();
        }
    }

    // 몬스터 생성-소멸 반복 루프
    IEnumerator MonsterCycle()
    {
        while (true)
        {
            Vector3 spawnPos = CalculateSpawnPosition();
            currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            Debug.Log("[자동소환] 몬스터 소환 위치: " + spawnPos);

            checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());

            while (currentMonster != null)
                yield return null;

            yield return new WaitForSeconds(RespawnTime);
        }
    }

    // G 키로 수동 소환
    void SpawnMonsterOnce()
    {
        Vector3 spawnPos = CalculateSpawnPosition();
        currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Debug.Log("[G 키] 몬스터 강제 소환 위치: " + spawnPos);

        checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());
    }

    // 플레이어 방향 기준 소환 위치 계산
    Vector3 CalculateSpawnPosition()
    {
        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null)
        {
            Debug.LogWarning("Player 컴포넌트를 찾을 수 없습니다!");
            return player.position;
        }

        float playerDir = playerComp.facingRight ? 1f : -1f;
        Vector2 origin = player.position;
        Vector2 direction = playerDir > 0 ? Vector2.right : Vector2.left;
        float maxDistance = spawnRange;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallLayer);

        float spawnX;
        float spawnY = player.position.y;

        if (hit.collider != null)
        {
            float xOffset = (playerDir > 0) ? -1f : -0.5f;
            float maxSpawnX = hit.point.x + xOffset;
            float minSpawnX = player.position.x + playerDir * safeZone;

            spawnX = Mathf.Clamp(
                Random.Range(minSpawnX, maxSpawnX),
                Mathf.Min(minSpawnX, maxSpawnX),
                Mathf.Max(minSpawnX, maxSpawnX));

            if (playerDir > 0)
                spawnY += 1f; // 오른쪽일 때 Y 보정
        }
        else
        {
            float minSpawnX = player.position.x + playerDir * safeZone;
            float maxSpawnX = player.position.x + playerDir * spawnRange;
            spawnX = Random.Range(minSpawnX, maxSpawnX);
        }

        return new Vector3(spawnX, spawnY, 0f);
    }


    // 카메라에서 안 보일 경우 디스폰
    IEnumerator CheckVisibilityAndDespawn()
    {
        float invisibleTime = 0f;
        Renderer renderer = currentMonster.GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            Debug.LogWarning("Renderer 없음. 강제 디스폰 처리.");
            yield return new WaitForSeconds(despawnTime);
            Destroy(currentMonster);
            yield break;
        }

        while (currentMonster != null)
        {
            if (!renderer.isVisible)
            {
                invisibleTime += Time.deltaTime;
                if (invisibleTime >= 10f)
                {
                    Destroy(currentMonster);
                    Debug.Log("몬스터 디스폰됨 (10초 이상 안 보임)");
                    yield break;
                }
            }
            else
            {
                invisibleTime = 0f;
            }

            yield return null;
        }
    }

    // Scene 뷰에서 레이 시각화
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null) return;

        float dir = playerComp.facingRight ? 1f : -1f;
        Vector3 pos = player.position;

        // 전체 소환 가능 범위
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.right * spawnRange * dir);

        // 소환 금지 범위
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * safeZone * dir);

        // 기준점
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, 0.2f);
    }
}
