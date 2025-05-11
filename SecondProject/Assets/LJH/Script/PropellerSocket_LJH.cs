using System;
using UnityEngine;

public class PropellerSocket_LJH : MonoBehaviour
{
    public static event Action OnVentEnd;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌 감지됨: " + other.name);
        if (other.CompareTag("ProPeller") && TrashCan.Instance.VentTrashCount == 4)
        {
            other.transform.position = Vector2.zero;
            other.GetComponent<DragGlass>().enabled = false;
            Debug.Log("교체완료");
            AudioManager.Instance.PlaySFX("Vendingmachine", transform.position);
            CompleteVentMission();
        }
    }
    
    void CompleteVentMission()
    {
        OnVentEnd?.Invoke();
    }
}
