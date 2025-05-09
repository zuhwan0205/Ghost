using System;
using UnityEngine;

public class GlassInteract : Interactable
{
    public static event Action OnGlassInteract;
    
    public override void Interact()
    {
        if (MiniGameManagaer_LJH.Instance.isMiniGaming == false)
        {
            Debug.Log("유리 상호작용 - 이벤트 발행");
            OnGlassInteract?.Invoke();
        }
        
    }
}
