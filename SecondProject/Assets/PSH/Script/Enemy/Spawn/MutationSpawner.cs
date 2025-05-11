using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Floor
{
    public string floorName;
    public List<Transform> rooms;            // �� ����Ʈ (BoxCollider2D ����)
    public Transform detectionArea;          // �÷��̾� ������ ���� BoxCollider2D
}

public class MutationSpawner : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private List<Transform> spawnPoints;       // ��ü ���� ����Ʈ
    [SerializeField] private GameObject[] enemyPrefabs;         // ��ȯ�� �� ������ ���
    [SerializeField] private List<Floor> floors;                // �� ����Ʈ
    [SerializeField] private LayerMask playerLayer;             // �÷��̾� ���� ���̾�
    [SerializeField] private LayerMask enemyLayer;              // ���� ���� ���̾�
    [SerializeField] private float spawnInterval = 10f;         // ��ȯ �ֱ�

    [Header("���� ���̵� ����")]
    [SerializeField] private int spawnLevel = 1; // 1~3 (�ܺο��� ����)
    private int maxEnemyCount = 3;              // �ڵ� ������

    private float spawnTimer;

    private void Start()
    {
        ApplySpawnLevel();
        spawnTimer = spawnInterval;

        if (spawnPoints.Count == 0)
            Debug.LogError("[MutationSpawner] ���� ����Ʈ�� �����ϴ�!");
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
            Debug.Log($"[Spawner] ��ü ���� ���� �ִ�ġ({maxEnemyCount})�Դϴ�.");
            return;
        }

        foreach (var floor in floors)
        {
            if (IsPlayerInFloor(floor))
            {
                Debug.Log($"[Spawner] {floor.floorName}�� �÷��̾ �־� ��ŵ�մϴ�.");
                continue;
            }

            List<Transform> emptyRooms = GetEmptyRooms(floor);
            if (emptyRooms.Count == 0)
            {
                Debug.Log($"[Spawner] {floor.floorName}�� �� ���� �����ϴ�.");
                continue;
            }

            Transform selectedRoom = emptyRooms[Random.Range(0, emptyRooms.Count)];
            SpawnEnemyInRoom(selectedRoom);
            break; // �� ���� �����ϰ� ����
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
            Debug.LogWarning($"[Spawner] '{room.name}' ��ó�� ���� ����Ʈ�� �����ϴ�.");
            return;
        }

        Transform spawnPoint = validPoints[Random.Range(0, validPoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"[Spawner] ���� ��ȯ��: {enemyPrefab.name} at {spawnPoint.position}");
    }

    // �ܺο��� ���� ������ �����ϴ� �Լ�
    public void SetSpawnLevel(int level)
    {
        spawnLevel = Mathf.Clamp(level, 1, 3);
        ApplySpawnLevel();
        Debug.Log($"[Spawner] ���� {spawnLevel} �� �ִ� ���� �� {maxEnemyCount} ������");
    }

    // ������ ���� �ִ� ���� �� ����
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

    // ���� ����Ʈ/�� ����� �ð�ȭ
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
