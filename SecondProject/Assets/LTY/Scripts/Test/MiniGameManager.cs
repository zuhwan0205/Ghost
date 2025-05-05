using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    // �̴ϰ��Ӱ� �г��� �����ϴ� ����ü
    [System.Serializable]
    private struct MiniGameEntry
    {
        public string gameId;
        public GameObject panel;
        public MonoBehaviour miniGameScript; // IMiniGame�� ������ ��ũ��Ʈ
    }

    [SerializeField] private List<MiniGameEntry> miniGameEntries = new List<MiniGameEntry>();
    [SerializeField] private TrashCan trashCan; // Glass �̴ϰ��ӿ��� ����� TrashCan
    [SerializeField] private LightSocket lightSocket; // Light �̴ϰ��ӿ��� ����� LightSocket
    [SerializeField] private PropellerSocket_LJH propellerSocket; // Vent �̴ϰ��ӿ��� ����� PropellerSocket

    private Dictionary<string, IMiniGame> miniGames = new Dictionary<string, IMiniGame>();
    private Dictionary<string, GameObject> miniGamePanels = new Dictionary<string, GameObject>();
    private string activeGameId = null;
    private Interactable currentInteractable;

    private void Awake()
    {
        // �̴ϰ��� �ʱ�ȭ
        foreach (var entry in miniGameEntries)
        {
            if (entry.panel == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}�� �г��� �������� �ʾҽ��ϴ�!");
                continue;
            }

            miniGamePanels.Add(entry.gameId, entry.panel);
            entry.panel.SetActive(false); // ��� �г� ��Ȱ��ȭ

            // miniGameScript�� ���� ���("Light", "Vent", "Glass")�� MiniGameManager�� ���� ����
            if (entry.miniGameScript == null)
            {
                if (entry.gameId == "Glass" && trashCan == null)
                {
                    Debug.LogError("TrashCan�� �������� �ʾҽ��ϴ�!");
                }
                if (entry.gameId == "Light" && lightSocket == null)
                {
                    Debug.LogError("LightSocket�� �������� �ʾҽ��ϴ�!");
                }
                if (entry.gameId == "Vent" && propellerSocket == null)
                {
                    Debug.LogError("PropellerSocket�� �������� �ʾҽ��ϴ�!");
                }
                continue;
            }

            var miniGame = entry.miniGameScript as IMiniGame;
            if (miniGame == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}�� IMiniGame �������̽��� �������� �ʾҽ��ϴ�!");
                continue;
            }

            miniGames.Add(entry.gameId, miniGame);
        }
    }

    // �̴ϰ��� ����
    public void StartMiniGame(string gameId, Interactable interactable)
    {
        if (!miniGamePanels.ContainsKey(gameId))
        {
            Debug.LogError($"MiniGame ID {gameId}�� ��ϵ��� �ʾҽ��ϴ�!");
            return;
        }

        if (activeGameId != null)
        {
            Debug.LogWarning("�ٸ� �̴ϰ����� �̹� ���� ���Դϴ�!");
            return;
        }

        activeGameId = gameId;
        currentInteractable = interactable;
        ActivatePanel(gameId);

        // MiniGameScript�� �ִ� ��� (MirrorCleaning, Wiring ��)
        if (miniGames.ContainsKey(gameId))
        {
            miniGames[gameId].StartMiniGame(interactable);
        }
        // MiniGameScript�� ���� ��� (Light, Vent, Glass)
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
            // Light�� �ʱ�ȭ �ʿ� ����
        }

        Debug.Log($"MiniGame {gameId} ����!");
    }

    // �̴ϰ��� ���
    public void CancelMiniGame(string gameId)
    {
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}�� Ȱ��ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        if (miniGames.ContainsKey(gameId))
        {
            miniGames[gameId].CancelGame();
        }

        DeactivatePanel(gameId);
        activeGameId = null;
        currentInteractable = null;
        Debug.Log($"MiniGame {gameId} ���!");
    }

    // ��� �̴ϰ��� ���
    public void CancelAllGames()
    {
        if (activeGameId != null)
        {
            CancelMiniGame(activeGameId);
        }
    }

    // �̴ϰ��� �Ϸ�
    public void CompleteMiniGame(string gameId, Interactable interactable)
    {
        if (activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}�� Ȱ��ȭ���� �ʾҽ��ϴ�!");
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
        Debug.Log($"MiniGame {gameId} �Ϸ�!");
    }

    // �г� Ȱ��ȭ
    private void ActivatePanel(string gameId)
    {
        foreach (var entry in miniGameEntries)
        {
            entry.panel.SetActive(entry.gameId == gameId);
        }
    }

    // �г� ��Ȱ��ȭ
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

    // ���� �� �г� ��Ȱ��ȭ
    private IEnumerator DisableAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    // ���� Ȱ��ȭ�� �̴ϰ��� Ȯ��
    public bool IsAnyMiniGameActive()
    {
        return activeGameId != null;
    }

    // �̴ϰ��� �Ϸ� ȣ�� (LightSocket, PropellerSocket_LJH, TrashCan���� ȣ��)
    public void OnSocketCompleted(string gameId)
    {
        if (gameId == activeGameId)
        {
            CompleteMiniGame(gameId, currentInteractable);
        }
    }
}