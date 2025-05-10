using UnityEngine;
using System.Linq;

public class WiringGameManager : MonoBehaviour
{
    public WireConnection[] wires;
    public GameObject miniGamePanel;
    private Interactable interactable;
    private bool isMiniGameActive = false;
    private float inputIgnoreTime = 0.1f;
    private float timeSinceStart = 0f;
    private Vector2[] startPositions = new Vector2[] {
        new Vector2(-300, 150),
        new Vector2(-300, 0),
        new Vector2(-300, -150)
    };
    private Vector2[] endPositions = new Vector2[] {
        new Vector2(300, 150),
        new Vector2(300, 0),
        new Vector2(300, -150)
    };
    private PhoneManager phoneManager;

    public bool IsMiniGameActive => isMiniGameActive;

    void Awake()
    {
        if (miniGamePanel == null)
        {
            Debug.LogError("[WiringGameManager] miniGamePanel이 할당되지 않았습니다!");
            return;
        }
        Debug.Log($"[WiringGameManager] miniGamePanel 초기 상태: {miniGamePanel.activeSelf}");

        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("[WiringGameManager] wires 배열이 비어 있습니다! 자식에서 로드 시도...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires 배열에 null 항목이 있습니다!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire 초기화: {wire.name}");
        }
    }

    void Start()
    {
        Debug.Log("[WiringGameManager] Start 호출됨");
        phoneManager = FindFirstObjectByType<PhoneManager>(FindObjectsInactive.Include);
        if (phoneManager == null) Debug.LogWarning("[WiringGameManager] PhoneManager를 찾을 수 없습니다!");

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.transform.parent.gameObject.SetActive(false);
            miniGamePanel.SetActive(false);
            Debug.Log("[WiringGameManager] miniGamePanel 및 부모 캔버스 비활성화 초기화");
        }
    }

    void Update()
    {
        if (!isMiniGameActive) return;

        timeSinceStart += Time.deltaTime;
        if (timeSinceStart < inputIgnoreTime) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(1) || Input.GetAxisRaw("Mouse ScrollWheel") != 0f)
        {
            if (!Input.GetMouseButtonDown(0))
            {
                Debug.Log($"[WiringGameManager] 마우스 좌클릭 이외 입력 감지: anyKeyDown={Input.anyKeyDown}, 우클릭={Input.GetMouseButtonDown(1)}, 휠={Input.GetAxisRaw("Mouse ScrollWheel")}");
                CancelGame();
                Debug.Log("[WiringGameManager] 미니게임 취소!");
            }
        }
    }

    public void StartMiniGame(Interactable interactable = null)
    {
        if (isMiniGameActive)
        {
            Debug.Log("[WiringGameManager] 미니게임 이미 진행 중: 호출 무시");
            return;
        }

        this.interactable = interactable;
        Debug.Log($"[WiringGameManager] StartMiniGame 호출됨, interactable: {(interactable != null ? interactable.gameObject.name : "null")}");

        // 휴대폰 UI 비활성화
        if (phoneManager != null && phoneManager.IsPhoneOpen)
        {
            phoneManager.ForceClosePhoneScreen();
            Debug.Log("[WiringGameManager] 미니게임 시작: 휴대폰 UI 비활성화");
        }

        if (miniGamePanel == null || miniGamePanel.transform.parent == null)
        {
            Debug.LogError("[WiringGameManager] miniGamePanel 또는 부모가 null입니다!");
            return;
        }

        Transform parentCanvas = miniGamePanel.transform.parent;
        parentCanvas.gameObject.SetActive(true);
        miniGamePanel.SetActive(true);
        Debug.Log($"[WiringGameManager] 부모 캔버스: {parentCanvas.name}, miniGamePanel 활성화: {miniGamePanel.activeSelf}");

        Canvas canvas = miniGamePanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[WiringGameManager] 캔버스 상태: 활성화={canvas.enabled}, 스케일={canvas.scaleFactor}, 정렬={canvas.sortingOrder}");
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                Debug.LogWarning("[WiringGameManager] 캔버스 비활성화 상태였음, 강제 활성화");
            }
        }
        else
        {
            Debug.LogError("[WiringGameManager] 캔버스를 찾을 수 없습니다!");
        }

        RectTransform rect = miniGamePanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            Debug.Log($"[WiringGameManager] miniGamePanel 위치: {rect.anchoredPosition}, 스케일: {rect.localScale}, 크기: {rect.rect}");
        }

        isMiniGameActive = true;
        timeSinceStart = 0f;
        ShufflePositions();

        // wires 완전 초기화
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("[WiringGameManager] wires 배열이 비어 있습니다! 다시 로드...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires 배열에 null 항목이 있습니다!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire {wire.name} 초기화 완료");
        }
    }

    public void CancelGame()
    {
        if (!isMiniGameActive)
        {
            Debug.Log("[WiringGameManager] 미니게임 이미 비활성화 상태!");
            return;
        }

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.SetActive(false);
            miniGamePanel.transform.parent.gameObject.SetActive(false);
            Debug.Log("[WiringGameManager] 미니게임 취소, 부모 캔버스 비활성화");
        }
        else
        {
            Debug.LogError("[WiringGameManager] miniGamePanel 또는 부모가 null입니다!");
        }

        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires 배열에 null 항목이 있습니다!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire {wire.name} 취소 완료");
        }
    }

    void ShufflePositions()
    {
        Transform[] startNodes = new Transform[] {
            wires[0].startNode,
            wires[1].startNode,
            wires[2].startNode
        };
        Transform[] endNodes = new Transform[] {
            wires[0].endNode,
            wires[1].endNode,
            wires[2].endNode
        };

        Vector2[] shuffledStartPositions = startPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < startNodes.Length; i++)
        {
            if (startNodes[i] != null)
                startNodes[i].localPosition = shuffledStartPositions[i];
            else
                Debug.LogWarning($"[WiringGameManager] startNodes[{i}]이 null입니다!");
        }

        Vector2[] shuffledEndPositions = endPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < endNodes.Length; i++)
        {
            if (endNodes[i] != null)
                endNodes[i].localPosition = shuffledEndPositions[i];
            else
                Debug.LogWarning($"[WiringGameManager] endNodes[{i}]이 null입니다!");
        }
    }

    public void CheckAllWires()
    {
        bool allConnected = true;
        foreach (var wire in wires)
        {
            if (wire == null || !wire.isConnected)
            {
                allConnected = false;
                break;
            }
        }

        if (allConnected)
        {
            Debug.Log("[WiringGameManager] 미니게임 완료!");
            if (miniGamePanel != null && miniGamePanel.transform.parent != null)
            {
                miniGamePanel.SetActive(false);
                miniGamePanel.transform.parent.gameObject.SetActive(false);
                Debug.Log("[WiringGameManager] 부모 캔버스 비활성화 완료");
            }
            isMiniGameActive = false;
            if (interactable != null)
            {
                interactable.OnMiniGameCompleted();
                Debug.Log("[WiringGameManager] Interactable에 완료 알림 전송");
            }
            else
            {
                Debug.LogWarning("[WiringGameManager] Interactable이 null입니다!");
            }
            // ScheduleManager에 미션 완료 알림
            ScheduleManager.Instance.CompleteMission("WireBoxSystem");
        }
    }
}