using UnityEngine;
using Unity.Collections;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class SecurityGate : EventObject
{
    private Animator anim;
    private RandomObjManager rom;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rom = GameObject.Find("RandObjManager").GetComponent<RandomObjManager>();
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
            anim.SetBool("AlramOn", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("1");
        // 켜진상태로 닿으면 알람 시작
        if (isWorking && collision.gameObject.CompareTag("Player"))
        {
            anim.SetBool("StandBy", false);
            anim.SetBool("AlramOn", true);
        }
    }
}
