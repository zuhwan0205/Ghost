using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScheduleManager : MonoBehaviour
{
    public TextMeshProUGUI  scheduleText01;
    public TextMeshProUGUI scheduleText02;
    public TextMeshProUGUI scheduleText03;
    
    private List<string> activeMissions;
    private Dictionary<string, string> missionDesc;

    private int MissionCount = 0;
    
    public static ScheduleManager Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        StartCoroutine(InitSchedule());
    }

    IEnumerator InitSchedule()
    {
        yield return null;
        yield return null;

        var missionManager = MissionManager.Instance;
        
        activeMissions = missionManager.activeMissionNames;
        missionDesc = missionManager.missionDescriptions;

        if (activeMissions.Count >= 3)
        {
            scheduleText01.text = missionDesc[activeMissions[0]];
            scheduleText02.text = missionDesc[activeMissions[1]];
            scheduleText03.text = missionDesc[activeMissions[2]];
        }
        else
        {
            scheduleText01.text = "Failed to load mission info.";
            scheduleText02.text = "";
            scheduleText03.text = "";
        }
    }
    
    public void CompleteMission(string key)
    {
        if (activeMissions == null) return;

        if (key == activeMissions[0])
        {
            scheduleText01.text = $"[Done] {missionDesc[key]}";
            scheduleText01.color = Color.green;
            MissionCount = 1;
        }
        else if (key == activeMissions[1])
        {
            scheduleText02.text = $"[Done] {missionDesc[key]}";
            scheduleText02.color = Color.green;
            MissionCount = 2;
        }
        else if (key == activeMissions[2])
        {
            scheduleText03.text = $"[Done] {missionDesc[key]}";
            scheduleText03.color = Color.green;
            MissionCount = 3;
        }

        if (MissionCount == 1)
        {
            PhoneManager.Instance?.AddMessage("나 보다 전에 일했던 사람이 알려줬는데 밤에 뭔가 기어다니는 소리가 자주 들렸다고 하더라고....");
        }
        else if (MissionCount == 2)
        {
            PhoneManager.Instance?.AddMessage("아 맞다, 내가 일할 때는 마네킹의 위치가 이상할 때도 있더라... 누가 옮기다 까먹었나...");
        }
        else if(MissionCount == 3)
        {
            PhoneManager.Instance?.AddMessage("이제 곧 끝날 시간이네! 퇴근 축하해!!! 이제 입구 쪽으로 가면 문이 열려 있을 거야.");
        }
    }
}