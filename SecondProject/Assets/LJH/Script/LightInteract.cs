using System;
using UnityEngine;

public class LightInteract : Interactable
{
    public static event Action OnLightInteract;
    
    public override void Interact()
    {
        if (MiniGameManagaer_LJH.Instance.isMiniGaming == false)
        {
            Debug.Log("전등 상호작용 - 이벤트 발행");
            OnLightInteract?.Invoke();
        }
        
    }
}
