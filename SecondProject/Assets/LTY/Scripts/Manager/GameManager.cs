using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 패턴
    public int missionQuota = 5; // 한 스테이지당 필요한 미션 수
    private int currentMissions = 0; // 완료한 미션 수
    private bool isGameOver = false;
    private bool isCleared = false;
    public DoorUnlockScript door; // 문 스크립트 참조

    void Awake()
    {
        // 싱글톤 설정
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

        // 미션 조건 충족 시 문 열기
        if (currentMissions >= missionQuota)
        {
            if (door != null)
            {
                door.UnlockDoor();
            }
        }
    }

    // 출구에서 스테이지 클리어 체크
    public void CheckStageClear()
    {
        if (isGameOver || isCleared) return;

        if (currentMissions >= missionQuota)
        {
            isCleared = true;
            Debug.Log("Stage Cleared!");
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
        Invoke("RestartStage", 2f); // 2초 후 재시작
    }

    // 다음 스테이지 로드
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