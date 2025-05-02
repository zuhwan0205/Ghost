using UnityEngine;

public class StarforceQTE : MonoBehaviour
{
    public RectTransform star;           
    public RectTransform successZone;    
    public float speed = 1000f;           
    
    private bool goingRight = true;
    private bool isQTEActive = false;
    private float minX, maxX;

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
            float starX = star.position.x;
            float zoneMin = successZone.position.x - successZone.rect.width / 2;
            float zoneMax = successZone.position.x + successZone.rect.width / 2;

            if (starX >= zoneMin && starX <= zoneMax)
            {
                Debug.Log("성공");
                isQTEActive = false;
            }
            else
            {
                Debug.Log("실패");
                isQTEActive = false;
            }
        }
    }
}