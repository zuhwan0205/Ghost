using UnityEngine;

public interface IMiniGame
{
    void StartMiniGame(Interactable interactable);
    void CancelGame();
    void CompleteGame();
    bool IsActive { get; }
}
