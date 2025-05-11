using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public GameObject Mannequin;
    public GameObject Glass;
    public GameObject Vent;
    public GameObject Radio;
    public GameObject Carpet;
    public GameObject MirrorSystem;
    public GameObject WireBoxSystem;
    public GameObject LostPropertyManager;
    public GameObject TrashManager;

    public bool OnLostManager;
    public bool OnTrashManager;
    

    public static MissionManager Instance;

    private List<GameObject> allMiniGames;
    public List<string> activeMissionNames = new List<string>();
    
    public Dictionary<string, string> missionDescriptions = new Dictionary<string, string>();
    
    private readonly Dictionary<int, int> stageMissionCount = new Dictionary<int, int>
    {
        { 1, 3 },
        { 2, 5 },
        { 3, 7 }
    };

    private void Awake()
    {
        Instance = this;

        allMiniGames = new List<GameObject>
        {
            Mannequin, Glass, Vent,
            Radio, Carpet, MirrorSystem, WireBoxSystem,
            LostPropertyManager, TrashManager
        };
        
        missionDescriptions = new Dictionary<string, string>
        {
            { "Mannequin", "Clean the mannequin." },
            { "Glass", "Clean up the broken glass." },
            { "Vent", "Fix the ventilation system." },
            { "Radio", "Tune the radio to zero." },
            { "Carpet", "Organize the carpet." },
            { "MirrorSystem", "Wipe the glass clean." },
            { "WireBoxSystem", "Organize the tangled wires." },
            {"LostPropertyManager","Find Lost Property."},
            {"TrashManager","Clear the trash."}
        };
        
        int count = stageMissionCount.TryGetValue(GameManager.Instance.CurrentStage, out var c) ? c : 3;
        
        ActivateRandomMiniGames(count);
        
        TrashManager.SetActive(true);
        
        //if (GameManager.Instance.CurrentStage == 1)
        // {
        //     ActivateRandomMiniGames(3);
        // }
        // else if (GameManager.Instance.CurrentStage == 2)
        // {
        //     ActivateRandomMiniGames(5);
        // }
        // else if (GameManager.Instance.CurrentStage == 3)
        // {
        //     ActivateRandomMiniGames(7);
        // }
        

        if (activeMissionNames.Contains("LostPropertyManager"))
        {
            OnLostManager = true;
        }
        else
        {
            OnLostManager = false;
        }
    }

    private void ActivateRandomMiniGames(int count)
    {
        activeMissionNames.Clear();

        foreach (var miniGame in allMiniGames)
        {
            if (miniGame != null)
                miniGame.SetActive(false);
        }

        List<GameObject> shuffled = new List<GameObject>(allMiniGames);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[rand]) = (shuffled[rand], shuffled[i]);
        }

        for (int i = 0; i < count && i < shuffled.Count; i++)
        {
            if (shuffled[i] != null)
            {
                shuffled[i].SetActive(true);
                activeMissionNames.Add(shuffled[i].name);
            }
        }
    }
}