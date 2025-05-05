using UnityEngine;
using System.Linq;

public class WiringGameManager : MonoBehaviour, IMiniGame
{
    [SerializeField] private WireConnection[] wires;
    [SerializeField] private GameObject miniGamePanel;
    private Interactable interactable;
    private bool isMiniGameActive = false;
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

    public bool IsActive => isMiniGameActive;

    private void Awake()
    {
        if (miniGamePanel == null)
        {
            Debug.LogError("miniGamePanel이 할당되지 않았습니다!");
            return;
        }
        miniGamePanel.SetActive(false);

        if (wires == null || wires.Length == 0)
        {
            wires = GetComponentsInChildren<WireConnection>(true);
        }
        foreach (var wire in wires)
        {
            if (wire == null) continue;
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
        }
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isMiniGameActive)
            return;

        this.interactable = interactable;
        miniGamePanel.SetActive(true);
        isMiniGameActive = true;
        ShufflePositions();

        foreach (var wire in wires)
        {
            if (wire == null) continue;
            wire.isConnected = false;
            if (wire.wireImage != null)
                wire.wireImage.gameObject.SetActive(false);
        }
    }

    public void CancelGame()
    {
        if (!isMiniGameActive)
            return;

        isMiniGameActive = false;
        miniGamePanel.SetActive(false);
        foreach (var wire in wires)
        {
            if (wire != null && wire.wireImage != null)
            {
                wire.isConnected = false;
                wire.wireImage.gameObject.SetActive(false);
            }
        }
    }

    public void CompleteGame()
    {
        isMiniGameActive = false;
        miniGamePanel.SetActive(false);
    }

    private void ShufflePositions()
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
        }

        Vector2[] shuffledEndPositions = endPositions.OrderBy(x => Random.value).ToArray();
        for (int i = 0; i < endNodes.Length; i++)
        {
            if (endNodes[i] != null)
                endNodes[i].localPosition = shuffledEndPositions[i];
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
            FindFirstObjectByType<MiniGameManager>().CompleteMiniGame("Wiring", interactable); // 129줄 수정
        }
    }
}