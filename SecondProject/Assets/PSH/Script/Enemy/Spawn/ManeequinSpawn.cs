using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform player;

    [Header("몬스터 소환조건")]
    [SerializeField] float spawnRange = 20f;
    [SerializeField] float safeZone = 8f;
    private float despawnTime = 6f;
    private float currentRespawnTime = 20f;

    [Header("스폰 난이도 설정")]
    [SerializeField] private int spawnLevel = 1; // 1~3
    public LayerMask wallLayer;

    private GameObject currentMonster;
    private Coroutine checkVisibilityCoroutine;
    private CinemachineCamera virtualCamera;


    private float respawnCountdown = 0f;
    private bool isRespawning = false;

    void Start()
    {
        virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera를 찾을 수 없습니다.");
            return;
        }

        ApplySpawnLevel(); // 시작 시 현재 레벨 기반 시간 적용
        StartCoroutine(InitialSpawnDelay());
    }

    // 첫 스폰만 30초 지연 후 루프 시작
    IEnumerator InitialSpawnDelay()
    {
        Debug.Log("[MannequinSpawn] 첫 스폰까지 30초 대기");
        yield return new WaitForSeconds(30f);
        StartCoroutine(MonsterCycle());
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

            // 몬스터가 사라진 후 타이머 시작
            isRespawning = true;
            respawnCountdown = currentRespawnTime;

            while (respawnCountdown > 0f)
            {
                respawnCountdown -= Time.deltaTime;
                yield return null;
            }

            isRespawning = false;
        }
    }

    void OnGUI()
    {
        if (isRespawning)
        {
            string text = $"[Mannequin] 다음 소환까지: {respawnCountdown:F1}초";

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 20;
            style.normal.textColor = Color.red;

            float width = 350f;
            float height = 30f;
            float x = Screen.width - width - 10f;
            float y = 10f;

            GUI.Label(new Rect(x, y, width, height), text, style);
        }
    }

    // 외부에서 난이도 설정할 수 있는 함수
    public void SetSpawnLevel(int level)
    {
        spawnLevel = Mathf.Clamp(level, 1, 3);
        ApplySpawnLevel();
        Debug.Log($"[MannequinSpawn] 스폰 레벨 {spawnLevel} → 주기 {currentRespawnTime}초로 설정됨");
    }

    // 레벨에 따라 주기 적용
    private void ApplySpawnLevel()
    {
        currentRespawnTime = spawnLevel switch
        {
            1 => 20f,
            2 => 15f,
            3 => 10f,
            _ => 20f
        };
    }

    // 플레이어 방향 기준 소환 위치 계산
    Vector3 CalculateSpawnPosition()
    {
        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null)
        {
            Debug.LogWarning("Player 컴포넌트를 찾지 못했습니다.");
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
                spawnY += 1f;
        }
        else
        {
            float minSpawnX = player.position.x + playerDir * safeZone;
            float maxSpawnX = player.position.x + playerDir * spawnRange;
            spawnX = Random.Range(minSpawnX, maxSpawnX);
        }

        return new Vector3(spawnX, spawnY, 0f);
    }

    // 카메라에서 보이지 않으면 제거
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

    // 디버그용: Scene 뷰에서 소환 범위 표시
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null) return;

        float dir = playerComp.facingRight ? 1f : -1f;
        Vector3 pos = player.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.right * spawnRange * dir);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * safeZone * dir);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, 0.2f);
    }
}
