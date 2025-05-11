using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    public GameObject monsterPrefab;
    public Transform player;

    [Header("���� ��ȯ����")]
    [SerializeField] float spawnRange = 20f;
    [SerializeField] float safeZone = 8f;
    private float despawnTime = 6f;
    private float currentRespawnTime = 20f;

    [Header("���� ���̵� ����")]
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
            Debug.LogError("Cinemachine Virtual Camera�� ã�� �� �����ϴ�.");
            return;
        }

        ApplySpawnLevel(); // ���� �� ���� ���� ��� �ð� ����
        StartCoroutine(InitialSpawnDelay());
    }

    // ù ������ 30�� ���� �� ���� ����
    IEnumerator InitialSpawnDelay()
    {
        Debug.Log("[MannequinSpawn] ù �������� 30�� ���");
        yield return new WaitForSeconds(30f);
        StartCoroutine(MonsterCycle());
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

            // ���Ͱ� ����� �� Ÿ�̸� ����
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
            string text = $"[Mannequin] ���� ��ȯ����: {respawnCountdown:F1}��";

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

    // �ܺο��� ���̵� ������ �� �ִ� �Լ�
    public void SetSpawnLevel(int level)
    {
        spawnLevel = Mathf.Clamp(level, 1, 3);
        ApplySpawnLevel();
        Debug.Log($"[MannequinSpawn] ���� ���� {spawnLevel} �� �ֱ� {currentRespawnTime}�ʷ� ������");
    }

    // ������ ���� �ֱ� ����
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

    // �÷��̾� ���� ���� ��ȯ ��ġ ���
    Vector3 CalculateSpawnPosition()
    {
        Player playerComp = player.GetComponent<Player>();
        if (playerComp == null)
        {
            Debug.LogWarning("Player ������Ʈ�� ã�� ���߽��ϴ�.");
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

    // ī�޶󿡼� ������ ������ ����
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

    // ����׿�: Scene �信�� ��ȯ ���� ǥ��
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
