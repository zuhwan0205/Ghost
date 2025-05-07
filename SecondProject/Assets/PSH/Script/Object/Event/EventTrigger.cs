using UnityEngine;

public class EventTrigger : Interactable
{
    [SerializeField] private string targetRoomID = "3-1";
    private EventMonsterChase[] eventMonsters;

    private void Start()
    {
        // ���� �ִ� ��� EventMonsterChase ������Ʈ�� ã�� �迭�� ����
        eventMonsters = FindObjectsByType<EventMonsterChase>(FindObjectsSortMode.None);

        if (eventMonsters.Length == 0)
        {
            Debug.LogWarning("[ExampleTrigger] EventMonsterChase ������Ʈ���� ã�� �� �����ϴ�.");
        }
    }

    public override void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Debug.Log("[ExampleTrigger] �Է� ������. ��� ���� ȣ�� ����.");
        foreach (var monster in EventMonsterChase.allMonsters)
        {
            if (monster != null)
            {
                monster.TriggerMoveToRoom(targetRoomID);
            }
        }

    }
}
