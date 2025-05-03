using UnityEngine;

public class StainWiper : MonoBehaviour, IMiniGame
{
    private Camera cam;
    private Vector2 lastSwipePos;
    private float swipeThreshold = 0.1f;
    private bool hasSwipedOnce = false;
    private bool isGameActive = false;
    private Interactable currentInteractable;
    private int cleanCount = 0;
    private string gameId;

    private void Awake()
    {
        cam = Camera.main;
        gameObject.SetActive(false);
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
            return;

        currentInteractable = interactable;
        gameId = interactable.miniGameId; // miniGameId로 미니게임 구분
        gameObject.SetActive(true);
        isGameActive = true;
        cleanCount = 0;
        hasSwipedOnce = false;
    }

    public void CancelGame()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        gameObject.SetActive(false);
        cleanCount = 0;
        currentInteractable = null;
    }

    public void CompleteGame()
    {
        isGameActive = false;
        gameObject.SetActive(false);
        cleanCount = 0;
    }

    public bool IsActive => isGameActive;

    private void Update()
    {
        if (!isGameActive)
            return;

        Vector2 inputPos;
        if (!InputHandler.GetInputPosition(out inputPos, cam))
        {
            hasSwipedOnce = false;
            return;
        }

        if (!hasSwipedOnce)
        {
            lastSwipePos = inputPos;
            hasSwipedOnce = true;
        }

        if (Vector2.Distance(inputPos, lastSwipePos) > swipeThreshold)
        {
            TryClean(inputPos);
            lastSwipePos = inputPos;
        }
    }

    private void TryClean(Vector2 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out Stain stain))
        {
            stain.Clean(position);
        }
        else if (hit != null && hit.TryGetComponent(out Dust_LJH dust))
        {
            dust.Clean(position);
        }
    }

    public void OnCleanCompleted()
    {
        cleanCount++;
        int requiredCount = gameId == "Mannequin" ? 3 : 4; // 미니게임별 완료 조건
        if (cleanCount >= requiredCount)
        {
            FindObjectOfType<MiniGameManager>().CompleteMiniGame(gameId, currentInteractable);
        }
    }
}