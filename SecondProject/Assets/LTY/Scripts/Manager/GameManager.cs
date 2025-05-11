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
    private LevelTester levelTester;

    void Awake()
    {
        // �̱��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1) 씬 이름 기준으로 미션 개수 매핑
        switch (scene.name)
        {
            case "Stage1":
                missionQuota = 3;       //3
                CurrentStage = 1;
                levelTester.SetLevel(1);
                break;
            case "Stage2":
                missionQuota = 5;
                CurrentStage = 2;
                levelTester.SetLevel(2);
                break;
            case "Stage3":
                missionQuota = 7;
                CurrentStage = 3;
                levelTester.SetLevel(3);
                break;
            default:
                Debug.LogWarning($"Unknown scene: {scene.name}, defaulting to 3");
                missionQuota = 3;
                break;
        }
        // 2) 진행 상황 초기화
        ResetStage();
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
        if (CurrentStage < 3)
        {
            //CurrentStage++;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            // 3단계 클리어 시 엔딩으로
            //SceneManager.LoadScene("EndingScene");
        }
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