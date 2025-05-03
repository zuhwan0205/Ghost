using System;
using UnityEngine;

public class Stain : MonoBehaviour
{
    [SerializeField] private GameObject beforeBG;
    [SerializeField] private GameObject afterBG;
    [SerializeField] private GameObject mannequin;
    private SpriteRenderer stainSR;
    [SerializeField] private float cleanThreshold = 1f;
    [SerializeField] private float cleanStep = 0.2f;
    private float cleanAmount = 0f;
    private float lastCleanTime = 0f;
    private float cleanCooldown = 0.2f;

    private void Awake()
    {
        stainSR = GetComponent<SpriteRenderer>();
    }

    public void Clean(Vector2 hitPos)
    {
        if (Time.time - lastCleanTime < cleanCooldown)
            return;

        lastCleanTime = Time.time;
        cleanAmount += cleanStep;
        stainSR.color = new Color(0.545f, 0.27f, 0.075f, 1f - cleanAmount);

        if (cleanAmount >= cleanThreshold)
        {
            Destroy(gameObject);
            FindObjectOfType<StainWiper>().OnCleanCompleted();
        }
    }
}