using UnityEngine;
using System.Linq;

public class WiringGameManager : MonoBehaviour
{
    public WireConnection[] wires;
    public GameObject miniGamePanel;
    private Interactable interactable; // 상호작용 오브젝트 참조
    private bool isMiniGameActive = false; // 미니게임 상태 추가
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

    // 미니게임 활성화 상태를 외부에서 확인 가능하도록 getter 추가
    public bool IsMiniGameActive => isMiniGameActive;

    void Awake()
    {
        // 초기 비활성화 설정
        if (miniGamePanel == null)
        {
            Debug.LogError("miniGamePanel이 할당되지 않았습니다!");
            return;
        }
        // MiniGamePanel은 활성화 상태 유지, 부모는 비활성화 상태로 시작
        Debug.Log("MiniGamePanel 초기 상태 확인: 활성화 상태 = " + miniGamePanel.activeSelf);

        // wires 배열 확인 및 초기화
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("wires 배열이 비어 있습니다! Inspector에서 설정하세요.");
            wires = GetComponentsInChildren<WireConnection>(true); // 비활성화된 자식도 포함
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("wires 배열에 null 항목이 있습니다!");
                continue;
            }
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        Debug.Log("WiringGameManager Start 호출됨");
        isMiniGameActive = false;
    }

    public void StartMiniGame(Interactable interactable = null)
    {
        if (isMiniGameActive)
        {
            Debug.Log("미니게임 이미 진행 중: StartMiniGame 호출 무시");
            return;
        }

        this.interactable = interactable; // 상호작용 오브젝트 저장
        Debug.Log("StartMiniGame 호출됨");

        // MiniGameCanvas_wire 활성화
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            Transform parentCanvas = miniGamePanel.transform.parent;
            if (!parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
                Debug.Log($"부모 오브젝트(MiniGameCanvas_wire) 활성화 완료: {parentCanvas.name}");
            }
            else
            {
                Debug.Log($"부모 오브젝트(MiniGameCanvas_wire) 이미 활성화 상태: {parentCanvas.name}");
            }

            miniGamePanel.SetActive(true);
            Debug.Log($"miniGamePanel 활성화 완료, 활성화 상태: {miniGamePanel.activeSelf}");

            // 캔버스 상태 확인
            Canvas canvas = miniGamePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"캔버스 상태: 활성화={canvas.enabled}, 레이어={canvas.gameObject.layer}");
                if (!canvas.enabled)
                {
                    Debug.LogWarning("캔버스가 비활성화 상태입니다! 강제로 활성화합니다.");
                    canvas.enabled = true;
                }
            }
            else
            {
                Debug.LogError("캔버스를 찾을 수 없습니다!");
            }

            // 위치와 스케일 확인
            RectTransform rect = miniGamePanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"MiniGamePanel 위치: {rect.anchoredPosition}, 스케일: {rect.localScale}, 크기: {rect.rect}");
            }
        }
        else
        {
            Debug.LogError("miniGamePanel이 null이거나 부모가 없습니다!");
            return;
        }

        isMiniGameActive = true;
        ShufflePositions();

        // wires 초기화 확인 및 설정
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("wires 배열이 비어 있습니다! 다시 로드 시도...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("wires 배열에 null 항목이 있습니다!");
                continue;
            }
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
            Debug.Log($"Wire {wire.name} 초기화 완료");
        }
    }

    // 취소 메서드
    public void CancelGame()
    {
        if (!isMiniGameActive)
        {
            Debug.Log("미니게임이 이미 비활성화 상태입니다!");
            return;
        }

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.SetActive(false);
            Transform parentCanvas = miniGamePanel.transform.parent;
            parentCanvas.gameObject.SetActive(false);
            Debug.Log($"부모 오브젝트(MiniGameCanvas_wire) 비활성화 완료: {parentCanvas.name}");
            Debug.Log("미니게임 취소됨!");
        }
        else
        {
            Debug.LogError("miniGamePanel이 null이거나 부모가 없습니다!");
        }

        foreach (var wire in wires)
        {
            if (wire != null && wire.wireImage != null)
            {
                wire.isConnected = false;
                wire.wireImage.gameObject.SetActive(false);
            }
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
                Debug.LogWarning($"startNodes[{i}]이 null입니다!");
        }

        Vector2[] shuffledEndPositions = endPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < endNodes.Length; i++)
        {
            if (endNodes[i] != null)
                endNodes[i].localPosition = shuffledEndPositions[i];
            else
                Debug.LogWarning($"endNodes[{i}]이 null입니다!");
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
            Debug.Log("미니게임 완료!");
            if (miniGamePanel != null && miniGamePanel.transform.parent != null)
            {
                miniGamePanel.SetActive(false);
                Transform parentCanvas = miniGamePanel.transform.parent;
                parentCanvas.gameObject.SetActive(false);
                Debug.Log($"부모 오브젝트(MiniGameCanvas_wire) 비활성화 완료: {parentCanvas.name}");
            }
            isMiniGameActive = false;
            if (interactable != null)
            {
                interactable.OnMiniGameCompleted(); // 상호작용 오브젝트에 완료 알림
                Debug.Log("Interactable에 완료 알림 전송 완료!");
            }
            else
            {
                Debug.LogWarning("Interactable이 설정되지 않았습니다!");
            }
        }
    }
}