using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FindLost : EventObject
{
    [Header("Lost INFO")]
    public bool isFake;
    private Light2D light;
    private RandomObjManager rom;
    private AudioManager aud;

    private void Start()
    {
        light = GetComponentInChildren<Light2D>();
        rom = GameObject.Find("RandObjManager").GetComponent<RandomObjManager>();
        aud = AudioManager.Instance;
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
                    aud.Play("Take");
                    isWorking = false;
                } else
                {
                    aud.Play("FakeItem");
                    isWorking = false;
                    rom.Mtime -= 15;
                    rom.Stime -= 15;
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
