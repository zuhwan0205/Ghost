using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public GameObject Mannequin;
    public GameObject Glass;
    public GameObject Light;
    public GameObject Vent;
    public GameObject Radio;
    public GameObject Carpet;
    public GameObject MirrorSystem;
    public GameObject WireBoxSystem;

    public static MissionManager Instance;

    private List<GameObject> allMiniGames;
    public List<string> activeMissionNames = new List<string>();
    
    public Dictionary<string, string> missionDescriptions = new Dictionary<string, string>();

    private void Awake()
    {
        Instance = this;

        allMiniGames = new List<GameObject>
        {
            Mannequin, Glass, Light, Vent,
            Radio, Carpet, MirrorSystem, WireBoxSystem
        };
        
        missionDescriptions = new Dictionary<string, string>
        {
            { "Mannequin", "Clean the mannequin." },
            { "Glass", "Clean up the broken glass." },
            { "Light", "Replace the light bulb." },
            { "Vent", "Fix the ventilation system." },
            { "Radio", "Tune the radio to zero." },
            { "Carpet", "Organize the carpet." },
            { "MirrorSystem", "Wipe the glass clean." },
            { "WireBoxSystem", "Organize the tangled wires." }
        };
    }

    private void Start()
    {
        ActivateRandomMiniGames(3);
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