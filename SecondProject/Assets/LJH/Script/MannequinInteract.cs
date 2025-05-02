using System;
using UnityEngine;

public class MannequinInteract : Interactable
{
    public static event Action OnMannequinInteracted;

    public override void Interact()
    {
        Debug.Log("마네킹 상호작용 - 이벤트 발행");
        OnMannequinInteracted?.Invoke();
    }
}
