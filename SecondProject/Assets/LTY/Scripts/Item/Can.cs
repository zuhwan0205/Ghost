using UnityEngine;

public class Can : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isGrounded = false;
    [SerializeField] private string landSound = "can_land"; // 깡통 착지 사운드
    [SerializeField] private float lureDetectRange = 20f; // 몬스터가 반응할 최대 거리

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

            // 깡통 착지 위치
            Vector2 landingPos = transform.position;

            // Mutation 몬스터들에게 깡통 위치 전달 (근처 몬스터만)
            Mutation[] mutations = FindObjectsByType<Mutation>(FindObjectsSortMode.None);
            foreach (var mutation in mutations)
            {
                if (mutation != null)
                {
                    float distance = Vector2.Distance(landingPos, mutation.transform.position);
                    if (distance <= lureDetectRange)
                    {
                        mutation.SetLureTarget(landingPos, gameObject);
                        Debug.Log($"[Can] Mutation 몬스터 호출: {mutation.gameObject.name}, 목표 위치: {landingPos}, 거리: {distance}");
                    }
                    else
                    {
                        Debug.Log($"[Can] Mutation 몬스터 {mutation.gameObject.name} 너무 멀리 있음, 거리: {distance}");
                    }
                }
            }

            // 기존 EventMonsterChase 호출 (유지)
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, lureDetectRange); // 몬스터 반응 범위
        }
    }
}