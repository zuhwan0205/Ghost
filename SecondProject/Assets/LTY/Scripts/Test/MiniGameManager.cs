using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    [System.Serializable]
    private struct MiniGameEntry
    {
        public string gameId;
        public GameObject panel;
        public MonoBehaviour miniGameScript;
    }

    [SerializeField] private List<MiniGameEntry> miniGameEntries = new List<MiniGameEntry>();
    private Dictionary<string, IMiniGame> miniGames = new Dictionary<string, IMiniGame>();
    private Dictionary<string, GameObject> miniGamePanels = new Dictionary<string, GameObject>();
    private string activeGameId = null;
    private Interactable currentInteractable;
    private int ventTrashCount = 0;

    private void Awake()
    {
        foreach (var entry in miniGameEntries)
        {
            if (entry.panel == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}의 패널이 설정되지 않았습니다!");
                continue;
            }

            miniGamePanels.Add(entry.gameId, entry.panel);
            entry.panel.SetActive(false);

            if (entry.miniGameScript == null)
            {
                continue;
            }

            var miniGame = entry.miniGameScript as IMiniGame;
            if (miniGame == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}는 IMiniGame 인터페이스를 구현하지 않았습니다!");
                continue;
            }

            miniGames.Add(entry.gameId, miniGame);
        }
    }

    public void StartMiniGame(string gameId, Interactable interactable)
    {
        Debug.Log($"StartMiniGame called with gameId: {gameId}, interactable: {interactable != null}");
        if (!miniGamePanels.ContainsKey(gameId))
        {
            Debug.LogError($"MiniGame ID {gameId}가 등록되지 않았습니다!");
            return;
        }

        if (activeGameId != null)
        {
            Debug.LogWarning("다른 미니게임이 이미 진행 중입니다!");
            return;
        }

        activeGameId = gameId;
        currentInteractable = interactable;
        if (miniGamePanels[gameId] == null)
        {
            Debug.LogError($"Panel for gameId {gameId} is null!");
            return;
        }

        // 패널 명시적 활성화
        miniGamePanels[gameId].SetActive(true);
        Debug.Log($"Panel {gameId} 활성화 - {miniGamePanels[gameId].activeSelf}");

        if (miniGames.ContainsKey(gameId))
        {
            if (miniGames[gameId] != null)
            {
                miniGames[gameId].StartMiniGame(interactable);
            }
            else
            {
                Debug.LogError($"MiniGame script for {gameId} is null!");
            }
        }
        else if (gameId == "Vent")
        {
            ventTrashCount = 0;
        }

        Debug.Log($"MiniGame {gameId} 시작!");
    }

    public void CancelMiniGame(string gameId)
    {
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        if (miniGames.ContainsKey(gameId))
        {
            if (miniGames[gameId] != null)
            {
                miniGames[gameId].CancelGame();
            }
        }

        DeactivatePanel(gameId);
        activeGameId = null;
        currentInteractable = null;
        Debug.Log($"MiniGame {gameId} 취소!");
    }

    public void CancelAllGames()
    {
        if (activeGameId != null)
        {
            CancelMiniGame(activeGameId);
        }
    }

    public void CompleteMiniGame(string gameId, Interactable interactable)
    {
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        if (miniGames.ContainsKey(gameId))
        {
            if (miniGames[gameId] != null)
            {
                miniGames[gameId].CompleteGame();
            }
        }

        DeactivatePanel(gameId);
        if (interactable != null)
        {
            interactable.OnMiniGameCompleted();
        }
        else
        {
            Debug.LogError("Interactable is null during CompleteMiniGame!");
        }
        activeGameId = null;
        currentInteractable = null;
        Debug.Log($"MiniGame {gameId} 완료!");
    }

    private void ActivatePanel(string gameId)
    {
        foreach (var entry in miniGameEntries)
        {
            entry.panel.SetActive(entry.gameId == gameId);
        }
    }

    private void DeactivatePanel(string gameId)
    {
        foreach (var entry in miniGameEntries)
        {
            if (entry.gameId == gameId)
            {
                StartCoroutine(DisableAfterDelay(entry.panel, 3f));
                break;
            }
        }
    }

    private IEnumerator DisableAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public bool IsAnyMiniGameActive()
    {
        return activeGameId != null;
    }

    public void OnSocketCompleted(string gameId)
    {
        if (gameId == activeGameId)
        {
            CompleteMiniGame(gameId, currentInteractable);
        }
    }

    public void OnVentTrashCleaned()
    {
        if (activeGameId != "Vent") return;

        ventTrashCount++;
        Debug.Log($"Vent 쓰레기 처리됨: {ventTrashCount}/4");
        if (ventTrashCount >= 4)
        {
            CompleteMiniGame("Vent", currentInteractable);
        }
    }

    public int GetVentTrashCount()
    {
        return ventTrashCount;
    }
}