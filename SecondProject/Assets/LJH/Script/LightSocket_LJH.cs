using System;
using System.Collections;
using UnityEngine;

public class LightSocket : MonoBehaviour
{
    public static event Action OnLightEnd;
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("충돌 감지됨: " + other.name);
        if (other.CompareTag("Light"))
        {
            // 정확히 위치 맞춰졌을 때 고정
            other.transform.position = transform.position;
            other.GetComponent<DragGlass>().enabled = false;
            Debug.Log("교체완료");
            CompleteChangeLight();
        }
    }

    void CompleteChangeLight()
    {
        OnLightEnd?.Invoke();
    }
}
