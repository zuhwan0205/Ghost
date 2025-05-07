using System;
using UnityEngine;

public class RadioInteract : Interactable
{
    public static event Action OnRadioInteract;
    
    public override void Interact()
    {
        if (MiniGameManagaer_LJH.Instance.isMiniGaming == false)
        {
            Debug.Log("무전기 상호작용 - 이벤트 발행");
            OnRadioInteract?.Invoke();
        }
        
    }
}
