using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScheduleManager : MonoBehaviour
{
    [SerializeField]
    private List<TextMeshProUGUI> scheduleTexts;

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

        activeMissions = MissionManager.Instance.activeMissionNames;
        missionDesc    = MissionManager.Instance.missionDescriptions;

        for (int i = 0; i < scheduleTexts.Count; i++)
        {
            if (i < activeMissions.Count)
            {
                scheduleTexts[i].gameObject.SetActive(true);
                scheduleTexts[i].text = missionDesc[ activeMissions[i] ];
                scheduleTexts[i].color = Color.white; // 기본 색
            }
            else
            {
                scheduleTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void CompleteMission(string key)
    {
        
        int idx = activeMissions.IndexOf(key);
        if (idx < 0 || idx >= scheduleTexts.Count) return;
        
        var txt = scheduleTexts[idx];
        txt.text  = $"[Done] {missionDesc[key]}";
        txt.color = Color.green;
        
        GameManager.Instance.AddMissionProgress();
        int done  = GameManager.Instance.GetCurrentMissions();      
        int total = activeMissions.Count;                           

        if (done < total)
        {
            switch (done)
            {
                case 1:
                    PhoneManager.Instance?.AddMessage("나 보다 전에 일했던 사람이 알려줬는데 밤에 뭔가 기어다니는 소리가 자주 들렸다고 하더라고....");
                    break;
                case 2:
                    PhoneManager.Instance?.AddMessage("아 맞다, 내가 일할 때는 마네킹의 위치가 이상할 때도 있더라... 누가 옮기다 까먹었나...");
                    break;
                case 3:
                    PhoneManager.Instance?.AddMessage("저번에 마네킹 정리 하다가 기절한 적도 있어..");
                    break;
                case 4:
                    PhoneManager.Instance?.AddMessage("탐지기는 울리면 바로 꺼야해.. 안그러면 귀찮아 질거야");
                    break;
                case 5:
                    PhoneManager.Instance?.AddMessage("TV 고장나서 갑자기 켜질 수 도 있으니까 그것도 잘 끄고");
                    break;
                case 6:
                    PhoneManager.Instance?.AddMessage("이제 곧 퇴근 시간이네.. 화이팅!!");
                    break;
            }
        }
        else
        {
            PhoneManager.Instance?.AddMessage("퇴근 축하해!!! 이제 입구 쪽으로 가면 문이 열려 있을 거야.");
        }
    }
}