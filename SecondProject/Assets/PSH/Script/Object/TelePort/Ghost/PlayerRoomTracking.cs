using UnityEngine;

public class PlayerRoomTracking : MonoBehaviour
{
    [Header("�÷��̾� �� �ν� ����")]
    public string currentRoomID; // ���� �÷��̾ �ִ� �� ID

    // �濡 �������� �� ȣ�� (�⺻���� �浹 ����)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
        {
            RoomIdentifier room = collision.GetComponent<RoomIdentifier>();
            if (room != null)
            {
                currentRoomID = room.roomID;
                Debug.Log($"[PlayerRoom] �÷��̾ �濡 ����: {currentRoomID}");
            }
            else
            {
                Debug.LogWarning("[PlayerRoom] ������ �濡 RoomIdentifier�� �����ϴ�.");
            }
        }
    }

    // ���� �� �̹� �� �ȿ� �־� Enter�� �ȵ� ��� ���
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (string.IsNullOrEmpty(currentRoomID) && collision.CompareTag("Room"))
        {
            RoomIdentifier room = collision.GetComponent<RoomIdentifier>();
            if (room != null)
            {
                currentRoomID = room.roomID;
                Debug.Log($"[PlayerRoom] Stay �� �� ������: {currentRoomID}");
            }
        }
    }
}
