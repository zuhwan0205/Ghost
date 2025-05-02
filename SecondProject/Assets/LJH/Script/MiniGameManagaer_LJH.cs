using System;
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
        MG_GlassPanel.SetActive(false);
        Destroy(MG_Glass);
        //인게임 거울 교체
    }

    void MannequinPanelOff()
    {
        MG_MannequinPanel.SetActive(false);
        Destroy(MG_Mannequin);
    }

    void VentPanelOff()
    {
        MG_VentPanel.SetActive(false);
        Destroy(MG_Vent);
    }

    void RadioPanelOff()
    {
        MG_RadioPanel.SetActive(false);
        Destroy(MG_Radio);
        //무서운 소리 잠깐 재생
    }

    void LightPanelOff()
    {
        MG_LightPanel.SetActive(false);
        Destroy(MG_Light);
        //인게임 조명 켜기
    }

    void CarpetPanelOff()
    {
        MG_CarpetPanel.SetActive(false);
        Destroy(MG_Carpet);
    }

    void MannequinPanelOn()
    {
        MG_MannequinPanel.SetActive(true);
    }

    void GlassPanelOn()
    {
        MG_GlassPanel.SetActive(true);
    }

    void LightPanelOn()
    {
        MG_LightPanel.SetActive(true);
    }

    void RadioPanelOn()
    {
        MG_RadioPanel.SetActive(true);
    }

    void VentPanelOn()
    {
        MG_VentPanel.SetActive(true);
    }

    void CarpetPanelOn()
    {
        MG_CarpetPanel.SetActive(true);
    }
}
