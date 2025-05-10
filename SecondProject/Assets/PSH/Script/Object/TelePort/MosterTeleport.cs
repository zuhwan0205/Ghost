using UnityEngine;
using System.Collections;

public class MonsterTelePort : MonoBehaviour
{
    [Header("�ڷ���Ʈ ����")]
    public GameObject teleportDestination; // �̵��� ������ ��Ż
    public GameObject destinationRoom;      // �̵��� ������ �� (���� �� ������Ʈ)

    [Header("������")]
    [SerializeField] private float teleportChance = 1f; // �ڷ���Ʈ Ȯ�� (0~1)
    [SerializeField] private LayerMask enemyLayer;        // �� ���̾� ����ũ
    [SerializeField] private float teleportDelay = 1f;     // �ڷ���Ʈ ���� �ð� (��)

    private Mutation mutationScript; // ���� AI ��ũ��Ʈ ����
    private Renderer rend; // ���� ������
    private bool playerTeleportedRecently = false; // �÷��̾ �ֱ� ��Ż�� ������ ����
    private bool teleportCooldown = false; // �ڷ���Ʈ ���� ��ٿ� ����

    [Header("���� ����")]
    [SerializeField] private AudioSource[] teleportSources;


    private void Start()
    {
        // Mutation ��ũ��Ʈ ��������
        mutationScript = GetComponent<Mutation>();
        if (mutationScript == null)
            Debug.LogError("[MonsterTelePort] Mutation ��ũ��Ʈ�� ã�� �� �����ϴ�!");

        // ������ ��������
        rend = GetComponent<Renderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ��ٿ� ���̸� ����
        if (teleportCooldown) return;

        // Door �±װ� �ƴϸ� ����
        if (!collision.CompareTag("Door")) return;

        // �ε��� ��Ż ������Ʈ���� TelePort ��ũ��Ʈ ��������
        TelePort portal = collision.GetComponent<TelePort>();
        if (portal == null)
        {
            Debug.LogWarning("[MonsterTelePort] �ε��� ������Ʈ�� TelePort ��ũ��Ʈ�� �����ϴ�.");
            return;
        }

        // �ε��� ��Ż�� �������� �� ���� �б�
        teleportDestination = portal.teleport;
        destinationRoom = portal.box2;

        if (teleportDestination == null || destinationRoom == null)
        {
            Debug.LogWarning("[MonsterTelePort] ��Ż ������ �������� �ʽ��ϴ�.");
            return;
        }

        // �߰� �����̸� ������ �ڷ���Ʈ
        if (mutationScript.IsChasingPlayer() && playerTeleportedRecently)
        {
            StartCoroutine(TeleportWithDelay(teleportDelay));
            return;
        }

        // �Ϲ� ������ �� Ȯ���� �ڷ���Ʈ �õ�
        if (!mutationScript.IsChasingPlayer())
        {
            if (RoomHasEnemy(destinationRoom))
            {
                Debug.Log("[MonsterTelePort] ���� �濡 ���Ͱ� �־ �ڷ���Ʈ ���.");
                return;
            }

            float randomValue = Random.Range(0f, 1f);
            if (randomValue <= teleportChance)
            {
                StartCoroutine(TeleportWithDelay(teleportDelay));
            }
            else
            {
                Debug.Log("[MonsterTelePort] Ȯ�� ���з� �ڷ���Ʈ ���.");
            }
        }
    }

    // 1�� ���� ������ٰ� �ڷ���Ʈ�ϴ� �ڷ�ƾ
    private IEnumerator TeleportWithDelay(float delay)
    {
        // ���� ������ ���� (�������)
        if (rend != null)
            rend.enabled = false;

        yield return new WaitForSeconds(delay);

        // �ڷ���Ʈ ����
        Teleport();

        // ���� ������ �ٽ� �ѱ� (�����ϰ�)
        if (rend != null)
            rend.enabled = true;

        // �ڷ���Ʈ ���� ��ٿ� ����
        teleportCooldown = true;
        yield return new WaitForSeconds(1.0f);
        teleportCooldown = false;
    }

    // ���� �ڷ���Ʈ ����
    private void Teleport()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("[MonsterTelePort] teleportDestination�� �������� �ʾҽ��ϴ�!");
            return;
        }

        transform.position = teleportDestination.transform.position;

        PlayAudioGroup(teleportSources);

        Debug.Log("[MonsterTelePort] ���Ͱ� �ڷ���Ʈ�Ǿ����ϴ�!");
    }

    // ������ �濡 �ٸ� ���Ͱ� �ִ��� �˻�
    private bool RoomHasEnemy(GameObject room)
    {
        if (room == null) return false;

        Collider2D box = room.GetComponent<Collider2D>();
        if (box == null)
        {
            Debug.LogWarning("[MonsterTelePort] Destination Room�� Collider2D�� �����ϴ�.");
            return false;
        }

        Collider2D[] enemies = Physics2D.OverlapBoxAll(box.bounds.center, box.bounds.size, 0f, enemyLayer);
        return enemies.Length > 0;
    }

    // �÷��̾ ��Ż�� ���� �� ȣ��Ǵ� �Լ�
    public void NotifyPlayerTeleported()
    {
        playerTeleportedRecently = true;

        // 3�� �� �ڵ� �ʱ�ȭ
        Invoke(nameof(ResetPlayerTeleportedFlag), 3f);
    }

    // �÷��̾� ��Ż �˸� �ʱ�ȭ
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
