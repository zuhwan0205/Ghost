using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 패턴
    public int missionQuota = 5; // 한 스테이지당 필요한 미션 수
    private int currentMissions = 0; // 완료한 미션 수
    private bool isGameOver = false;
    private bool isCleared = false;

    void Awake()
    {
        // 싱글톤 설정: GameManager 하나만 존재
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 미션 완료 시 호출
    public void AddMissionProgress()
    {
        if (isGameOver || isCleared) return;

        currentMissions++;
        Debug.Log($"Missions Completed: {currentMissions}/{missionQuota}");

        // UI 업데이트 (나중에 UIManager와 연동)
        // UIManager.Instance.UpdateMissionUI(currentMissions, missionQuota);
    }

    // 출구에서 스테이지 클리어 체크
    public void CheckStageClear()
    {
        if (isGameOver || isCleared) return;

        if (currentMissions >= missionQuota)
        {
            isCleared = true;
            Debug.Log("Stage Cleared!");
            // UI 표시 또는 다음 스테이지로 전환
            // UIManager.Instance.ShowClearScreen();
            Invoke("LoadNextStage", 2f); // 2초 후 다음 스테이지
        }
        else
        {
            Debug.Log($"Need {missionQuota - currentMissions} more missions!");
        }
    }

    // 게임 오버 처리
    public void GameOver()
    {
        if (isGameOver || isCleared) return;

        isGameOver = true;
        Debug.Log("Game Over!");
        // UI 표시
        // UIManager.Instance.ShowGameOverScreen();
        Invoke("RestartStage", 2f); // 2초 후 재시작
    }

    // 다음 스테이지 로드
    void LoadNextStage()
    {
        // 현재 씬 인덱스 가져와 다음 씬 로드 (씬 이름으로도 가능)
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("Game Completed!");
            // 게임 종료 UI 또는 메인 메뉴로
        }
        ResetStage();
    }

    // 스테이지 재시작
    void RestartStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetStage();
    }

    // 스테이지 초기화
    void ResetStage()
    {
        currentMissions = 0;
        isGameOver = false;
        isCleared = false;
    }

    // 현재 미션 진행도 반환
    public int GetCurrentMissions()
    {
        return currentMissions;
    }

    // 미션 할당량 반환
    public int GetMissionQuota()
    {
        return missionQuota;
    }
}