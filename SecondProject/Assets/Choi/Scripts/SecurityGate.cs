using UnityEngine;
using Unity.Collections;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class SecurityGate : EventObject
{
    private Animator anim;
    private RandomObjManager rom;
    private AudioManager aud;
    private AudioSource alarm;
    private bool alarmOn = false;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rom = GameObject.Find("RandObjManager").GetComponent<RandomObjManager>();
        aud = AudioManager.Instance;
    }

    protected override void Update()
    {
        base.Update();

        if (!anim.GetBool("AlramOn")) 
        { 
            if (isWorking) anim.SetBool("StandBy", true); 
            else anim.SetBool("StandBy", false); 
        }

        // needTime까지 상호작용 완료시 해제
        if (detected && interactionTime > needTime) 
        { 
            isWorking = false;
            alarmOn = false;
            if (alarm.isPlaying) { alarm.Stop(); alarm.loop = false; }
            anim.SetBool("AlramOn", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 켜진상태로 닿으면 알람 시작
        if (isWorking && collision.gameObject.CompareTag("Player"))
        {
            if (!alarmOn)
            {
                alarm = aud.PlayLoopSFX("SecurityAlarm", transform.position);
                alarmOn = true;
            }
            anim.SetBool("StandBy", false);
            anim.SetBool("AlramOn", true);
        }
    }
}
