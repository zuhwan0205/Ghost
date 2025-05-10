using System;
using UnityEngine;

public class QTEManager_LJH : MonoBehaviour
{
    //QTE 패널
    public GameObject SmashQTEPanel;
    public GameObject StarForceQTEPanel;
    
    //플레이어 참조
    //public GameObject PlayerController;
    
    public static QTEManager_LJH Instance;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        StarforceQTE.OnEndStarForceQTE += EndQte;
        SmashQTE.OnEndSmashQTE += EndSmashQte;
    }

    private void OnDisable()
    {
        StarforceQTE.OnEndStarForceQTE -= EndQte;
        SmashQTE.OnEndSmashQTE -= EndSmashQte;
    }

    public void StartSmashQte()
    {
        SmashQTEPanel.SetActive(true);
        SmashQTE.Instance.StartSmashQTE();
        //PlayerController.SetActive(false);
    }

    public void StartStarForceQte()
    {
        StarForceQTEPanel.SetActive(true);
        StarforceQTE.Instance.StartstarforceQTE();
        //PlayerController.SetActive(false);
    }

    void EndQte()
    {
        StarForceQTEPanel.SetActive(false);
        Mannequin.Instance.EscapeFromMannequin();
    }

    void EndSmashQte()
    {
        SmashQTEPanel.SetActive(false);
        Mannequin.Instance.EscapeFromMannequin();
    }

}
