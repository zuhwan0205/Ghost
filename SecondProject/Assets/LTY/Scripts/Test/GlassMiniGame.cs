using UnityEngine;

public class GlassMiniGame : MonoBehaviour, IMiniGame
{
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private TrashCan trashCan;
    private bool isGameActive = false;
    private Interactable currentInteractable;

    public bool IsActive => isGameActive;

    private void Awake()
    {
        if (miniGamePanel == null || trashCan == null)
        {
            Debug.LogError("miniGamePanel 또는 TrashCan이 설정되지 않았습니다!");
        }
        miniGamePanel.SetActive(false);
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
            return;

        currentInteractable = interactable;
        miniGamePanel.SetActive(true);
        isGameActive = true;
        trashCan.ResetCount();
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

    public void OnGlassMissionCompleted()
    {
        FindFirstObjectByType<MiniGameManager>().CompleteMiniGame("Glass", currentInteractable); // 50줄 수정
    }
}