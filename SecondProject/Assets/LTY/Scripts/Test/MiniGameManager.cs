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
    [SerializeField] private TrashCan trashCan; // Glass 미니게임에서 사용할 TrashCan
    [SerializeField] private LightSocket lightSocket; // Light 미니게임에서 사용할 LightSocket
    [SerializeField] private PropellerSocket_LJH propellerSocket; // Vent 미니게임에서 사용할 PropellerSocket

    private Dictionary<string, IMiniGame> miniGames = new Dictionary<string, IMiniGame>();
    private Dictionary<string, GameObject> miniGamePanels = new Dictionary<string, GameObject>();
    private string activeGameId = null;
    private Interactable currentInteractable;

    private void Awake()
    {
        // 미니게임 초기화
        foreach (var entry in miniGameEntries)
        {
            if (entry.panel == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}의 패널이 설정되지 않았습니다!");
                continue;
            }

            miniGamePanels.Add(entry.gameId, entry.panel);
            entry.panel.SetActive(false); // 모든 패널 비활성화

            // miniGameScript가 없는 경우("Light", "Vent", "Glass")는 MiniGameManager가 직접 관리
            if (entry.miniGameScript == null)
            {
                if (entry.gameId == "Glass" && trashCan == null)
                {
                    Debug.LogError("TrashCan이 설정되지 않았습니다!");
                }
                if (entry.gameId == "Light" && lightSocket == null)
                {
                    Debug.LogError("LightSocket이 설정되지 않았습니다!");
                }
                if (entry.gameId == "Vent" && propellerSocket == null)
                {
                    Debug.LogError("PropellerSocket이 설정되지 않았습니다!");
                }
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

    // 미니게임 시작
    public void StartMiniGame(string gameId, Interactable interactable)
    {
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
        ActivatePanel(gameId);

        // MiniGameScript가 있는 경우 (MirrorCleaning, Wiring 등)
        if (miniGames.ContainsKey(gameId))
        {
            miniGames[gameId].StartMiniGame(interactable);
        }
        // MiniGameScript가 없는 경우 (Light, Vent, Glass)
        else
        {
            if (gameId == "Glass")
            {
                trashCan.ResetCount();
            }
            else if (gameId == "Vent")
            {
                propellerSocket.ResetVentTrashCount();
            }
            // Light는 초기화 필요 없음
        }

        Debug.Log($"MiniGame {gameId} 시작!");
    }

    // 미니게임 취소
    public void CancelMiniGame(string gameId)
    {
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        if (miniGames.ContainsKey(gameId))
        {
            miniGames[gameId].CancelGame();
        }

        DeactivatePanel(gameId);
        activeGameId = null;
        currentInteractable = null;
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
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}가 활성화되지 않았습니다!");
            return;
        }

        if (miniGames.ContainsKey(gameId))
        {
            miniGames[gameId].CompleteGame();
        }

        DeactivatePanel(gameId);
        interactable.OnMiniGameCompleted();
        activeGameId = null;
        currentInteractable = null;
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

    // 미니게임 완료 호출 (LightSocket, PropellerSocket_LJH, TrashCan에서 호출)
    public void OnSocketCompleted(string gameId)
    {
        if (gameId == activeGameId)
        {
            CompleteMiniGame(gameId, currentInteractable);
        }
    }
}