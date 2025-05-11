using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MG_Radio_LJH : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public float sensitivity = 0.01f; // 회전 민감도 조절
    public float targetFrequency = 0f;
    public float frequencyTolerance = 0.1f;

    public TMP_Text frequencyText;
    public AudioSource staticNoise;

    private RectTransform dialRect;
    private float totalRotation = 0f;
    private float currentFreq = 0f;
    private float displayedFreq = 0f;

    private Vector2 prevMousePos;
    
    private bool isMatched = false;

    public static event Action OnRadioEnd;

    void Start()
    {
        dialRect = GetComponent<RectTransform>();
        currentFreq = 130f;
        displayedFreq = 130f;
        frequencyText.text = "130.0";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMatched) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialRect, eventData.position, eventData.pressEventCamera, out prevMousePos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isMatched) return;
        
        Vector2 currentMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dialRect, eventData.position, eventData.pressEventCamera, out currentMousePos);

        // 현재 위치와 이전 위치 사이의 각도 차이 계산
        float angleDelta = Vector2.SignedAngle(prevMousePos, currentMousePos);
        totalRotation += angleDelta * sensitivity;

        // 회전 값을 적용
        dialRect.localEulerAngles = new Vector3(0, 0, -totalRotation);

        // 주파수 계산: 오른쪽으로 돌리면 증가하고, 왼쪽으로 돌리면 감소
        currentFreq = Mathf.Max(0f, 130f - totalRotation);

        // 주파수 차이 계산
        float diff = Mathf.Abs(currentFreq - targetFrequency);
        staticNoise.volume = Mathf.Clamp01(diff / 10f);

        if (diff < frequencyTolerance)
        {
            CompleteFrequencyMatch();
            AudioManager.Instance.PlaySFX("WalkieTalkie", transform.position);
        }

        prevMousePos = currentMousePos;
    }
    
    void Update()
    {
        if (isMatched) return;
        displayedFreq = Mathf.Lerp(displayedFreq, currentFreq, Time.deltaTime * 10f);
        frequencyText.text = $"{displayedFreq:F1}";
    }

    void CompleteFrequencyMatch()
    {
        isMatched = true;
        staticNoise.Stop();
        frequencyText.text = "0";
        Debug.Log("정상 주파수 연결됨");
        OnRadioEnd?.Invoke();
    }
}
