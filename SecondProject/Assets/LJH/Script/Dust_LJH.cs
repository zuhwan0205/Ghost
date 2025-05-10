using System;
using UnityEngine;

public class Dust_LJH : MonoBehaviour
{
    private SpriteRenderer stainSR;
    [SerializeField] private float cleanThreshold = 1f;
    [SerializeField] private float cleanStep = 0.2f;
    private float cleanAmount = 0f;
    private float lastCleanTime = 0f;
    private float cleanCooldown = 0.2f;
    private int cleanCount = 0;
    public GameObject Carpet;
    [SerializeField] private MonoBehaviour CarpetInteract;

    public static event Action OnCarpetEnd;

    private void Awake()
    {
        stainSR = GetComponent<SpriteRenderer>();
    }
    
    public void Clean(Vector2 hitPos)
    {
        // 일정 시간 간격보다 빨리 들어오면 무시
        if (Time.time - lastCleanTime < cleanCooldown) return;

        lastCleanTime = Time.time; // 마지막 닦은 시간 기록
        cleanAmount += cleanStep;
        Debug.Log("CleanAmount: " + cleanAmount);
        stainSR.color = new Color(0.545f, 0.27f, 0.075f, 1f - cleanAmount);

        if (cleanAmount >= cleanThreshold)
        {
            Destroy(gameObject);
            StainWiper.Instance.cleanCount += 1;
            Debug.Log(StainWiper.Instance.cleanCount);
        }

        if (StainWiper.Instance.cleanCount == 4)
        {
            CompleteCleanDust();
            Debug.Log("Clean Complete");
        }
    }

    public void CompleteCleanDust()
    {
        //Destroy(Carpet);
        CarpetInteract.enabled = false;
        OnCarpetEnd?.Invoke();
    }
}
