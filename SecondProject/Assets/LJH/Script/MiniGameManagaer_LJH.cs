using System;
using System.Collections;
using UnityEngine;

public class MiniGameManagaer_LJH : MonoBehaviour
{
    //패널
    public GameObject MG_MannequinPanel;
    public GameObject MG_GlassPanel;
    public GameObject MG_VentPanel;
    public GameObject MG_RadioPanel;
    public GameObject MG_CarpetPanel;
    
    //오브젝트
    public GameObject MG_Mannequin;
    public GameObject MG_Glass;
    public GameObject MG_Vent;
    public GameObject MG_Radio;
    public GameObject MG_Carpet;
    public GameObject AfterMannequin;
    
    //
    //public GameObject UI_Interaction;
    public bool isMiniGaming = false;
    private float panelCooldown = 0f;
    public static MiniGameManagaer_LJH Instance;
    
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        TrashCan.OnGlassMiniGameEnd += GlassPanelOff;
        Stain.OnMannequinEnd += MannequinPanelOff;
        PropellerSocket_LJH.OnVentEnd += VentPanelOff;
        MG_Radio_LJH.OnRadioEnd += RadioPanelOff;
        Dust_LJH.OnCarpetEnd += CarpetPanelOff;
        MannequinInteract.OnMannequinInteracted += MannequinPanelOn;
        CarpetInteract.OnCarpetInteract += CarpetPanelOn;
        GlassInteract.OnGlassInteract += GlassPanelOn;
        RadioInteract.OnRadioInteract += RadioPanelOn;
        VentInteract.OnVentInteract += VentPanelOn;
        LostManager.OnEndLostGame += EndLostGame;
        TrashManager.OnEndTrashGame += EndTrashGame;
    }

    private void OnDisable()
    {
        TrashCan.OnGlassMiniGameEnd -= GlassPanelOff;
        Stain.OnMannequinEnd -= MannequinPanelOff;
        PropellerSocket_LJH.OnVentEnd -= VentPanelOff;
        MG_Radio_LJH.OnRadioEnd -= RadioPanelOff;
        Dust_LJH.OnCarpetEnd -= CarpetPanelOff;
        MannequinInteract.OnMannequinInteracted -= MannequinPanelOn;
        CarpetInteract.OnCarpetInteract -= CarpetPanelOn;
        GlassInteract.OnGlassInteract -= GlassPanelOn;
        RadioInteract.OnRadioInteract -= RadioPanelOn;
        VentInteract.OnVentInteract -= VentPanelOn;
        LostManager.OnEndLostGame -= EndLostGame;
        TrashManager.OnEndTrashGame -= EndTrashGame;
    }

    private void Update()
    {
        if (panelCooldown > 0) panelCooldown -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E) && isMiniGaming && panelCooldown <= 0f)
        {
            Debug.Log(isMiniGaming);
            DisableAllPanels();
        }
    }

    void GlassPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_GlassPanel, 3));
        Destroy(MG_Glass);
        //인게임 거울 교체
        ScheduleManager.Instance.CompleteMission("Glass");
    }

    void MannequinPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_MannequinPanel,3));
        Destroy(MG_Mannequin);
        AfterMannequin.SetActive(true);
        ScheduleManager.Instance.CompleteMission("Mannequin");
    }

    void VentPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_VentPanel, 3f));
        Destroy(MG_Vent);
        ScheduleManager.Instance.CompleteMission("Vent");
    }

    void RadioPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_RadioPanel, 3f));
        Destroy(MG_Radio);
        //UI_Interaction.SetActive(true);
        //무서운 소리 잠깐 재생
        ScheduleManager.Instance.CompleteMission("Radio");
    }

    void CarpetPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_CarpetPanel, 3f));
        Destroy(MG_Carpet);
        ScheduleManager.Instance.CompleteMission("Carpet");
    }
    
    private IEnumerator DisableAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
        isMiniGaming = false;
    }

    void MannequinPanelOn()
    {
        DisableAllPanels();
        MG_MannequinPanel.SetActive(true);
        isMiniGaming = true;
        panelCooldown = 0.2f;
    }

    void GlassPanelOn()
    {
        DisableAllPanels();
        MG_GlassPanel.SetActive(true);
        isMiniGaming = true;
        panelCooldown = 0.2f;
    }

    void RadioPanelOn()
    {
        DisableAllPanels();
        MG_RadioPanel.SetActive(true);
        //UI_Interaction.SetActive(false);
        isMiniGaming = true;
        panelCooldown = 0.2f;
    }

    void VentPanelOn()
    {
        DisableAllPanels();
        MG_VentPanel.SetActive(true);
        isMiniGaming = true;
        panelCooldown = 0.2f;
    }

    void CarpetPanelOn()
    {
        DisableAllPanels();
        MG_CarpetPanel.SetActive(true);
        isMiniGaming = true;
        panelCooldown = 0.2f;
    }
    
    void DisableAllPanels()
    {
        if (isMiniGaming == true)
        {
            MG_MannequinPanel.SetActive(false);
            MG_GlassPanel.SetActive(false);
            MG_VentPanel.SetActive(false);
            MG_RadioPanel.SetActive(false);
            MG_CarpetPanel.SetActive(false);
            isMiniGaming = false;
        }
    }

    void EndLostGame()
    {
        ScheduleManager.Instance.CompleteMission("LostPropertyManager");
    }

    void EndTrashGame()
    {
        ScheduleManager.Instance.CompleteMission("TrashManager");
    }
}