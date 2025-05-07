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
        }
        else if (key == activeMissions[1])
        {
            scheduleText02.text = $"[Done] {missionDesc[key]}";
            scheduleText02.color = Color.green;
        }
        else if (key == activeMissions[2])
        {
            scheduleText03.text = $"[Done] {missionDesc[key]}";
            scheduleText03.color = Color.green;
        }
    }
}