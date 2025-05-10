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
            Debug.LogError("[WiringGameManager] miniGamePanel�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
        Debug.Log($"[WiringGameManager] miniGamePanel �ʱ� ����: {miniGamePanel.activeSelf}");

        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("[WiringGameManager] wires �迭�� ��� �ֽ��ϴ�! �ڽĿ��� �ε� �õ�...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires �迭�� null �׸��� �ֽ��ϴ�!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire �ʱ�ȭ: {wire.name}");
        }
    }

    void Start()
    {
        Debug.Log("[WiringGameManager] Start ȣ���");
        phoneManager = FindFirstObjectByType<PhoneManager>(FindObjectsInactive.Include);
        if (phoneManager == null) Debug.LogWarning("[WiringGameManager] PhoneManager�� ã�� �� �����ϴ�!");

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.transform.parent.gameObject.SetActive(false);
            miniGamePanel.SetActive(false);
            Debug.Log("[WiringGameManager] miniGamePanel �� �θ� ĵ���� ��Ȱ��ȭ �ʱ�ȭ");
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
                Debug.Log($"[WiringGameManager] ���콺 ��Ŭ�� �̿� �Է� ����: anyKeyDown={Input.anyKeyDown}, ��Ŭ��={Input.GetMouseButtonDown(1)}, ��={Input.GetAxisRaw("Mouse ScrollWheel")}");
                CancelGame();
                Debug.Log("[WiringGameManager] �̴ϰ��� ���!");
            }
        }
    }

    public void StartMiniGame(Interactable interactable = null)
    {
        if (isMiniGameActive)
        {
            Debug.Log("[WiringGameManager] �̴ϰ��� �̹� ���� ��: ȣ�� ����");
            return;
        }

        this.interactable = interactable;
        Debug.Log($"[WiringGameManager] StartMiniGame ȣ���, interactable: {(interactable != null ? interactable.gameObject.name : "null")}");

        // �޴��� UI ��Ȱ��ȭ
        if (phoneManager != null && phoneManager.IsPhoneOpen)
        {
            phoneManager.ForceClosePhoneScreen();
            Debug.Log("[WiringGameManager] �̴ϰ��� ����: �޴��� UI ��Ȱ��ȭ");
        }

        if (miniGamePanel == null || miniGamePanel.transform.parent == null)
        {
            Debug.LogError("[WiringGameManager] miniGamePanel �Ǵ� �θ� null�Դϴ�!");
            return;
        }

        Transform parentCanvas = miniGamePanel.transform.parent;
        parentCanvas.gameObject.SetActive(true);
        miniGamePanel.SetActive(true);
        Debug.Log($"[WiringGameManager] �θ� ĵ����: {parentCanvas.name}, miniGamePanel Ȱ��ȭ: {miniGamePanel.activeSelf}");

        Canvas canvas = miniGamePanel.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"[WiringGameManager] ĵ���� ����: Ȱ��ȭ={canvas.enabled}, ������={canvas.scaleFactor}, ����={canvas.sortingOrder}");
            if (!canvas.enabled)
            {
                canvas.enabled = true;
                Debug.LogWarning("[WiringGameManager] ĵ���� ��Ȱ��ȭ ���¿���, ���� Ȱ��ȭ");
            }
        }
        else
        {
            Debug.LogError("[WiringGameManager] ĵ������ ã�� �� �����ϴ�!");
        }

        RectTransform rect = miniGamePanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            Debug.Log($"[WiringGameManager] miniGamePanel ��ġ: {rect.anchoredPosition}, ������: {rect.localScale}, ũ��: {rect.rect}");
        }

        isMiniGameActive = true;
        timeSinceStart = 0f;
        ShufflePositions();

        // wires ���� �ʱ�ȭ
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("[WiringGameManager] wires �迭�� ��� �ֽ��ϴ�! �ٽ� �ε�...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires �迭�� null �׸��� �ֽ��ϴ�!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire {wire.name} �ʱ�ȭ �Ϸ�");
        }
    }

    public void CancelGame()
    {
        if (!isMiniGameActive)
        {
            Debug.Log("[WiringGameManager] �̴ϰ��� �̹� ��Ȱ��ȭ ����!");
            return;
        }

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.SetActive(false);
            miniGamePanel.transform.parent.gameObject.SetActive(false);
            Debug.Log("[WiringGameManager] �̴ϰ��� ���, �θ� ĵ���� ��Ȱ��ȭ");
        }
        else
        {
            Debug.LogError("[WiringGameManager] miniGamePanel �Ǵ� �θ� null�Դϴ�!");
        }

        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("[WiringGameManager] wires �迭�� null �׸��� �ֽ��ϴ�!");
                continue;
            }
            wire.ResetWire();
            Debug.Log($"[WiringGameManager] Wire {wire.name} ��� �Ϸ�");
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
                Debug.LogWarning($"[WiringGameManager] startNodes[{i}]�� null�Դϴ�!");
        }

        Vector2[] shuffledEndPositions = endPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < endNodes.Length; i++)
        {
            if (endNodes[i] != null)
                endNodes[i].localPosition = shuffledEndPositions[i];
            else
                Debug.LogWarning($"[WiringGameManager] endNodes[{i}]�� null�Դϴ�!");
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
            Debug.Log("[WiringGameManager] �̴ϰ��� �Ϸ�!");
            if (miniGamePanel != null && miniGamePanel.transform.parent != null)
            {
                miniGamePanel.SetActive(false);
                miniGamePanel.transform.parent.gameObject.SetActive(false);
                Debug.Log("[WiringGameManager] �θ� ĵ���� ��Ȱ��ȭ �Ϸ�");
            }
            isMiniGameActive = false;
            if (interactable != null)
            {
                interactable.OnMiniGameCompleted();
                Debug.Log("[WiringGameManager] Interactable�� �Ϸ� �˸� ����");
            }
            else
            {
                Debug.LogWarning("[WiringGameManager] Interactable�� null�Դϴ�!");
            }
            // ScheduleManager�� �̼� �Ϸ� �˸�
            ScheduleManager.Instance.CompleteMission("WireBoxSystem");
        }
    }
}