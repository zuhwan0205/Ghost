using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MG_Radio_LJH : MonoBehaviour, IMiniGame, IBeginDragHandler, IDragHandler
{
    [SerializeField] private GameObject miniGamePanel;
    [SerializeField] private float sensitivity = 0.01f;
    [SerializeField] private float targetFrequency = 0f;
    [SerializeField] private float frequencyTolerance = 0.1f;
    [SerializeField] private TMP_Text frequencyText;
    [SerializeField] private AudioSource staticNoise;

    private RectTransform dialRect;
    private float totalRotation = 0f;
    private float currentFreq = 0f;
    private float displayedFreq = 0f;
    private Vector2 prevMousePos;
    private bool isGameActive = false;
    private Interactable currentInteractable;

    public bool IsActive => isGameActive;

    private void Awake()
    {
        dialRect = GetComponent<RectTransform>();
        miniGamePanel.SetActive(false);
    }

    private void Start()
    {
        currentFreq = 130f;
        displayedFreq = 130f;
        frequencyText.text = "130.0";
    }

    public void StartMiniGame(Interactable interactable)
    {
        if (isGameActive)
            return;

        currentInteractable = interactable;
        miniGamePanel.SetActive(true);
        isGameActive = true;
        totalRotation = 0f;
        currentFreq = 130f;
        displayedFreq = 130f;
        frequencyText.text = "130.0";
        staticNoise.Play();
    }

    public void CancelGame()
    {
        if (!isGameActive)
            return;

        isGameActive = false;
        miniGamePanel.SetActive(false);
        staticNoise.Stop();
        currentInteractable = null;
    }

    public void CompleteGame()
    {
        isGameActive = false;
        miniGamePanel.SetActive(false);
        staticNoise.Stop();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isGameActive)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(dialRect, eventData.position, eventData.pressEventCamera, out prevMousePos);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isGameActive)
            return;

        Vector2 currentMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dialRect, eventData.position, eventData.pressEventCamera, out currentMousePos);

        float angleDelta = Vector2.SignedAngle(prevMousePos, currentMousePos);
        totalRotation += angleDelta * sensitivity;

        dialRect.localEulerAngles = new Vector3(0, 0, -totalRotation);
        currentFreq = Mathf.Max(0f, 130f - totalRotation);

        float diff = Mathf.Abs(currentFreq - targetFrequency);
        staticNoise.volume = Mathf.Clamp01(diff / 10f);

        if (diff < frequencyTolerance)
        {
            FindObjectOfType<MiniGameManager>().CompleteMiniGame("Radio", currentInteractable);
        }

        prevMousePos = currentMousePos;
    }

    private void Update()
    {
        if (!isGameActive)
            return;

        displayedFreq = Mathf.Lerp(displayedFreq, currentFreq, Time.deltaTime * 10f);
        frequencyText.text = $"{displayedFreq:F1}";
    }
}