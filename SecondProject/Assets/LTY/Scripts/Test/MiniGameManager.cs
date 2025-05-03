using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    // 미니게임과 패널을 매핑하는 구조체
    [System.Serializable]
    private struct MiniGameEntry
    {
        public string gameId;
        public GameObject panel;
        public MonoBehaviour miniGameScript; // IMiniGame을 구현한 스크립트
    }

    [SerializeField] private List<MiniGameEntry> miniGameEntries = new List<MiniGameEntry>();
    private Dictionary<string, IMiniGame> miniGames = new Dictionary<string, IMiniGame>();
    private string activeGameId = null;

    private void Awake()
    {
        // 미니게임 초기화
        foreach (var entry in miniGameEntries)
        {
            if (entry.miniGameScript == null || entry.panel == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}의 스크립트 또는 패널이 설정되지 않았습니다!");
                continue;
            }

            var miniGame = entry.miniGameScript as IMiniGame;
            if (miniGame == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}는 IMiniGame 인터페이스를 구현하지 않았습니다!");
                continue;
            }

            miniGames.Add(entry.gameId, miniGame);
            entry.panel.SetActive(false); // 모든 패널 비활성화
        }
    }

    // 미니게임 시작
    public void StartMiniGame(string gameId, Interactable interactable)
    {
        if (!miniGames.ContainsKey(gameId))
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
        ActivatePanel(gameId);
        miniGames[gameId].StartMiniGame(interactable);
        Debug.Log($"MiniGame {gameId} 시작!");
    }

    // 미니게임 취소
    public void CancelMiniGame(string gameId)
    {
        if (!miniGames.ContainsKey(gameId) || activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        miniGames[gameId].CancelGame();
        DeactivatePanel(gameId);
        activeGameId = null;
        Debug.Log($"MiniGame {gameId} 취소!");
    }

    // 모든 미니게임 취소
    public void CancelAllGames()
    {
        if (activeGameId != null)
        {
            CancelMiniGame(activeGameId);
        }
    }

    // 미니게임 완료
    public void CompleteMiniGame(string gameId, Interactable interactable)
    {
        if (!miniGames.ContainsKey(gameId) || activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        miniGames[gameId].CompleteGame();
        DeactivatePanel(gameId);
        interactable.OnMiniGameCompleted();
        activeGameId = null;
        Debug.Log($"MiniGame {gameId} 완료!");
    }

    // 패널 활성화
    private void ActivatePanel(string gameId)
    {
        foreach (var entry in miniGameEntries)
        {
            entry.panel.SetActive(entry.gameId == gameId);
        }
    }

    // 패널 비활성화
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

    // 지연 후 패널 비활성화
    private IEnumerator DisableAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    // 현재 활성화된 미니게임 확인
    public bool IsAnyMiniGameActive()
    {
        return activeGameId != null;
    }
}