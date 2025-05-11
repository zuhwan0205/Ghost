using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    public GameObject monsterPrefab;         // ������ ���� ������
    public Transform player;                 // �÷��̾� Transform

    [Header("���� ��ȯ����")]
    [SerializeField] float spawnRange = 20f;     // ��ȯ �ִ� �Ÿ�
    [SerializeField] float safeZone = 8f;        // �÷��̾� �ֺ� ��ȯ ���� ����
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
            Debug.LogError("Cinemachine Virtual Camera�� ã�� �� �����ϴ�.");
            return;
        }

        StartCoroutine(MonsterCycle());
    }

    void Update()
    {
        // G Ű�� ���� �׽�Ʈ ��ȯ
        if (Input.GetKeyDown(KeyCode.G) && currentMonster == null)
        {
            SpawnMonsterOnce();
        }
    }

    // ���� ����-�Ҹ� �ݺ� ����
    IEnumerator MonsterCycle()
    {
        while (true)
        {
            Vector3 spawnPos = CalculateSpawnPosition();
            currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            Debug.Log("[�ڵ���ȯ] ���� ��ȯ ��ġ: " + spawnPos);

            checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());

            while (currentMonster != null)
                yield return null;

            yield return new WaitForSeconds(RespawnTime);
        }
    }

    // G Ű�� ���� ��ȯ
    void SpawnMonsterOnce()
    {
        Vector3 spawnPos = CalculateSpawnPosition();
        currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
        Debug.Log("[G Ű] ���� ���� ��ȯ ��ġ: " + spawnPos);

        checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());
    }

    // �÷��̾� ���� ���� ��ȯ ��ġ ���
    Vector3 CalculateSpawnPosition()
    {
        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null)
        {
            Debug.LogWarning("Player ������Ʈ�� ã�� �� �����ϴ�!");
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
                spawnY += 1f; // �������� �� Y ����
        }
        else
        {
            float minSpawnX = player.position.x + playerDir * safeZone;
            float maxSpawnX = player.position.x + playerDir * spawnRange;
            spawnX = Random.Range(minSpawnX, maxSpawnX);
        }

        return new Vector3(spawnX, spawnY, 0f);
    }


    // ī�޶󿡼� �� ���� ��� ����
    IEnumerator CheckVisibilityAndDespawn()
    {
        float invisibleTime = 0f;
        Renderer renderer = currentMonster.GetComponentInChildren<Renderer>();

        if (renderer == null)
        {
            Debug.LogWarning("Renderer ����. ���� ���� ó��.");
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
                    Debug.Log("���� ������ (10�� �̻� �� ����)");
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

    // Scene �信�� ���� �ð�ȭ
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null) return;

        float dir = playerComp.facingRight ? 1f : -1f;
        Vector3 pos = player.position;

        // ��ü ��ȯ ���� ����
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.right * spawnRange * dir);

        // ��ȯ ���� ����
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * safeZone * dir);

        // ������
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(pos, 0.2f);
    }
}
