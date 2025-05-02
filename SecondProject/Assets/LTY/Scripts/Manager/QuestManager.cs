using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private Sprite questRewardPhoto; // 퀘스트 보상 사진

    public void CompleteQuest()
    {
        // 퀘스트 완료 로직
        if (PhoneManager.Instance != null && questRewardPhoto != null)
        {
            PhoneManager.Instance.AddPhoto(questRewardPhoto);
            Debug.Log("퀘스트 완료: 사진 추가됨");
        }
    }

    // 테스트용
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // Q 키로 퀘스트 완료 시뮬레이션
            CompleteQuest();
    }
}