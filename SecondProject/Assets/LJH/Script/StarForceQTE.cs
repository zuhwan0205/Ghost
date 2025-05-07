using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class StarforceQTE : MonoBehaviour
{
    public RectTransform star;           
    public RectTransform successZone;    
    public float speed = 1000f;           
    
    private bool goingRight = true;
    private bool isQTEActive = false;
    private float minX, maxX;
    
    private int successCount = 0;
    
    public static event Action OnEndStarForceQTE;

    void Start()
    {
        StartQTE();
    }

    public void StartQTE()
    {
        isQTEActive = true;
        minX = -450f;
        maxX = 450f;
        star.anchoredPosition = new Vector2(minX, 0);
        goingRight = true;
        
        float zoneRangeMin = minX + 50f;
        float zoneRangeMax = maxX - 50f;

        float randomX = Random.Range(zoneRangeMin, zoneRangeMax);
        successZone.anchoredPosition = new Vector2(randomX, successZone.anchoredPosition.y);
    }

    void Update()
    {
        if (!isQTEActive) return;
        
        Vector2 pos = star.anchoredPosition;
        pos.x += (goingRight ? 1 : -1) * speed * Time.deltaTime;

        if (pos.x >= maxX)
        {
            pos.x = maxX;
            goingRight = false;
        }
        else if (pos.x <= minX)
        {
            pos.x = minX;
            goingRight = true;
        }

        star.anchoredPosition = pos;
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isQTEActive = false; // 별 멈춤

            float starX = star.position.x;
            float zoneMin = successZone.position.x - successZone.rect.width / 2;
            float zoneMax = successZone.position.x + successZone.rect.width / 2;

            if (starX >= zoneMin && starX <= zoneMax)
            {
                successCount++;
                Debug.Log("성공! 총 성공 횟수: " + successCount);

                if (successCount >= 3)
                {
                    Debug.Log("QTE 완료!");
                    OnEndStarForceQTE?.Invoke();
                }
                else
                {
                    StartCoroutine(RestartQTEAfterDelay(0.5f));
                }
            }
            else
            {
                Debug.Log("실패. 다시 시작");
                StartCoroutine(RestartQTEAfterDelay(0.5f));
            }
        }
    }
    
    private IEnumerator RestartQTEAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartQTE();
    }
}