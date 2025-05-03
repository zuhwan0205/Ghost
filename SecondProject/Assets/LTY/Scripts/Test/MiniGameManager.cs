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
    private Dictionary<string, IMiniGame> miniGames = new Dictionary<string, IMiniGame>();
    private string activeGameId = null;

    private void Awake()
    {
        // �̴ϰ��� �ʱ�ȭ
        foreach (var entry in miniGameEntries)
        {
            if (entry.miniGameScript == null || entry.panel == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}�� ��ũ��Ʈ �Ǵ� �г��� �������� �ʾҽ��ϴ�!");
                continue;
            }

            var miniGame = entry.miniGameScript as IMiniGame;
            if (miniGame == null)
            {
                Debug.LogError($"MiniGame {entry.gameId}�� IMiniGame �������̽��� �������� �ʾҽ��ϴ�!");
                continue;
            }

            miniGames.Add(entry.gameId, miniGame);
            entry.panel.SetActive(false); // ��� �г� ��Ȱ��ȭ
        }
    }

    // �̴ϰ��� ����
    public void StartMiniGame(string gameId, Interactable interactable)
    {
        if (!miniGames.ContainsKey(gameId))
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
        ActivatePanel(gameId);
        miniGames[gameId].StartMiniGame(interactable);
        Debug.Log($"MiniGame {gameId} ����!");
    }

    // �̴ϰ��� ���
    public void CancelMiniGame(string gameId)
    {
        if (!miniGames.ContainsKey(gameId) || activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}�� Ȱ��ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        miniGames[gameId].CancelGame();
        DeactivatePanel(gameId);
        activeGameId = null;
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
        if (!miniGames.ContainsKey(gameId) || activeGameId != gameId)
        {
            Debug.LogWarning($"MiniGame ID {gameId}�� Ȱ��ȭ���� �ʾҽ��ϴ�!");
            return;
        }

        miniGames[gameId].CompleteGame();
        DeactivatePanel(gameId);
        interactable.OnMiniGameCompleted();
        activeGameId = null;
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
}