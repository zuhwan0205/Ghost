using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MannequinSpawn : MonoBehaviour
{
    // ������ ���� ������
    public GameObject monsterPrefab;
    // �÷��̾��� Transform - ���� ��ġ ����
    public Transform player;

    [Header("���� ��ȯ����")]
    [SerializeField] float spawnRange = 8f;   // �¿� �ִ� �Ÿ�
    [SerializeField] float safeZone = 1f;     // ��ȯ ���� �Ÿ�
    [SerializeField] private float despawnTime = 6f;    // ���� ���� �ð� (������ ���� ���)
    [SerializeField] private float RespawnTime = 20f;   // ���� �� ����� ��� �ð�
    public LayerMask wallLayer;     // �� ���� ���̾�

    private GameObject currentMonster;                  // ���� �����ϴ� ���� �ν��Ͻ�
    private Coroutine checkVisibilityCoroutine;         // ���� �þ� üũ�� �ڷ�ƾ
    private CinemachineCamera virtualCamera;            // Cinemachine ���� ī�޶�


    void Start()
    {
        // ���� �����ϴ� Cinemachine ī�޶� ã��
        virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera�� ã�� �� �����ϴ�.");
            return;
        }



        // ���� ����-�Ҹ� ����Ŭ ����
        StartCoroutine(MonsterCycle());
    }

    // ���� ������ ������ �ݺ��ϴ� ����
    IEnumerator MonsterCycle()
    {
        while (true)
        {
            // ���� ��ġ ���
            Vector3 spawnPos = CalculateSpawnPosition();

            // ���� ����
            currentMonster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            Debug.Log("���� ������ ��ġ: " + spawnPos);

            // ������ ���� ��� ���� ���� ����
            checkVisibilityCoroutine = StartCoroutine(CheckVisibilityAndDespawn());

            // ���Ͱ� �����ϴ� ���� ���
            while (currentMonster != null)
                yield return null;

            // ���� �� ��������� ��� �ð�
            yield return new WaitForSeconds(RespawnTime);
        }
    }

    // ���� ���� ��ġ�� ����ϴ� �Լ�
    Vector3 CalculateSpawnPosition()
    {
        // ���� ��ȯ ���� ����: �÷��̾� ����
        float leftMin = player.position.x - spawnRange;
        float leftMax = player.position.x - safeZone;

        // ������ ��ȯ ���� ����: �÷��̾� ����
        float rightMin = player.position.x + safeZone;
        float rightMax = player.position.x + spawnRange;

        // �� ������ ���� ����
        leftMin = CheckWallInRange2D(leftMin, leftMax);
        rightMax = CheckWallInRange2D(rightMax, rightMin);

        float spawnX;

        // �¿� ���� ��ȿ�� üũ
        bool canSpawnLeft = leftMin < leftMax;
        bool canSpawnRight = rightMin < rightMax;

        //���� �� �����ϸ� ���� ����
        if (canSpawnLeft && canSpawnRight)
        {
            spawnX = (Random.value < 0.5f)
                ? Random.Range(leftMin, leftMax)
                : Random.Range(rightMin, rightMax);
        }
        else if (canSpawnLeft)
        {
            spawnX = Random.Range(leftMin, leftMax);
        }
        else if (canSpawnRight)
        {
            spawnX = Random.Range(rightMin, rightMax);
        }
        else
        {
            Debug.LogWarning("��ȯ ��ġ�� �����ϴ�. fallback ��ġ ���");
            spawnX = player.position.x; // �⺻ fallback ��ġ
        }

        float spawnY = player.position.y;           // Y�� �÷��̾� �������� ����
        float spawnZ = 0f;                           // 2D������ ���� Z�� 0���� ����

        return new Vector3(spawnX, spawnY, spawnZ);
    }


    // ��ȯ ��ġ���� ���� �ִ��� Ȯ���Ͽ� ��ġ�� �����ϴ� �Լ�
    float CheckWallInRange2D(float currentLimit, float targetLimit)
    {
        // Raycast ���� ����
        Vector2 direction = (targetLimit - currentLimit) > 0 ? Vector2.right : Vector2.left;

        // Raycast �Ÿ�
        float distance = Mathf.Abs(currentLimit - targetLimit);

        // Ray ���� ��ġ
        Vector2 rayOrigin = new Vector2(targetLimit, player.position.y);

        // ���� Raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, distance, wallLayer);

        // �浹�� ���� �ִٸ�, �ش� ������ ��¦ ���� �������� ��ġ ����
        if (hit.collider != null)
        {
            // �� �ٷ� �տ��� ���ߵ��� ����
            return hit.point.x - (0.1f * direction.x);
        }

        // �浹�� ���ٸ� ���� ���� ��ġ ����
        return currentLimit;
    }

    // ���Ͱ� ī�޶� ������ ������ �����ϴ� ����
    IEnumerator CheckVisibilityAndDespawn()
    {
        float invisibleTime = 0f;

        // �ڽ� ���� Renderer ã��
        Renderer renderer = currentMonster.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning("Renderer ����. ���� ���� ó��.");
            yield return new WaitForSeconds(despawnTime);
            Destroy(currentMonster);
            yield break;
        }

        // ���Ͱ� �����ϴ� ���� �ݺ� üũ
        while (currentMonster != null)
        {
            if (!renderer.isVisible) // ī�޶� �� ���̸�
            {
                invisibleTime += Time.deltaTime;

                if (invisibleTime >= 10f) // 10�� �̻� �� ���̸� ����
                {
                    Destroy(currentMonster);
                    Debug.Log("���� ������ (10�� �̻� �� ����)");
                    yield break;
                }
            }
            else
            {
                // ���̸� �ð� �ʱ�ȭ
                invisibleTime = 0f;
            }

            yield return null;
        }
    }
}
