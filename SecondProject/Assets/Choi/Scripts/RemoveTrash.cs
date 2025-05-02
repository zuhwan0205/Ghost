using UnityEngine;

public class RemoveTrash : EventObject
{
    [Header("Can INFO")]
    [SerializeField] GameObject trashObj;

    private void Start()
    {
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
