
using UnityEngine;
using System.Collections.Generic;

public class GhostRoomChase : MonoBehaviour
{
    [Header("�÷��̾� �� ����")]
    public PlayerRoomTracking playerRoom;             // �÷��̾��� ���� �� ������ �����ϴ� ������Ʈ
    public GhostTelePort ghostTelePort;               // �ڷ���Ʈ�� �����ϴ� ��ũ��Ʈ

    private string playerCurrentRoomID;               // �÷��̾ �ִ� �� ID
    public string currentRoomID;                      // ��Ʈ�� �ִ� �� ID

    [Header("��Ż ����")]
    public List<GhostPortal> portals;                 // ��� ��Ż ���

    private bool warnedMissingPlayerRoom = false;     // ��� �޽����� �� ���� ����ϱ� ���� �÷���

    private void Start()
    {
        // �÷��̾� �� ���� ������Ʈ �ڵ� �Ҵ�
        if (playerRoom == null)
        {
            playerRoom = FindAnyObjectByType<PlayerRoomTracking>();
            if (playerRoom == null)
                Debug.LogWarning("[GhostRoomChase] PlayerRoomTracking �ڵ� ���� ����: ���� �������� ����");
        }

        // �� �� ��Ż���� �ڵ� ����
        portals = new List<GhostPortal>(FindObjectsByType<GhostPortal>(FindObjectsSortMode.None));
        Debug.Log($"[GhostRoomChase] �ڵ� ������ ��Ż ����: {portals.Count}");

        // GhostTelePort ��ũ��Ʈ �ڵ� ����
        ghostTelePort = GetComponent<GhostTelePort>();
        if (ghostTelePort == null)
        {
            Debug.LogError("[GhostRoomChase] GhostTelePort ��ũ��Ʈ�� ã�� �� �����ϴ�!");
        }
        else
        {
            ghostTelePort.allPortals = portals;
        }
    }

    private void Update()
    {
        // �÷��̾��� �� ID ������Ʈ
        UpdatePlayerRoomID();

        // ���� �� ID�� �������� ���� ��� �ƹ��͵� ���� ����
        if (string.IsNullOrEmpty(currentRoomID) || string.IsNullOrEmpty(playerCurrentRoomID))
            return;

        // ���� �ٸ� ��� �ڷ���Ʈ ��� ã��
        if (currentRoomID != playerCurrentRoomID)
        {
            Debug.Log($"[GhostRoomChase] �÷��̾�� ��Ʈ�� �ٸ� �濡 ����: {currentRoomID} -> {playerCurrentRoomID}");
            FindAndSetPath(currentRoomID, playerCurrentRoomID);
        }
    }

    private void UpdatePlayerRoomID()
    {
        if (playerRoom != null)
        {
            playerCurrentRoomID = playerRoom.currentRoomID;
        }
        else if (!warnedMissingPlayerRoom)
        {
            Debug.LogWarning("[GhostRoomChase] PlayerRoomTracking ���� �� ��!");
            warnedMissingPlayerRoom = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // �浹�� ������Ʈ�� Room�̶�� ���� �� ID ����
        if (other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[GhostRoomChase] ���� �� ID ���ŵ�: {currentRoomID}");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // ���� �� Room�� �̹� ���� �ִ� ��� ���
        if (string.IsNullOrEmpty(currentRoomID) && other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[GhostRoomChase] Stay �� �� ������: {currentRoomID}");
            }
        }
    }

    private void FindAndSetPath(string fromRoomID, string toRoomID)
    {
        Queue<string> path = FindPath(fromRoomID, toRoomID);
        if (path != null && path.Count > 0)
        {
            ghostTelePort.SetRoute(path);  // ��� ����
        }
    }

    // BFS �˰������� ��Ż ��� ã��
    private Queue<string> FindPath(string fromRoomID, string toRoomID)
    {
        Dictionary<string, string> cameFrom = new Dictionary<string, string>();
        Queue<string> frontier = new Queue<string>();
        frontier.Enqueue(fromRoomID);
        cameFrom[fromRoomID] = null;

        while (frontier.Count > 0)
        {
            string current = frontier.Dequeue();
            if (current == toRoomID) break;

            foreach (var portal in portals)
            {
                if (portal.fromRoomID == current && !cameFrom.ContainsKey(portal.toRoomID))
                {
                    frontier.Enqueue(portal.toRoomID);
                    cameFrom[portal.toRoomID] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(toRoomID)) return null;

        Stack<string> reversedPath = new Stack<string>();
        string node = toRoomID;
        while (node != null)
        {
            reversedPath.Push(node);
            node = cameFrom[node];
        }

        Queue<string> path = new Queue<string>(reversedPath);
        path.Dequeue(); // ���� �� �����ϰ� ��� ����
        return path;
    }

    // ���� ������ �� �ܺο��� ȣ���ؼ� ������ ���� ����
    public void TryChaseOnBlocked()
    {
        UpdatePlayerRoomID();

        if (string.IsNullOrEmpty(currentRoomID) || string.IsNullOrEmpty(playerCurrentRoomID))
        {
            Debug.LogWarning("[GhostRoomChase] TryChaseOnBlocked ȣ�� �� �� ID�� �������");
            return;
        }

        if (currentRoomID != playerCurrentRoomID)
        {
            Debug.Log($"[GhostRoomChase] TryChaseOnBlocked: {currentRoomID} �� {playerCurrentRoomID} ���� ����");
            FindAndSetPath(currentRoomID, playerCurrentRoomID);
        }
    }
}
