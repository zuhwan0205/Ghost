
using UnityEngine;
using System.Collections.Generic;

public class GhostRoomChase : MonoBehaviour
{
    [Header("플레이어 방 추적")]
    public PlayerRoomTracking playerRoom;             // 플레이어의 현재 방 정보를 추적하는 컴포넌트
    public GhostTelePort ghostTelePort;               // 텔레포트를 제어하는 스크립트

    private string playerCurrentRoomID;               // 플레이어가 있는 방 ID
    public string currentRoomID;                      // 고스트가 있는 방 ID

    [Header("포탈 설정")]
    public List<GhostPortal> portals;                 // 모든 포탈 목록

    private bool warnedMissingPlayerRoom = false;     // 경고 메시지를 한 번만 출력하기 위한 플래그

    private void Start()
    {
        // 플레이어 방 추적 컴포넌트 자동 할당
        if (playerRoom == null)
        {
            playerRoom = FindAnyObjectByType<PlayerRoomTracking>();
            if (playerRoom == null)
                Debug.LogWarning("[GhostRoomChase] PlayerRoomTracking 자동 연결 실패: 씬에 존재하지 않음");
        }

        // 씬 내 포탈들을 자동 수집
        portals = new List<GhostPortal>(FindObjectsByType<GhostPortal>(FindObjectsSortMode.None));
        Debug.Log($"[GhostRoomChase] 자동 수집된 포탈 개수: {portals.Count}");

        // GhostTelePort 스크립트 자동 연결
        ghostTelePort = GetComponent<GhostTelePort>();
        if (ghostTelePort == null)
        {
            Debug.LogError("[GhostRoomChase] GhostTelePort 스크립트를 찾을 수 없습니다!");
        }
        else
        {
            ghostTelePort.allPortals = portals;
        }
    }

    private void Update()
    {
        // 플레이어의 방 ID 업데이트
        UpdatePlayerRoomID();

        // 양쪽 방 ID가 설정되지 않은 경우 아무것도 하지 않음
        if (string.IsNullOrEmpty(currentRoomID) || string.IsNullOrEmpty(playerCurrentRoomID))
            return;

        // 방이 다를 경우 텔레포트 경로 찾기
        if (currentRoomID != playerCurrentRoomID)
        {
            Debug.Log($"[GhostRoomChase] 플레이어와 고스트가 다른 방에 있음: {currentRoomID} -> {playerCurrentRoomID}");
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
            Debug.LogWarning("[GhostRoomChase] PlayerRoomTracking 연결 안 됨!");
            warnedMissingPlayerRoom = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌한 오브젝트가 Room이라면 현재 방 ID 갱신
        if (other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[GhostRoomChase] 현재 방 ID 갱신됨: {currentRoomID}");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 시작 시 Room과 이미 겹쳐 있는 경우 대비
        if (string.IsNullOrEmpty(currentRoomID) && other.CompareTag("Room"))
        {
            RoomIdentifier id = other.GetComponent<RoomIdentifier>();
            if (id != null)
            {
                currentRoomID = id.roomID;
                Debug.Log($"[GhostRoomChase] Stay 중 방 감지됨: {currentRoomID}");
            }
        }
    }

    private void FindAndSetPath(string fromRoomID, string toRoomID)
    {
        Queue<string> path = FindPath(fromRoomID, toRoomID);
        if (path != null && path.Count > 0)
        {
            ghostTelePort.SetRoute(path);  // 경로 설정
        }
    }

    // BFS 알고리즘으로 포탈 경로 찾기
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
        path.Dequeue(); // 현재 방 제외하고 경로 설정
        return path;
    }

    // 길이 막혔을 때 외부에서 호출해서 강제로 추적 유도
    public void TryChaseOnBlocked()
    {
        UpdatePlayerRoomID();

        if (string.IsNullOrEmpty(currentRoomID) || string.IsNullOrEmpty(playerCurrentRoomID))
        {
            Debug.LogWarning("[GhostRoomChase] TryChaseOnBlocked 호출 중 방 ID가 비어있음");
            return;
        }

        if (currentRoomID != playerCurrentRoomID)
        {
            Debug.Log($"[GhostRoomChase] TryChaseOnBlocked: {currentRoomID} → {playerCurrentRoomID} 추적 유도");
            FindAndSetPath(currentRoomID, playerCurrentRoomID);
        }
    }
}
