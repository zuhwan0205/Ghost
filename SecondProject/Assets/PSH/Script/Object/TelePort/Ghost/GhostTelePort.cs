using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostTelePort : MonoBehaviour
{
    [Header("텔레포트 설정")]
    public Transform teleportDestination;       // 목적지 위치
    public GameObject destinationRoom;          // 목적지 방 오브젝트

    [SerializeField] private float teleportDelay = 1f;          // 순간이동 지연 시간
    [SerializeField] private float pathHeightOffset = 0.2f;     // 경로 라인 높이 보정값

    private Renderer rend;                      // 고스트의 렌더러 (숨기기용)
    private Queue<string> routeQueue = new Queue<string>();    // 이동할 방 ID 목록
    public List<GhostPortal> allPortals;        // 모든 포탈 목록

    private LineRenderer lineRenderer;          // 경로 시각화 라인

    private void Start()
    {
        rend = GetComponent<Renderer>();
        lineRenderer = GetComponent<LineRenderer>();

        if (rend == null)
            Debug.LogError("[GhostTelePort] Renderer를 찾을 수 없습니다!");
        if (lineRenderer == null)
            Debug.LogWarning("[GhostTelePort] LineRenderer가 없습니다!");
    }

    // 외부에서 경로를 설정함
    public void SetRoute(IEnumerable<string> path)
    {
        routeQueue = new Queue<string>(path);
        UpdateLinePath();  // 시각적 경로도 갱신
    }

    public bool HasRoute() => routeQueue != null && routeQueue.Count > 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (routeQueue.Count == 0)
        {
            Debug.LogWarning("[Teleport] routeQueue가 비어있습니다!");
            return;
        }

        GhostPortal portal = collision.GetComponent<GhostPortal>();
        if (portal == null)
        {
            Debug.LogWarning("[Teleport] GhostPortal 컴포넌트가 없음!");
            return;
        }

        string nextRoomID = routeQueue.Peek(); // 다음 목적지 ID
        Debug.Log($"[Teleport] nextRoomID: {nextRoomID}, portal.toRoomID: {portal.toRoomID}");

        // 올바른 목적지일 경우 순간이동 시작
        if (portal.toRoomID == nextRoomID)
        {
            Debug.Log("[Teleport] 텔레포트 조건 충족, 순간이동 시작!");

            teleportDestination = portal.teleport;
            destinationRoom = portal.destinationRoom;

            StartCoroutine(TeleportWithDelay(teleportDelay)); // 순간이동 지연 처리

            routeQueue.Dequeue(); // 다음 목적지 제거
            UpdateLinePath();     // 라인 다시 갱신
        }
    }

    // 순간이동 실행 (지연 포함)
    private IEnumerator TeleportWithDelay(float delay)
    {
        if (rend != null) rend.enabled = false;

        yield return new WaitForSeconds(delay);

        Teleport();

        if (rend != null) rend.enabled = true;

        yield return new WaitForSeconds(0.05f);
        transform.position += new Vector3(0.01f, 0f, 0f); // 재충돌 방지용 위치 미세 이동
    }

    // 실제 위치 이동 처리
    private void Teleport()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("[GhostTelePort] teleportDestination이 설정되지 않았습니다!");
            return;
        }

        transform.position = teleportDestination.transform.position;
        Debug.Log("[GhostTelePort] 고스트가 텔레포트되었습니다!");
    }

    // 경로 시각화 갱신
    private void UpdateLinePath()
    {
        if (lineRenderer == null || routeQueue.Count == 0 || allPortals == null) return;

        List<Vector3> points = new List<Vector3>();
        points.Add(transform.position + Vector3.up * pathHeightOffset);

        string current = GetComponent<GhostRoomChase>()?.currentRoomID;
        Queue<string> temp = new Queue<string>(routeQueue);

        while (temp.Count > 0)
        {
            string next = temp.Dequeue();
            foreach (var portal in allPortals)
            {
                if (portal.fromRoomID == current && portal.toRoomID == next)
                {
                    points.Add(portal.teleport.transform.position + Vector3.up * pathHeightOffset);
                    current = next;
                    break;
                }
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }
}
