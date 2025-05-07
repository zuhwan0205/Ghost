using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventMonsterChase : MonoBehaviour
{
    [Header("����")]
    public List<GhostPortal> allPortals;
    [SerializeField] private float teleportDelay = 1f;
    [SerializeField] private float waitTimeAfterArrival = 10f;
    [SerializeField] private float returnDelay = 2f;

    private Queue<string> routeQueue = new Queue<string>();
    private string currentRoomID;
    private string originalRoomID;
    private bool isReturning = false;
    private Renderer rend;
    public static List<EventMonsterChase> allMonsters = new List<EventMonsterChase>();


    private void Start()
    {
        allPortals = new List<GhostPortal>(FindObjectsByType<GhostPortal>(FindObjectsSortMode.None));
        rend = GetComponent<Renderer>();
    }

    public void TriggerMoveToRoom(string targetRoomID)
    {
        Debug.Log($"[EventMonsterChase] TriggerMoveToRoom ȣ��� - currentRoomID: {currentRoomID}");

        if (string.IsNullOrEmpty(currentRoomID))
        {
            Debug.LogWarning("[EventMonsterChase] currentRoomID�� �������� �ʾҽ��ϴ�.");
            return;
        }

        if (string.IsNullOrEmpty(originalRoomID))
        {
            originalRoomID = currentRoomID;
            Debug.Log($"[EventMonsterChase] originalRoomID �����: {originalRoomID}");
        }

        Queue<string> path = FindPath(currentRoomID, targetRoomID);

        if (path != null && path.Count > 0)
        {
            routeQueue = path;
            StartCoroutine(FollowRoute(() => StartCoroutine(WaitThenReturn())));
        }
    }

    private IEnumerator FollowRoute(System.Action onArrival)
    {
        while (routeQueue.Count > 0)
        {
            yield return null; // �� ������ routeQueue ���� Ȯ��
        }
        onArrival?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GhostPortal portal = other.GetComponent<GhostPortal>();
        if (portal == null || routeQueue.Count == 0) return;

        string nextRoomID = routeQueue.Peek();
        if (portal.toRoomID == nextRoomID)
        {
            Debug.Log($"[EventMonsterChase] ��Ż �浹 ���� �� �ڷ���Ʈ: {portal.toRoomID}");
            StartCoroutine(TeleportTo(portal));
            routeQueue.Dequeue();
        }
    }

    private IEnumerator TeleportTo(GhostPortal portal)
    {
        if (rend != null) rend.enabled = false;
        yield return new WaitForSeconds(teleportDelay);
        transform.position = portal.teleport.position;
        currentRoomID = portal.toRoomID;
        if (rend != null) rend.enabled = true;
    }

    private IEnumerator WaitThenReturn()
    {
        Debug.Log("[EventMonsterChase] ���� �� ��� ����");
        yield return new WaitForSeconds(waitTimeAfterArrival);

        Debug.Log($"[EventMonsterChase] ���� ��� ��� ����: {currentRoomID} �� {originalRoomID}");
        Queue<string> returnPath = FindPath(currentRoomID, originalRoomID);
        if (returnPath != null && returnPath.Count > 0)
        {
            yield return new WaitForSeconds(returnDelay);
            routeQueue = returnPath;
            isReturning = true;
            StartCoroutine(FollowRoute(() =>
            {
                isReturning = false;
                Debug.Log("[EventMonsterChase] ���� ��ġ ���� �Ϸ�");
            }));
        }
        else
        {
            Debug.LogWarning("[EventMonsterChase] ���� ��� ��� ����: ��� ����");
        }
    }

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

            foreach (var portal in allPortals)
            {
                if (portal.fromRoomID == current && !cameFrom.ContainsKey(portal.toRoomID))
                {
                    frontier.Enqueue(portal.toRoomID);
                    cameFrom[portal.toRoomID] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(toRoomID)) return null;

        Stack<string> reversed = new Stack<string>();
        string node = toRoomID;
        while (node != null)
        {
            reversed.Push(node);
            node = cameFrom[node];
        }

        Queue<string> path = new Queue<string>(reversed);
        path.Dequeue();
        return path;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (string.IsNullOrEmpty(currentRoomID) && other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[EventMonsterChase] Stay �� �� ������: {currentRoomID}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[EventMonsterChase] �� ����: {currentRoomID}");
            }
        }
    }

    private void OnEnable()
    {
        if (!allMonsters.Contains(this))
        {
            allMonsters.Add(this);
            Debug.Log($"[EventMonsterChase] ��ϵ�: {gameObject.name}");
        }
    }

    private void OnDisable()
    {
        if (allMonsters.Contains(this))
        {
            allMonsters.Remove(this);
            Debug.Log($"[EventMonsterChase] ������: {gameObject.name}");
        }
    }

}
