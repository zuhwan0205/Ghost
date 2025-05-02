using System;
using System.Collections;
using UnityEngine;

public class MiniGameManagaer_LJH : MonoBehaviour
{
    //패널
    public GameObject MG_MannequinPanel;
    public GameObject MG_GlassPanel;
    public GameObject MG_LightPanel;
    public GameObject MG_VentPanel;
    public GameObject MG_RadioPanel;
    public GameObject MG_CarpetPanel;
    
    //오브젝트
    public GameObject MG_Mannequin;
    public GameObject MG_Glass;
    public GameObject MG_Light;
    public GameObject MG_Vent;
    public GameObject MG_Radio;
    public GameObject MG_Carpet;
    
    //
    //public GameObject UI_Interaction;

    private void OnEnable()
    {
        TrashCan.OnGlassMiniGameEnd += GlassPanelOff;
        Stain.OnMannequinEnd += MannequinPanelOff;
        PropellerSocket_LJH.OnVentEnd += VentPanelOff;
        MG_Radio_LJH.OnRadioEnd += RadioPanelOff;
        LightSocket.OnLightEnd += LightPanelOff;
        Dust_LJH.OnCarpetEnd += CarpetPanelOff;
        MannequinInteract.OnMannequinInteracted += MannequinPanelOn;
        CarpetInteract.OnCarpetInteract += CarpetPanelOn;
        GlassInteract.OnGlassInteract += GlassPanelOn;
        LightInteract.OnLightInteract += LightPanelOn;
        RadioInteract.OnRadioInteract += RadioPanelOn;
        VentInteract.OnVentInteract += VentPanelOn;
    }

    private void OnDisable()
    {
        TrashCan.OnGlassMiniGameEnd -= GlassPanelOff;
        Stain.OnMannequinEnd -= MannequinPanelOff;
        PropellerSocket_LJH.OnVentEnd -= VentPanelOff;
        MG_Radio_LJH.OnRadioEnd -= RadioPanelOff;
        LightSocket.OnLightEnd -= LightPanelOff;
        Dust_LJH.OnCarpetEnd -= CarpetPanelOff;
        MannequinInteract.OnMannequinInteracted -= MannequinPanelOn;
        CarpetInteract.OnCarpetInteract -= CarpetPanelOn;
        GlassInteract.OnGlassInteract -= GlassPanelOn;
        LightInteract.OnLightInteract -= LightPanelOn;
        RadioInteract.OnRadioInteract -= RadioPanelOn;
        VentInteract.OnVentInteract -= VentPanelOn;
    }

    void GlassPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_GlassPanel, 3));
        Destroy(MG_Glass);
        //인게임 거울 교체
    }

    void MannequinPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_MannequinPanel,3));
        Destroy(MG_Mannequin);
    }

    void VentPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_VentPanel, 3f));
        Destroy(MG_Vent);
    }

    void RadioPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_RadioPanel, 3f));
        Destroy(MG_Radio);
        //UI_Interaction.SetActive(true);
        //무서운 소리 잠깐 재생
    }

    void LightPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_LightPanel, 3f));
        Destroy(MG_Light);
        //인게임 조명 켜기
    }

    void CarpetPanelOff()
    {
        StartCoroutine(DisableAfterDelay(MG_CarpetPanel, 3f));
        Destroy(MG_Carpet);
    }
    
    private IEnumerator DisableAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    void MannequinPanelOn()
    {
        DisableAllPanels();
        MG_MannequinPanel.SetActive(true);
    }

    void GlassPanelOn()
    {
        DisableAllPanels();
        MG_GlassPanel.SetActive(true);
    }

    void LightPanelOn()
    {
        DisableAllPanels();
        MG_LightPanel.SetActive(true);
    }

    void RadioPanelOn()
    {
        DisableAllPanels();
        MG_RadioPanel.SetActive(true);
        //UI_Interaction.SetActive(false);
    }

    void VentPanelOn()
    {
        DisableAllPanels();
        MG_VentPanel.SetActive(true);
    }

    void CarpetPanelOn()
    {
        DisableAllPanels();
        MG_CarpetPanel.SetActive(true);
    }
    
    void DisableAllPanels()
    {
        MG_MannequinPanel.SetActive(false);
        MG_GlassPanel.SetActive(false);
        MG_LightPanel.SetActive(false);
        MG_VentPanel.SetActive(false);
        MG_RadioPanel.SetActive(false);
        MG_CarpetPanel.SetActive(false);
        
    }
}