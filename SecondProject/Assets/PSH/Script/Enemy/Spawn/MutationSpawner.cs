using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MutationSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private List<Transform> spawnPoints; // 적 생성 위치 리스트
    [SerializeField] private LayerMask playerLayer; // 플레이어 감지를 위한 레이어 마스크
    [SerializeField] private LayerMask enemyLayer; // 적 감지를 위한 레이어 마스크
    [SerializeField] private List<Transform> maps; // 맵 오브젝트 리스트 (방 리스트)
    [SerializeField] private GameObject[] enemyPrefabs; // 소환할 수 있는 적 프리팹 목록
    [SerializeField] private int maxEnemyCount; // 최대 적 수 (소환 제한)
    [SerializeField] private float spawnInterval; // 적 소환 주기 (초)
    [SerializeField] private float spawnTimer; // 시간 측정을 위한 타이머

    private void Start()
    {
        // 스폰 포인트가 없으면 에러 출력
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("[MutationSpawner] 스폰 포인트가 지정되지 않았습니다!");
        }

        // 타이머 초기화
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime; // 프레임마다 타이머 감소

        if (spawnTimer <= 0f)
        {
            TrySpawn(); // 소환 시도
            spawnTimer = spawnInterval; // 타이머 리셋
        }
    }

    // 적 소환 시도 (조건에 맞는 경우에만)
    private void TrySpawn()
    {
        int totalEnemyCount = GetTotalEnemyCount(); // 전체 적 수 확인
        Debug.Log($"[MutationSpawner] TrySpawn 호출됨 - 현재 적 수: {totalEnemyCount}");

        if (totalEnemyCount >= maxEnemyCount)
        {
            Debug.Log($"[MutationSpawner] 해당 층의 적이 {maxEnemyCount}명 이상이라 소환하지 않습니다.");
            return;
        }

        if (totalEnemyCount < maxEnemyCount)
        {
            List<Transform> emptyMaps = GetEmptyMaps(); // 플레이어/적이 모두 없는 맵

            Debug.Log($"[MutationSpawner] 감지된 빈 맵 수: {emptyMaps.Count}");

            if (emptyMaps.Count == 0)
            {
                Debug.Log("[MutationSpawner] 빈 맵이 없어 소환하지 않습니다.");
                return;
            }

            // 랜덤 빈 맵 하나 선택
            Transform selectedMap = emptyMaps[Random.Range(0, emptyMaps.Count)];
            Debug.Log($"[MutationSpawner] 선택된 빈 맵: {selectedMap.name}");

            SpawnEnemyNearMap(selectedMap);
        }
    }


    // 특정 맵 주변의 유효한 스폰 포인트에서 적을 소환
    private void SpawnEnemyNearMap(Transform map)
    {
        List<Transform> availablePoints = new List<Transform>();

        foreach (var point in spawnPoints)
        {
            float distance = Vector2.Distance(map.position, point.position);
            if (distance < 30f)
            {
                availablePoints.Add(point);
            }
        }

        Debug.Log($"[MutationSpawner] '{map.name}' 주변 유효한 스폰 포인트 수: {availablePoints.Count}");

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning($"[MutationSpawner] '{map.name}' 주변에 유효한 스폰 포인트가 없습니다.");
            return;
        }

        Transform spawnPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"[MutationSpawner] 몬스터 소환됨: {enemy.name} at {spawnPoint.position}");
    }


    // 1층 전체 에너미 수 반환
    private int GetTotalEnemyCount()
    {
        int count = 0;
        foreach (var map in maps)
        {
            count += GetEnemyCountInMap(map);
        }
        return count;
    }

    // 특정 맵 안에 있는 에너미 수 반환
    private int GetEnemyCountInMap(Transform map)
    {
        BoxCollider2D box = map.GetComponent<BoxCollider2D>();
        if (box == null) return 0;

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);
        return colliders.Length;
    }

    // 플레이어와 에너미가 모두 없는 빈 맵 리스트 반환
    private List<Transform> GetEmptyMaps()
    {
        List<Transform> emptyMaps = new List<Transform>();

        foreach (var map in maps)
        {
            if (!IsPlayerInMap(map) && GetEnemyCountInMap(map) == 0)
            {
                emptyMaps.Add(map);
            }
        }

        return emptyMaps;
    }

    // 해당 맵에 플레이어가 있는지 확인
    private bool IsPlayerInMap(Transform map)
    {
        BoxCollider2D box = map.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning($"[MutationSpawner] '{map.name}'에 BoxCollider2D가 없습니다.");
            return false;
        }

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, playerLayer);
        return colliders.Length > 0;
    }

    // 디버그용: 스폰 포인트 및 맵 범위 시각화
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.green;
        foreach (Transform point in spawnPoints)
        {
            Gizmos.DrawWireSphere(point.position, 1.5f);
        }

        Gizmos.color = Color.cyan;
        if (maps != null)
        {
            foreach (Transform map in maps)
            {
                BoxCollider2D box = map.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
                }
            }
        }
    }
}
