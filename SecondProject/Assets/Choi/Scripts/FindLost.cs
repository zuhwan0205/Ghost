using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FindLost : EventObject
{
    [Header("Lost INFO")]
    public bool isFake;
    private Light2D light;

    private void Start()
    {
        light = GetComponentInChildren<Light2D>();
    }

    protected override void Update()
    {
        if (isWorking)
        {
            base.Update();

            if (isFake) light.enabled = true;

            if (interactionTime > needTime)
            {
                if (!isFake)
                {
                    isWorking = false;
                } else
                {
                    isWorking = false;
                    Debug.Log("��¥ �нǹ� �̺�Ʈ �߻�!");
                }
            }

            if (!isWorking) 
            { 
                gameObject.GetComponent<SpriteRenderer>().enabled = false; 
                light.enabled = false; 
            }
        }
    }
}
