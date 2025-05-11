using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Floor
{
    public string floorName;
    public List<Transform> rooms;            // 방 리스트 (BoxCollider2D 포함)
    public Transform detectionArea;          // 플레이어 감지를 위한 BoxCollider2D
}

public class MutationSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private List<Transform> spawnPoints;       // 전체 스폰 포인트
    [SerializeField] private GameObject[] enemyPrefabs;         // 소환할 적 프리팹 목록
    [SerializeField] private List<Floor> floors;                // 층 리스트
    [SerializeField] private LayerMask playerLayer;             // 플레이어 감지 레이어
    [SerializeField] private LayerMask enemyLayer;              // 몬스터 감지 레이어
    [SerializeField] private float spawnInterval = 10f;         // 소환 주기

    [Header("스폰 난이도 설정")]
    [SerializeField] private int spawnLevel = 1; // 1~3 (외부에서 설정)
    private int maxEnemyCount = 3;              // 자동 설정됨

    private float spawnTimer;

    private void Start()
    {
        ApplySpawnLevel();
        spawnTimer = spawnInterval;

        if (spawnPoints.Count == 0)
            Debug.LogError("[MutationSpawner] 스폰 포인트가 없습니다!");
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawn();
            spawnTimer = spawnInterval;
        }
    }

    private void TrySpawn()
    {
        int totalEnemyCount = GetTotalEnemyCountGlobal();

        if (totalEnemyCount >= maxEnemyCount)
        {
            Debug.Log($"[Spawner] 전체 몬스터 수가 최대치({maxEnemyCount})입니다.");
            return;
        }

        foreach (var floor in floors)
        {
            if (IsPlayerInFloor(floor))
            {
                Debug.Log($"[Spawner] {floor.floorName}에 플레이어가 있어 스킵합니다.");
                continue;
            }

            List<Transform> emptyRooms = GetEmptyRooms(floor);
            if (emptyRooms.Count == 0)
            {
                Debug.Log($"[Spawner] {floor.floorName}에 빈 방이 없습니다.");
                continue;
            }

            Transform selectedRoom = emptyRooms[Random.Range(0, emptyRooms.Count)];
            SpawnEnemyInRoom(selectedRoom);
            break; // 한 번만 스폰하고 종료
        }
    }

    private int GetTotalEnemyCountGlobal()
    {
        int count = 0;
        foreach (var floor in floors)
        {
            foreach (var room in floor.rooms)
            {
                count += GetEnemyCountInRoom(room);
            }
        }
        return count;
    }

    private bool IsPlayerInFloor(Floor floor)
    {
        if (floor.detectionArea == null) return false;

        BoxCollider2D box = floor.detectionArea.GetComponent<BoxCollider2D>();
        if (box == null) return false;

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;

        return Physics2D.OverlapBox(center, size, 0f, playerLayer);
    }

    private List<Transform> GetEmptyRooms(Floor floor)
    {
        List<Transform> emptyRooms = new List<Transform>();

        foreach (var room in floor.rooms)
        {
            if (!IsPlayerInRoom(room) && GetEnemyCountInRoom(room) == 0)
            {
                emptyRooms.Add(room);
            }
        }

        return emptyRooms;
    }

    private bool IsPlayerInRoom(Transform room)
    {
        BoxCollider2D box = room.GetComponent<BoxCollider2D>();
        if (box == null) return false;

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;
        return Physics2D.OverlapBox(center, size, 0f, playerLayer);
    }

    private int GetEnemyCountInRoom(Transform room)
    {
        BoxCollider2D box = room.GetComponent<BoxCollider2D>();
        if (box == null) return 0;

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;
        Collider2D[] enemies = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);
        return enemies.Length;
    }

    private void SpawnEnemyInRoom(Transform room)
    {
        List<Transform> validPoints = new List<Transform>();
        foreach (var point in spawnPoints)
        {
            if (Vector2.Distance(point.position, room.position) < 30f)
            {
                validPoints.Add(point);
            }
        }

        if (validPoints.Count == 0)
        {
            Debug.LogWarning($"[Spawner] '{room.name}' 근처에 스폰 포인트가 없습니다.");
            return;
        }

        Transform spawnPoint = validPoints[Random.Range(0, validPoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"[Spawner] 몬스터 소환됨: {enemyPrefab.name} at {spawnPoint.position}");
    }

    // 외부에서 스폰 레벨을 설정하는 함수
    public void SetSpawnLevel(int level)
    {
        spawnLevel = Mathf.Clamp(level, 1, 3);
        ApplySpawnLevel();
        Debug.Log($"[Spawner] 레벨 {spawnLevel} → 최대 몬스터 수 {maxEnemyCount} 설정됨");
    }

    // 레벨에 따라 최대 마리 수 결정
    private void ApplySpawnLevel()
    {
        maxEnemyCount = spawnLevel switch
        {
            1 => 4,
            2 => 5,
            3 => 6,
            _ => 4
        };
    }

    // 스폰 포인트/방 디버그 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Transform point in spawnPoints)
        {
            Gizmos.DrawWireSphere(point.position, 1.5f);
        }

        Gizmos.color = Color.cyan;
        foreach (var floor in floors)
        {
            foreach (Transform room in floor.rooms)
            {
                var box = room.GetComponent<BoxCollider2D>();
                if (box != null)
                    Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }

            if (floor.detectionArea != null)
            {
                var det = floor.detectionArea.GetComponent<BoxCollider2D>();
                if (det != null)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                    Gizmos.DrawCube(det.bounds.center, det.bounds.size);
                }
            }
        }
    }
}
