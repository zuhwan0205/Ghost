using UnityEngine;

public class FindLost : EventObject
{
    [Header("Lost INFO")]
    public bool isFake;

    protected override void Update()
    {
        if (isWorking)
        {
            base.Update();

            if (interactionTime > needTime)
            {
                if (!isFake)
                {
                    isWorking = false;
                } else
                {
                    isWorking = false;
                    Debug.Log("가짜 분실물 이벤트 발생!");
                }
            }

            if (!isWorking) gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
