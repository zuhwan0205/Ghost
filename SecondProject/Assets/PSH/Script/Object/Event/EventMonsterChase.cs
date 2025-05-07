using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EventMonsterChase : MonoBehaviour
{
    [Header("설정")]
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
        Debug.Log($"[EventMonsterChase] TriggerMoveToRoom 호출됨 - currentRoomID: {currentRoomID}");

        if (string.IsNullOrEmpty(currentRoomID))
        {
            Debug.LogWarning("[EventMonsterChase] currentRoomID가 설정되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(originalRoomID))
        {
            originalRoomID = currentRoomID;
            Debug.Log($"[EventMonsterChase] originalRoomID 저장됨: {originalRoomID}");
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
            yield return null; // 매 프레임 routeQueue 상태 확인
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
            Debug.Log($"[EventMonsterChase] 포탈 충돌 감지 → 텔레포트: {portal.toRoomID}");
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
        Debug.Log("[EventMonsterChase] 도착 후 대기 시작");
        yield return new WaitForSeconds(waitTimeAfterArrival);

        Debug.Log($"[EventMonsterChase] 복귀 경로 계산 시작: {currentRoomID} → {originalRoomID}");
        Queue<string> returnPath = FindPath(currentRoomID, originalRoomID);
        if (returnPath != null && returnPath.Count > 0)
        {
            yield return new WaitForSeconds(returnDelay);
            routeQueue = returnPath;
            isReturning = true;
            StartCoroutine(FollowRoute(() =>
            {
                isReturning = false;
                Debug.Log("[EventMonsterChase] 원래 위치 복귀 완료");
            }));
        }
        else
        {
            Debug.LogWarning("[EventMonsterChase] 복귀 경로 계산 실패: 경로 없음");
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
                Debug.Log($"[EventMonsterChase] Stay 중 방 감지됨: {currentRoomID}");
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
                Debug.Log($"[EventMonsterChase] 방 진입: {currentRoomID}");
            }
        }
    }

    private void OnEnable()
    {
        if (!allMonsters.Contains(this))
        {
            allMonsters.Add(this);
            Debug.Log($"[EventMonsterChase] 등록됨: {gameObject.name}");
        }
    }

    private void OnDisable()
    {
        if (allMonsters.Contains(this))
        {
            allMonsters.Remove(this);
            Debug.Log($"[EventMonsterChase] 해제됨: {gameObject.name}");
        }
    }

}
