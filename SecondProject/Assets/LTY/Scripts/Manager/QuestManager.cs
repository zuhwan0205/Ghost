using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private Sprite questRewardPhoto; // ����Ʈ ���� ����

    public void CompleteQuest()
    {
        // ����Ʈ �Ϸ� ����
        if (PhoneManager.Instance != null && questRewardPhoto != null)
        {
            PhoneManager.Instance.AddPhoto(questRewardPhoto);
            Debug.Log("����Ʈ �Ϸ�: ���� �߰���");
        }
    }

    // �׽�Ʈ��
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // Q Ű�� ����Ʈ �Ϸ� �ùķ��̼�
            CompleteQuest();
    }
}