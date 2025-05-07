using UnityEngine;

public class EventTrigger : Interactable
{
    [SerializeField] private string targetRoomID = "3-1";
    private EventMonsterChase[] eventMonsters;

    private void Start()
    {
        // 씬에 있는 모든 EventMonsterChase 컴포넌트를 찾아 배열로 저장
        eventMonsters = FindObjectsByType<EventMonsterChase>(FindObjectsSortMode.None);

        if (eventMonsters.Length == 0)
        {
            Debug.LogWarning("[ExampleTrigger] EventMonsterChase 오브젝트들을 찾을 수 없습니다.");
        }
    }

    public override void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Debug.Log("[ExampleTrigger] 입력 감지됨. 모든 몬스터 호출 시작.");
        foreach (var monster in EventMonsterChase.allMonsters)
        {
            if (monster != null)
            {
                monster.TriggerMoveToRoom(targetRoomID);
            }
        }

    }
}
