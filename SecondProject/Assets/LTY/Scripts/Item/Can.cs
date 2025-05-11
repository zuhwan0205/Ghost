using UnityEngine;

public class Can : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGrounded = false;
    [SerializeField] private string landSound = "can_land"; // 깡통 착지 사운드

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[Can] Rigidbody2D가 없습니다!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !isGrounded)
        {
            isGrounded = true;
            rb.linearVelocity = Vector2.zero; // 땅에 닿으면 정지
            rb.isKinematic = true; // 물리 충돌 비활성화
            AudioManager.Instance.PlayAt(landSound, transform.position); // 착지 사운드
            Debug.Log("[Can] 깡통이 땅에 닿음");

            // 깡통이 속한 방의 roomID 찾기
            RoomIdentifier room = null;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, LayerMask.GetMask("Room"));
            foreach (var hit in hits)
            {
                room = hit.GetComponent<RoomIdentifier>();
                if (room != null) break;
            }

            if (room != null)
            {
                Debug.Log($"[Can] 깡통이 속한 방: {room.roomID}");
                // EventMonsterChase 몬스터 호출
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
                Debug.LogWarning("[Can] 방 정보를 찾을 수 없음");
            }
        }
    }

    // 디버그용 감지 범위 시각화
    private void OnDrawGizmos()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f); // 방 감지 범위
        }
    }
}