using Unity.VisualScripting;
using UnityEngine;
using System;

public class Stain : MonoBehaviour
{
    public GameObject BeforeBG;
    public GameObject AfterBG;
    public GameObject Mannequin;
    private SpriteRenderer stainSR;
    [SerializeField] private float cleanThreshold = 1f;
    [SerializeField] private float cleanStep = 0.2f;
    private float cleanAmount = 0f;
    private float lastCleanTime = 0f;
    private float cleanCooldown = 0.2f;
    private int cleanCount = 0;

    public static event Action OnMannequinEnd;

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
            Debug.Log(cleanCount);
        }

        if (StainWiper.Instance.cleanCount == 3)
        {
            CompleteClean();
            Debug.Log("Clean Complete");
        }
    }

    public void CompleteClean()
    {
        BeforeBG.SetActive(false);
        AfterBG.SetActive(true);
        Destroy(Mannequin);
        OnMannequinEnd?.Invoke();
    }
}
