using UnityEngine;

public class PlayerRoomTracking : MonoBehaviour
{
    [Header("플레이어 방 인식 설정")]
    public string currentRoomID; // 현재 플레이어가 있는 방 ID

    // 방에 진입했을 때 호출 (기본적인 충돌 감지)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
        {
            RoomIdentifier room = collision.GetComponent<RoomIdentifier>();
            if (room != null)
            {
                currentRoomID = room.roomID;
                Debug.Log($"[PlayerRoom] 플레이어가 방에 진입: {currentRoomID}");
            }
            else
            {
                Debug.LogWarning("[PlayerRoom] 감지된 방에 RoomIdentifier가 없습니다.");
            }
        }
    }

    // 시작 시 이미 방 안에 있어 Enter가 안될 경우 대비
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (string.IsNullOrEmpty(currentRoomID) && collision.CompareTag("Room"))
        {
            RoomIdentifier room = collision.GetComponent<RoomIdentifier>();
            if (room != null)
            {
                currentRoomID = room.roomID;
                Debug.Log($"[PlayerRoom] Stay 중 방 감지됨: {currentRoomID}");
            }
        }
    }
}
