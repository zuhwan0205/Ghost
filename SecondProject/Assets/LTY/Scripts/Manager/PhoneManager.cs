using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance { get; private set; }

    [SerializeField] private Button phoneIconButton;
    [SerializeField] private GameObject phoneScreen;
    [SerializeField] private Button scheduleButton;
    [SerializeField] private Button messengerButton;
    [SerializeField] private Button photoButton;
    [SerializeField] private GameObject schedulePanel;
    [SerializeField] private GameObject messengerPanel;
    [SerializeField] private GameObject photoPanel;
    [SerializeField] private Button backButton;
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Text messengerText;
    [SerializeField] private GameObject photoThumbnailPrefab;
    [SerializeField] private GameObject imageViewPanel;
    [SerializeField] private Image fullImage;
    [SerializeField] private Button closeButton;
    [SerializeField] private List<Sprite> photos = new List<Sprite>();

    private bool isPhoneOpen = false;
    public bool IsPhoneOpen => isPhoneOpen;
    private WiringGameManager wiringGameManager;
    private MirrorCleaningGame mirrorCleaningGame;

    private List<string> messages = new List<string>();
    private int currentPage = 0;
    private const int MAX_MESSAGES = 30;
    private const int MAX_PHOTOS = 15;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (phoneIconButton == null) Debug.LogError("PhoneIconButton�� �Ҵ���� �ʾҽ��ϴ�!");
        if (phoneScreen == null) Debug.LogError("PhoneScreen�� �Ҵ���� �ʾҽ��ϴ�!");
        if (backButton == null) Debug.LogError("BackButton�� �Ҵ���� �ʾҽ��ϴ�!");
        if (leftArrowButton == null) Debug.LogError("LeftArrowButton�� �Ҵ���� �ʾҽ��ϴ�!");
        if (rightArrowButton == null) Debug.LogError("RightArrowButton�� �Ҵ���� �ʾҽ��ϴ�!");
        if (messengerText == null) Debug.LogError("MessengerText�� �Ҵ���� �ʾҽ��ϴ�!");
        if (photoThumbnailPrefab == null) Debug.LogError("PhotoThumbnailPrefab�� �Ҵ���� �ʾҽ��ϴ�!");
        if (imageViewPanel == null) Debug.LogError("ImageViewPanel�� �Ҵ���� �ʾҽ��ϴ�!");
        if (fullImage == null) Debug.LogError("FullImage�� �Ҵ���� �ʾҽ��ϴ�!");
        if (closeButton == null) Debug.LogError("CloseButton�� �Ҵ���� �ʾҽ��ϴ�!");
    }

    void Start()
    {
        Debug.Log("PhoneManager Start: ��ư �̺�Ʈ ���� ����");
        wiringGameManager = FindFirstObjectByType<WiringGameManager>(FindObjectsInactive.Include);
        mirrorCleaningGame = FindFirstObjectByType<MirrorCleaningGame>(FindObjectsInactive.Include);
        if (wiringGameManager == null) Debug.LogWarning("WiringGameManager�� ã�� �� �����ϴ�!");
        if (mirrorCleaningGame == null) Debug.LogWarning("MirrorCleaningGame�� ã�� �� �����ϴ�!");

        if (phoneIconButton != null)
            phoneIconButton.onClick.AddListener(TogglePhoneScreen);
        if (scheduleButton != null)
            scheduleButton.onClick.AddListener(() => ShowAppPanel(schedulePanel));
        if (messengerButton != null)
            messengerButton.onClick.AddListener(() => ShowAppPanel(messengerPanel));
        if (photoButton != null)
            photoButton.onClick.AddListener(() => ShowAppPanel(photoPanel));
        if (backButton != null)
            backButton.onClick.AddListener(GoBackToMain);
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(PreviousPage);
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(NextPage);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseImageView);

        if (phoneScreen != null)
            phoneScreen.SetActive(false);
        if (schedulePanel != null)
            schedulePanel.SetActive(false);
        if (messengerPanel != null)
            messengerPanel.SetActive(false);
        if (photoPanel != null)
            photoPanel.SetActive(false);
        if (backButton != null)
            backButton.gameObject.SetActive(false);
        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(false);
        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(false);
        if (imageViewPanel != null)
            imageViewPanel.SetActive(false);

        AddMessage("핸드폰에 백화점 사진 보내놨으니까 확인해");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePhoneScreen();
        }

        if (Input.GetKeyDown(KeyCode.M))
            AddMessage($"�� �޽��� {messages.Count + 1}");
        if (Input.GetKeyDown(KeyCode.P) && photos.Count > 0)
            AddPhoto(photos[Random.Range(0, photos.Count)]);
    }

    public void TogglePhoneScreen()
    {
        isPhoneOpen = !isPhoneOpen;
        if (phoneScreen != null)
            phoneScreen.SetActive(isPhoneOpen);

        if (isPhoneOpen)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
                player.HideVendingMachinePanel();

            if (wiringGameManager != null && wiringGameManager.IsMiniGameActive)
            {
                wiringGameManager.CancelGame();
                Debug.Log("[PhoneManager] �޴��� UI Ȱ��ȭ: ���� ���� �̴ϰ��� ���");
            }
            if (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf)
            {
                mirrorCleaningGame.CancelGame();
                Debug.Log("[PhoneManager] �޴��� UI Ȱ��ȭ: �ſ� �۱� �̴ϰ��� ���");
            }

            if (schedulePanel != null)
                schedulePanel.SetActive(false);
            if (messengerPanel != null)
                messengerPanel.SetActive(false);
            if (photoPanel != null)
                photoPanel.SetActive(false);
            if (backButton != null)
                backButton.gameObject.SetActive(false);
            if (leftArrowButton != null)
                leftArrowButton.gameObject.SetActive(false);
            if (rightArrowButton != null)
                rightArrowButton.gameObject.SetActive(false);
            if (imageViewPanel != null)
                imageViewPanel.SetActive(false);
        }
        else
        {
            if (backButton != null)
                backButton.gameObject.SetActive(false);
            if (leftArrowButton != null)
                leftArrowButton.gameObject.SetActive(false);
            if (rightArrowButton != null)
                rightArrowButton.gameObject.SetActive(false);
            if (imageViewPanel != null)
                imageViewPanel.SetActive(false);
        }
        Debug.Log($"[PhoneManager] �޴��� UI ����: {(isPhoneOpen ? "Ȱ��ȭ" : "��Ȱ��ȭ")}");
    }

    public void ForceClosePhoneScreen()
    {
        if (phoneScreen != null && phoneScreen.activeSelf)
        {
            phoneScreen.SetActive(false);
            isPhoneOpen = false;
            Debug.Log("[PhoneManager] �޴��� UI ���� ��Ȱ��ȭ �Ϸ�");

            if (schedulePanel != null)
                schedulePanel.SetActive(false);
            if (messengerPanel != null)
                messengerPanel.SetActive(false);
            if (photoPanel != null)
                photoPanel.SetActive(false);
            if (backButton != null)
                backButton.gameObject.SetActive(false);
            if (leftArrowButton != null)
                leftArrowButton.gameObject.SetActive(false);
            if (rightArrowButton != null)
                rightArrowButton.gameObject.SetActive(false);
            if (imageViewPanel != null)
                imageViewPanel.SetActive(false);
        }
    }

    private void ShowAppPanel(GameObject panel)
    {
        Debug.Log($"ShowAppPanel ȣ���: {panel?.name}");
        if (schedulePanel != null)
            schedulePanel.SetActive(false);
        if (messengerPanel != null)
            messengerPanel.SetActive(false);
        if (photoPanel != null)
            photoPanel.SetActive(false);
        if (imageViewPanel != null)
            imageViewPanel.SetActive(false);
        if (panel != null)
            panel.SetActive(true);

        if (panel == messengerPanel)
        {
            currentPage = messages.Count > 0 ? messages.Count - 1 : 0;
            if (backButton != null)
                backButton.gameObject.SetActive(true);
            if (leftArrowButton != null)
                leftArrowButton.gameObject.SetActive(true);
            if (rightArrowButton != null)
                rightArrowButton.gameObject.SetActive(true);
            UpdateMessengerPage();
        }
        else if (panel == photoPanel)
        {
            if (backButton != null)
                backButton.gameObject.SetActive(true);
            UpdatePhotoGallery();
        }
        else
        {
            if (backButton != null)
                backButton.gameObject.SetActive(true);
            if (leftArrowButton != null)
                leftArrowButton.gameObject.SetActive(false);
            if (rightArrowButton != null)
                rightArrowButton.gameObject.SetActive(false);
        }
    }

    private void GoBackToMain()
    {
        Debug.Log("GoBackToMain ȣ���");
        if (schedulePanel != null)
            schedulePanel.SetActive(false);
        if (messengerPanel != null)
            messengerPanel.SetActive(false);
        if (photoPanel != null)
            photoPanel.SetActive(false);
        if (backButton != null)
            backButton.gameObject.SetActive(false);
        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(false);
        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(false);
        if (imageViewPanel != null)
            imageViewPanel.SetActive(false);
    }

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateMessengerPage();
            Debug.Log($"���� ������: {currentPage + 1}");
        }
    }

    private void NextPage()
    {
        if (currentPage < messages.Count - 1)
        {
            currentPage++;
            UpdateMessengerPage();
            Debug.Log($"���� ������: {currentPage + 1}");
        }
    }

    private void UpdateMessengerPage()
    {
        if (messengerText != null)
        {
            if (messages.Count > 0 && currentPage >= 0 && currentPage < messages.Count)
                messengerText.text = messages[currentPage];
            else
                messengerText.text = "나 보다 전에 일했던 사람이 알려줬는데 밤에 뭔가 기어다니는 소리가 자주 들렸다고 하더라고....";
        }

        if (leftArrowButton != null)
            leftArrowButton.interactable = currentPage > 0;
        if (rightArrowButton != null)
            rightArrowButton.interactable = currentPage < messages.Count - 1;
    }

    public void AddMessage(string message)
    {
        messages.Add(message);
        if (messages.Count > MAX_MESSAGES)
        {
            messages.RemoveAt(0);
            if (currentPage > 0) currentPage--;
        }
        if (messengerPanel != null && messengerPanel.activeSelf)
        {
            currentPage = messages.Count - 1;
            UpdateMessengerPage();
        }
        Debug.Log($"�޽��� �߰�: {message}, �� �޽��� ��: {messages.Count}");
    }

    private void UpdatePhotoGallery()
    {
        foreach (Transform child in photoPanel.transform)
        {
            if (child.name.StartsWith("PhotoThumbnail"))
                Destroy(child.gameObject);
        }

        for (int i = 0; i < Mathf.Min(photos.Count, MAX_PHOTOS); i++)
        {
            GameObject thumbnail = Instantiate(photoThumbnailPrefab, photoPanel.transform);
            thumbnail.name = $"PhotoThumbnail_{i}";
            Image thumbnailImage = thumbnail.transform.Find("ThumbnailImage").GetComponent<Image>();
            if (thumbnailImage != null)
                thumbnailImage.sprite = photos[i];
            Button button = thumbnail.GetComponent<Button>();
            int index = i;
            button.onClick.AddListener(() => ShowFullImage(index));
        }
    }

    private void ShowFullImage(int index)
    {
        if (imageViewPanel != null && fullImage != null && index >= 0 && index < photos.Count)
        {
            fullImage.sprite = photos[index];
            imageViewPanel.SetActive(true);
            photoPanel.SetActive(false);
            if (backButton != null)
                backButton.gameObject.SetActive(true);
        }
    }

    private void CloseImageView()
    {
        if (imageViewPanel != null)
            imageViewPanel.SetActive(false);
        if (photoPanel != null)
            photoPanel.SetActive(true);
        if (backButton != null)
            backButton.gameObject.SetActive(true);
    }

    public void AddPhoto(Sprite photo)
    {
        if (photo != null)
        {
            photos.Add(photo);
            if (photos.Count > MAX_PHOTOS)
            {
                photos.RemoveAt(0);
            }
            if (photoPanel != null && photoPanel.activeSelf)
                UpdatePhotoGallery();
            Debug.Log($"���� �߰���: {photo.name}, �� ���� ��: {photos.Count}");
        }
    }
}