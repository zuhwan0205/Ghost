using System;
using UnityEngine;

public class VentInteract : Interactable
{
    public static event Action OnVentInteract;

    public override void Interact()
    {
        if (MiniGameManagaer_LJH.Instance.isMiniGaming == false)
        {
            Debug.Log("벤트 상호작용 - 이벤트 발행");
            OnVentInteract?.Invoke();
        }
        
    }
}
