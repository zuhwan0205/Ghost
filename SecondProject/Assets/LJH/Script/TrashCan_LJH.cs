using System;
using UnityEngine;

public class TrashCan : MonoBehaviour
{
    private int CleanCount = 0;
    public int VentTrashCount = 0;
    
    public static TrashCan Instance;
    
    //이벤트
    public static event Action OnGlassMiniGameEnd;

    private void Awake()
    {
        Instance = this;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌 : " + other.name);
        if (other.CompareTag("Glass"))
        {
            Destroy(other.gameObject);
            CleanCount++;
        }

        if (CleanCount == 8)
        {
            CompleteGlassMission();
        }

        if (other.CompareTag("Trash"))
        {
            Destroy(other.gameObject);
            CompleteTrashMission();
        }
        
        if(other.CompareTag("Vent_Trash"))
        {
            Destroy(other.gameObject);
            VentTrashCount++;
        }
    }

    void CompleteGlassMission()
    {
        OnGlassMiniGameEnd?.Invoke();
    }

    void CompleteTrashMission()
    {
        //미니게임 패널 비활성화
        //미션 상호작용 하는 인게임 오브젝트 비활성화
    }

    
}
