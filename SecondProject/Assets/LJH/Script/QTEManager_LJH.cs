using System;
using UnityEngine;

public class QTEManager_LJH : MonoBehaviour
{
    //QTE 패널
    public GameObject SmashQTEPanel;
    public GameObject StarForceQTEPanel;
    
    //플레이어 참조
    public GameObject PlayerController;

    private void OnEnable()
    {
        StarforceQTE.OnEndStarForceQTE += EndQte;
        SmashQTE.OnEndSmashQTE += EndQte;
    }

    private void OnDisable()
    {
        StarforceQTE.OnEndStarForceQTE -= EndQte;
        SmashQTE.OnEndSmashQTE -= EndQte;
    }

    void StartSmashQte()
    {
        SmashQTEPanel.SetActive(true);
        PlayerController.SetActive(false);
    }

    void StartStarForceQte()
    {
        StarForceQTEPanel.SetActive(true);
        PlayerController.SetActive(false);
    }

    void EndQte()
    {
        SmashQTEPanel.SetActive(false);
        StarForceQTEPanel.SetActive(false);
        PlayerController.SetActive(true);
    }

}
