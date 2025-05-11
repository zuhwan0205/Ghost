using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // �̱��� ����
    private int missionQuota; // �� ���������� �ʿ��� �̼� �� (3��)
    private int currentMissions = 0; // �Ϸ��� �̼� ��
    private bool isGameOver = false;
    private bool isCleared = false;
    public DoorUnlockScript door; // �� ��ũ��Ʈ ����
    public int CurrentStage = 1;

    void Awake()
    {
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        if (CurrentStage == 1)
        {
            missionQuota = 3;
        }
        else if (CurrentStage == 2)
        {
            missionQuota = 5;
        }
        else
        {
            missionQuota = 7;
        }
    }

    void Start()
    {
       
    }

    // �̼� �Ϸ� �� ȣ��
    public void AddMissionProgress()
    {
        if (isGameOver || isCleared) return;

        currentMissions++;
        Debug.Log($"Missions Completed: {currentMissions}/{missionQuota}");

        // �̼� ���� ���� �� �� ����
        if (currentMissions >= missionQuota)
        {
            if (door != null)
            {
                door.UnlockDoor();
            }
        }
    }

    // �ⱸ���� �������� Ŭ���� üũ
    public void CheckStageClear()
    {
        if (isGameOver || isCleared) return;

        if (currentMissions >= missionQuota)
        {
            isCleared = true;
            Debug.Log("Stage Cleared!");
            CurrentStage++;
            Invoke("LoadNextStage", 2f); // 2�� �� ���� ��������
        }
        else
        {
            Debug.Log($"Need {missionQuota - currentMissions} more missions!");
        }
    }

    // ���� ���� ó��
    public void GameOver()
    {
        if (isGameOver || isCleared) return;

        isGameOver = true;
        Debug.Log("Game Over!");
        Invoke("RestartStage", 2f); // 2�� �� �����
    }

    // ���� �������� �ε�
    void LoadNextStage()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Game Completed!");
        }
        ResetStage();
    }

    // �������� �����
    void RestartStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetStage();
    }

    // �������� �ʱ�ȭ
    void ResetStage()
    {
        currentMissions = 0;
        isGameOver = false;
        isCleared = false;
    }

    // ���� �̼� ���൵ ��ȯ
    public int GetCurrentMissions()
    {
        return currentMissions;
    }

    // �̼� �Ҵ緮 ��ȯ
    public int GetMissionQuota()
    {
        return missionQuota;
    }
}