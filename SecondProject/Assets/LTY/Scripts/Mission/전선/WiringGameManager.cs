using UnityEngine;
using System.Linq;

public class WiringGameManager : MonoBehaviour
{
    public WireConnection[] wires;
    public GameObject miniGamePanel;
    private Interactable interactable; // ��ȣ�ۿ� ������Ʈ ����
    private bool isMiniGameActive = false; // �̴ϰ��� ���� �߰�
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

    // �̴ϰ��� Ȱ��ȭ ���¸� �ܺο��� Ȯ�� �����ϵ��� getter �߰�
    public bool IsMiniGameActive => isMiniGameActive;

    void Awake()
    {
        // �ʱ� ��Ȱ��ȭ ����
        if (miniGamePanel == null)
        {
            Debug.LogError("miniGamePanel�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }
        // MiniGamePanel�� Ȱ��ȭ ���� ����, �θ�� ��Ȱ��ȭ ���·� ����
        Debug.Log("MiniGamePanel �ʱ� ���� Ȯ��: Ȱ��ȭ ���� = " + miniGamePanel.activeSelf);

        // wires �迭 Ȯ�� �� �ʱ�ȭ
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("wires �迭�� ��� �ֽ��ϴ�! Inspector���� �����ϼ���.");
            wires = GetComponentsInChildren<WireConnection>(true); // ��Ȱ��ȭ�� �ڽĵ� ����
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("wires �迭�� null �׸��� �ֽ��ϴ�!");
                continue;
            }
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        Debug.Log("WiringGameManager Start ȣ���");
        isMiniGameActive = false;
    }

    public void StartMiniGame(Interactable interactable = null)
    {
        if (isMiniGameActive)
        {
            Debug.Log("�̴ϰ��� �̹� ���� ��: StartMiniGame ȣ�� ����");
            return;
        }

        this.interactable = interactable; // ��ȣ�ۿ� ������Ʈ ����
        Debug.Log("StartMiniGame ȣ���");

        // MiniGameCanvas_wire Ȱ��ȭ
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            Transform parentCanvas = miniGamePanel.transform.parent;
            if (!parentCanvas.gameObject.activeSelf)
            {
                parentCanvas.gameObject.SetActive(true);
                Debug.Log($"�θ� ������Ʈ(MiniGameCanvas_wire) Ȱ��ȭ �Ϸ�: {parentCanvas.name}");
            }
            else
            {
                Debug.Log($"�θ� ������Ʈ(MiniGameCanvas_wire) �̹� Ȱ��ȭ ����: {parentCanvas.name}");
            }

            miniGamePanel.SetActive(true);
            Debug.Log($"miniGamePanel Ȱ��ȭ �Ϸ�, Ȱ��ȭ ����: {miniGamePanel.activeSelf}");

            // ĵ���� ���� Ȯ��
            Canvas canvas = miniGamePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"ĵ���� ����: Ȱ��ȭ={canvas.enabled}, ���̾�={canvas.gameObject.layer}");
                if (!canvas.enabled)
                {
                    Debug.LogWarning("ĵ������ ��Ȱ��ȭ �����Դϴ�! ������ Ȱ��ȭ�մϴ�.");
                    canvas.enabled = true;
                }
            }
            else
            {
                Debug.LogError("ĵ������ ã�� �� �����ϴ�!");
            }

            // ��ġ�� ������ Ȯ��
            RectTransform rect = miniGamePanel.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"MiniGamePanel ��ġ: {rect.anchoredPosition}, ������: {rect.localScale}, ũ��: {rect.rect}");
            }
        }
        else
        {
            Debug.LogError("miniGamePanel�� null�̰ų� �θ� �����ϴ�!");
            return;
        }

        isMiniGameActive = true;
        ShufflePositions();

        // wires �ʱ�ȭ Ȯ�� �� ����
        if (wires == null || wires.Length == 0)
        {
            Debug.LogWarning("wires �迭�� ��� �ֽ��ϴ�! �ٽ� �ε� �õ�...");
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null)
            {
                Debug.LogError("wires �迭�� null �׸��� �ֽ��ϴ�!");
                continue;
            }
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
            Debug.Log($"Wire {wire.name} �ʱ�ȭ �Ϸ�");
        }
    }

    // ��� �޼���
    public void CancelGame()
    {
        if (!isMiniGameActive)
        {
            Debug.Log("�̴ϰ����� �̹� ��Ȱ��ȭ �����Դϴ�!");
            return;
        }

        isMiniGameActive = false;
        if (miniGamePanel != null && miniGamePanel.transform.parent != null)
        {
            miniGamePanel.SetActive(false);
            Transform parentCanvas = miniGamePanel.transform.parent;
            parentCanvas.gameObject.SetActive(false);
            Debug.Log($"�θ� ������Ʈ(MiniGameCanvas_wire) ��Ȱ��ȭ �Ϸ�: {parentCanvas.name}");
            Debug.Log("�̴ϰ��� ��ҵ�!");
        }
        else
        {
            Debug.LogError("miniGamePanel�� null�̰ų� �θ� �����ϴ�!");
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
                Debug.LogWarning($"startNodes[{i}]�� null�Դϴ�!");
        }

        Vector2[] shuffledEndPositions = endPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < endNodes.Length; i++)
        {
            if (endNodes[i] != null)
                endNodes[i].localPosition = shuffledEndPositions[i];
            else
                Debug.LogWarning($"endNodes[{i}]�� null�Դϴ�!");
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
            Debug.Log("�̴ϰ��� �Ϸ�!");
            if (miniGamePanel != null && miniGamePanel.transform.parent != null)
            {
                miniGamePanel.SetActive(false);
                Transform parentCanvas = miniGamePanel.transform.parent;
                parentCanvas.gameObject.SetActive(false);
                Debug.Log($"�θ� ������Ʈ(MiniGameCanvas_wire) ��Ȱ��ȭ �Ϸ�: {parentCanvas.name}");
            }
            isMiniGameActive = false;
            if (interactable != null)
            {
                interactable.OnMiniGameCompleted(); // ��ȣ�ۿ� ������Ʈ�� �Ϸ� �˸�
                Debug.Log("Interactable�� �Ϸ� �˸� ���� �Ϸ�!");
            }
            else
            {
                Debug.LogWarning("Interactable�� �������� �ʾҽ��ϴ�!");
            }
        }
    }
}