using UnityEngine;

public class Can : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGrounded = false;
    [SerializeField] private string landSound = "can_land"; // ���� ���� ����

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[Can] Rigidbody2D�� �����ϴ�!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isGrounded)
        {
            isGrounded = true;
            rb.linearVelocity = Vector2.zero; // ���� ������ ����
            rb.isKinematic = true; // ���� �浹 ��Ȱ��ȭ
            AudioManager.Instance.PlayAt(landSound, transform.position); // ���� ����
            Debug.Log("[Can] ������ ���� ����");

            // ������ ���� ���� roomID ã��
            RoomIdentifier room = null;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("Room"));
            foreach (var hit in hits)
            {
                room = hit.GetComponent<RoomIdentifier>();
                if (room != null) break;
            }

            if (room != null)
            {
                Debug.Log($"[Can] ������ ���� ��: {room.roomID}");
                // EventMonsterChase ���� ȣ��
                foreach (var monster in EventMonsterChase.allMonsters)
                {
                    if (monster != null)
                    {
                        monster.TriggerMoveToRoom(room.roomID);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[Can] �� ������ ã�� �� ����");
            }
        }
    }

    // ����׿� ���� ���� �ð�ȭ
    private void OnDrawGizmos()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f); // �� ���� ����
        }
    }
}