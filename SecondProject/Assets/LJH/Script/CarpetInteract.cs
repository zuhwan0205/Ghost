using System;
using UnityEngine;

public class CarpetInteract : Interactable
{
    public static event Action OnCarpetInteract;
    
    public override void Interact()
    {
        if (MiniGameManagaer_LJH.Instance.isMiniGaming == false)
        {
            Debug.Log("카펫 상호작용 - 이벤트 발행");
            OnCarpetInteract?.Invoke();
        }
        
    }
}
