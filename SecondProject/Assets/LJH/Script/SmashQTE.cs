using UnityEngine;
using UnityEngine.UI;

public class SmashQTE : MonoBehaviour
{
    public Slider qteSlider;
    
    public float increasePerPress = 8f;
    public float decreasePerSecond = 10f;
    public float maxGauge = 100f;

    private float currentGauge = 0f;
    private bool isQTEActive = false;

    void Start()
    {
        StartQTE();
    }

    public void StartQTE()
    {
        currentGauge = 0f;
        isQTEActive = true;
        qteSlider.gameObject.SetActive(true);
        qteSlider.maxValue = maxGauge;
        qteSlider.value = currentGauge;
    }

    void Update()
    {
        if (!isQTEActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentGauge += increasePerPress;
            currentGauge = Mathf.Min(currentGauge, maxGauge);
            qteSlider.value = currentGauge;
        }

        currentGauge -= decreasePerSecond * Time.deltaTime;
        currentGauge = Mathf.Max(currentGauge, 0f);
        qteSlider.value = currentGauge;

        if (currentGauge >= maxGauge - 0.2f)
        {
            CompleteQTE();
        }
    }

    void CompleteQTE()
    {
        isQTEActive = false;
        qteSlider.gameObject.SetActive(false);
        Debug.Log("QTE 성공!");
        // TODO: 성공 시 처리할 이벤트 호출
    }
}