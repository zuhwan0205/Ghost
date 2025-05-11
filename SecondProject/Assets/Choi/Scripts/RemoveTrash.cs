using UnityEngine;

public class RemoveTrash : EventObject
{
    [Header("Can INFO")]
    [SerializeField] GameObject trashObj;

    private void Start()
    {
        trashObj.SetActive(false);
        if(!MissionManager.Instance.activeMissionNames.Contains("TrashManager")) return;
        if (isWorking) trashObj.SetActive(true);
    }

    protected override void Update()
    {
        if (isWorking)
        {
            base.Update();

            if (interactionTime > needTime)
            {
                isWorking = false;
                trashObj.SetActive(false);
            }
        }
    }
}
