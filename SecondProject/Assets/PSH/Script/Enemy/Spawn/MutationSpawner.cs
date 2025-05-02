using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MutationSpawner : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] private List<Transform> spawnPoints; // �� ���� ��ġ ����Ʈ
    [SerializeField] private LayerMask playerLayer; // �÷��̾� ������ ���� ���̾� ����ũ
    [SerializeField] private LayerMask enemyLayer; // �� ������ ���� ���̾� ����ũ
    [SerializeField] private List<Transform> maps; // �� ������Ʈ ����Ʈ (�� ����Ʈ)
    [SerializeField] private GameObject[] enemyPrefabs; // ��ȯ�� �� �ִ� �� ������ ���
    [SerializeField] private int maxEnemyCount; // �ִ� �� �� (��ȯ ����)
    [SerializeField] private float spawnInterval; // �� ��ȯ �ֱ� (��)
    [SerializeField] private float spawnTimer; // �ð� ������ ���� Ÿ�̸�

    private void Start()
    {
        // ���� ����Ʈ�� ������ ���� ���
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("[MutationSpawner] ���� ����Ʈ�� �������� �ʾҽ��ϴ�!");
        }

        // Ÿ�̸� �ʱ�ȭ
        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime; // �����Ӹ��� Ÿ�̸� ����

        if (spawnTimer <= 0f)
        {
            TrySpawn(); // ��ȯ �õ�
            spawnTimer = spawnInterval; // Ÿ�̸� ����
        }
    }

    // �� ��ȯ �õ� (���ǿ� �´� ��쿡��)
    private void TrySpawn()
    {
        int totalEnemyCount = GetTotalEnemyCount(); // ��ü �� �� Ȯ��
        Debug.Log($"[MutationSpawner] TrySpawn ȣ��� - ���� �� ��: {totalEnemyCount}");

        if (totalEnemyCount >= maxEnemyCount)
        {
            Debug.Log($"[MutationSpawner] �ش� ���� ���� {maxEnemyCount}�� �̻��̶� ��ȯ���� �ʽ��ϴ�.");
            return;
        }

        if (totalEnemyCount < maxEnemyCount)
        {
            List<Transform> emptyMaps = GetEmptyMaps(); // �÷��̾�/���� ��� ���� ��

            Debug.Log($"[MutationSpawner] ������ �� �� ��: {emptyMaps.Count}");

            if (emptyMaps.Count == 0)
            {
                Debug.Log("[MutationSpawner] �� ���� ���� ��ȯ���� �ʽ��ϴ�.");
                return;
            }

            // ���� �� �� �ϳ� ����
            Transform selectedMap = emptyMaps[Random.Range(0, emptyMaps.Count)];
            Debug.Log($"[MutationSpawner] ���õ� �� ��: {selectedMap.name}");

            SpawnEnemyNearMap(selectedMap);
        }
    }


    // Ư�� �� �ֺ��� ��ȿ�� ���� ����Ʈ���� ���� ��ȯ
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

        Debug.Log($"[MutationSpawner] '{map.name}' �ֺ� ��ȿ�� ���� ����Ʈ ��: {availablePoints.Count}");

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning($"[MutationSpawner] '{map.name}' �ֺ��� ��ȿ�� ���� ����Ʈ�� �����ϴ�.");
            return;
        }

        Transform spawnPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        Debug.Log($"[MutationSpawner] ���� ��ȯ��: {enemy.name} at {spawnPoint.position}");
    }


    // 1�� ��ü ���ʹ� �� ��ȯ
    private int GetTotalEnemyCount()
    {
        int count = 0;
        foreach (var map in maps)
        {
            count += GetEnemyCountInMap(map);
        }
        return count;
    }

    // Ư�� �� �ȿ� �ִ� ���ʹ� �� ��ȯ
    private int GetEnemyCountInMap(Transform map)
    {
        BoxCollider2D box = map.GetComponent<BoxCollider2D>();
        if (box == null) return 0;

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, enemyLayer);
        return colliders.Length;
    }

    // �÷��̾�� ���ʹ̰� ��� ���� �� �� ����Ʈ ��ȯ
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

    // �ش� �ʿ� �÷��̾ �ִ��� Ȯ��
    private bool IsPlayerInMap(Transform map)
    {
        BoxCollider2D box = map.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning($"[MutationSpawner] '{map.name}'�� BoxCollider2D�� �����ϴ�.");
            return false;
        }

        Vector2 center = box.bounds.center;
        Vector2 size = box.bounds.size;

        Collider2D[] colliders = Physics2D.OverlapBoxAll(center, size, 0f, playerLayer);
        return colliders.Length > 0;
    }

    // ����׿�: ���� ����Ʈ �� �� ���� �ð�ȭ
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
