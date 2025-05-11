using UnityEngine;

public class RemoveTrash : EventObject
{
    [Header("Can INFO")]
    [SerializeField] GameObject trashObj;
    [SerializeField] private bool coinChance = true;

    private void Start()
    {
        trashObj.SetActive(false);
        if(!MissionManager.Instance.activeMissionNames.Contains("TrashManager")) return;
        if (isWorking) trashObj.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();

        if (isWorking)
        {
            if (interactionTime > needTime)
            {
                AudioManager.Instance.Play("TrashCan");
                isWorking = false;
                GainCoin();
                trashObj.SetActive(false);
            }
        }

        if (interactionTime > needTime)
        {
            if (coinChance) GainCoin();
        }
    }

    private void GainCoin()
    {
        int rand = Random.Range(1, 4);
        GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().AddCoin(rand);
        AudioManager.Instance.Play("GetItem");
        coinChance = false;
    }
}
