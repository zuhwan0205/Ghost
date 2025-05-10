using UnityEngine;
using System.Collections;

public class Mannequin : MonoBehaviour
{
    [Header("����ŷ ����")]
    [SerializeField] private bool isActive = true; // ����ŷ�� Ȱ�� �������� ����
    [SerializeField] private bool isGrabbed = false; // �÷��̾ ��Ҵ��� ����

    [Header("����ŷ ���")]
    private int escapeCount = 0; // E Ű ���� Ƚ��
    private int escapeRequiredCount = 15; // Ż�⿡ �ʿ��� E Ű �Է� Ƚ��

    [Header("�÷��̾� ������ ����")]
    [SerializeField] private float damageInterval = 1.0f; // �������� �ִ� ����
    [SerializeField] private float damageAmount = 1.0f; // �ѹ��� �ִ� ��������

    [Header("����ŷ �ʱ�ȭ ����")]
    [SerializeField] private float resetTime = 10.0f; // �ʱ�ȭ���� �ɸ��� �ð�

    private Player player;
    private Rigidbody2D rb;
    
    public static Mannequin Instance;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // private void Update()
    // {
    //     Player_Grabbed();
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[����ŷ �浹 ����] �浹�� ��ü �±�: {other.tag}");

        if (isActive && !isGrabbed && other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();

            if (player != null)
            {
                isGrabbed = true;

                player.isHiding = true; // �̵� �Ұ�

                // �÷��̾�� ���� ������ ����
                StartCoroutine(DamagePlayer());

                if (Random.value < 0.5f)
                {
                    QTEManager_LJH.Instance.StartSmashQte();
                }
                else
                {
                    QTEManager_LJH.Instance.StartStarForceQte();
                }
                
                Debug.Log("����ŷ�� �÷��̾ ��ҽ��ϴ�!");
            }
            else
            {
                Debug.LogWarning("Player ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
    }

    // �÷��̾ ����� �� ȣ��Ǵ� �Լ�
    // private void Player_Grabbed()
    // {
    //     if (isGrabbed && player != null)
    //     {
    //         if (Input.GetKeyDown(KeyCode.E))
    //         {
    //             escapeCount++;
    //             Debug.Log($"E Ű �Է� Ƚ��: {escapeCount}/{escapeRequiredCount}");
    //
    //             if (escapeCount >= escapeRequiredCount)
    //             {
    //                 EscapeFromMannequin();
    //             }
    //         }
    //     }
    // }

    // �÷��̾ ����ŷ���� ����� �� ȣ��Ǵ� �Լ�
    public void EscapeFromMannequin()
    {
        Debug.Log("�÷��̾ ����ŷ���� ������ϴ�!");

        isGrabbed = false;
        isActive = false;

        if (player != null)
        {
            player.isHiding = false; // �̵� ����
        }

        // ����ŷ Collider ��Ȱ��ȭ
        GetComponent<Collider2D>().enabled = false;

        // ������ �ڷ�ƾ �ߴ�
        StopAllCoroutines();

        // ���� �ð� �� �ٽ� Ȱ��ȭ
        StartCoroutine(ResetMannequin(resetTime));
    }

    // �÷��̾ �������� �Դ� �Լ�
    private IEnumerator DamagePlayer()
    {
        while (isGrabbed && player != null)
        {
            player.TakeDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
        }
    }

    // ����ŷ�� �ٽ� Ȱ��ȭ��Ű�� �Լ�
    private IEnumerator ResetMannequin(float delay)
    {
        yield return new WaitForSeconds(delay);

        isActive = true;
        isGrabbed = false;
        escapeCount = 0;
        GetComponent<Collider2D>().enabled = true;

        Debug.Log("����ŷ�� �ٽ� Ȱ��ȭ�Ǿ����ϴ�!");
    }
}
