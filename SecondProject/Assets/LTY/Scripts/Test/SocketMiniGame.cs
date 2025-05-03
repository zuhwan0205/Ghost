using UnityEngine;

public class SocketMiniGame : MonoBehaviour, IMiniGame
{
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private LightSocket lightSocket;
    [SerializeField] private PropellerSocket_LJH propellerSocket;
    private bool isGameActive = false;
    private Interactable currentInteractable;
    private string gameId;

    public bool IsActive => isGameActive;

    private void Awake()
    {
        if (miniGamePanel == null || (lightSocket == null && propellerSocket == null))
        {
            Debug.LogError("miniGamePanel 또는 소켓이 설정되지 않았습니다!");
        }
        miniGamePanel.SetActive(false);
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
            return;

        currentInteractable = interactable;
        gameId = interactable.gameObject.name.Contains("Light") ? "Light" : "Vent";
        miniGamePanel.SetActive(true);
        isGameActive = true;
    }

    public void CancelGame()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        miniGamePanel.SetActive(false);
        currentInteractable = null;
    }

    public void CompleteGame()
    {
        isGameActive = false;
        miniGamePanel.SetActive(false);
    }

    public void OnSocketCompleted()
    {
        FindObjectOfType<MiniGameManager>().CompleteMiniGame(gameId, currentInteractable);
    }
}