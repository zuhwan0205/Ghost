using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostTelePort : MonoBehaviour
{
    [Header("텔레포트 설정")]
    public Transform teleportDestination;
    public GameObject destinationRoom;
    [SerializeField] private float teleportDelay = 1f;
    [SerializeField] private float pathHeightOffset = 0.2f;

    [Header("사운드 설정")]
    [SerializeField] private AudioSource[] teleportSources;


    private Renderer rend;
    private Queue<string> routeQueue = new Queue<string>();
    public List<GhostPortal> allPortals;
    private LineRenderer lineRenderer;

    private GhostRoomChase roomChase;
    private void Awake()
    {
        roomChase = GetComponent<GhostRoomChase>(); // 또는 필요한 컴포넌트에서 받아옴
    }

    private void Start()
    {
        rend = GetComponent<Renderer>();
        lineRenderer = GetComponent<LineRenderer>();

        if (rend == null)
            Debug.LogError("[GhostTelePort] Renderer를 찾을 수 없습니다!");
        if (lineRenderer == null)
            Debug.LogWarning("[GhostTelePort] LineRenderer가 없습니다!");
    }

    public void SetRoute(IEnumerable<string> path)
    {
        routeQueue = new Queue<string>(path);
        UpdateLinePath();
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

        string nextRoomID = routeQueue.Peek();
        Debug.Log($"[Teleport] nextRoomID: {nextRoomID}, portal.toRoomID: {portal.toRoomID}");

        if (portal.toRoomID == nextRoomID)
        {
            Debug.Log("[Teleport] 텔레포트 조건 충족, 순간이동 시작!");

            teleportDestination = portal.teleport;
            destinationRoom = portal.destinationRoom;

            StartCoroutine(TeleportWithDelay(teleportDelay));
            routeQueue.Dequeue();
            UpdateLinePath();
        }
    }

    private IEnumerator TeleportWithDelay(float delay)
    {
        if (rend != null) rend.enabled = false;

        yield return new WaitForSeconds(delay);

        Teleport();

        if (rend != null) rend.enabled = true;

        yield return new WaitForSeconds(0.05f);
        transform.position += new Vector3(0.01f, 0f, 0f); // 재충돌 방지용
    }

    private void Teleport()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("[GhostTelePort] teleportDestination이 설정되지 않았습니다!");
            return;
        }

        transform.position = teleportDestination.transform.position;

        PlayAudioGroup(teleportSources);

        Debug.Log("[GhostTelePort] 고스트가 텔레포트되었습니다!");
    }

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


    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && !source.isPlaying)
                source.Play();
        }
    }

    public Vector3 GetCurrentTargetPortalPosition()
    {
        if (routeQueue.Count == 0 || roomChase == null) return transform.position;

        string currentRoomID = roomChase.currentRoomID;
        string nextRoomID = routeQueue.Peek();

        // Unity 6 방식으로 포탈 탐색
        GhostPortal[] portals = Object.FindObjectsByType<GhostPortal>(FindObjectsSortMode.None);

        foreach (var portal in portals)
        {
            if (portal.fromRoomID == currentRoomID && portal.toRoomID == nextRoomID)
            {
                return portal.transform.position;
            }
        }

        return transform.position;
    }



}
