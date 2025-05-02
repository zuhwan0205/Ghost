using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerPhone : MonoBehaviour
{
    public GameObject phoneUI; // PhoneScreen
    public TextMeshProUGUI missionText; // MissionText
    public GameObject appPanel; // SchedulePanel
    public TextMeshProUGUI appContentText; // AppContentText
    public Button missionAppButton; // MessengerButton
    public Button backButton; // BackButton
    public Button phoneIconButton; // PhoneIconButton (UICanvas에 있음)

    void Start()
    {
        // 참조 체크
        if (phoneUI == null) Debug.LogError("PhoneUI not assigned! Assign PhoneScreen.");
        if (missionText == null) Debug.LogError("MissionText not assigned! Add MissionText to PhoneScreen.");
        if (appPanel == null) Debug.LogError("AppPanel not assigned! Assign SchedulePanel.");
        if (appContentText == null) Debug.LogError("AppContentText not assigned! Add AppContentText to SchedulePanel.");
        if (missionAppButton == null) Debug.LogError("MissionAppButton not assigned! Assign MessengerButton.");
        if (backButton == null) Debug.LogError("BackButton not assigned! Assign BackButton.");
        if (phoneIconButton == null) Debug.LogError("PhoneIconButton not assigned! Assign PhoneIconButton from UICanvas.");

        // 초기 상태
        phoneUI.SetActive(false);
        appPanel.SetActive(false);

        // 버튼 이벤트 설정
        missionAppButton.onClick.AddListener(OpenAppPanel);
        backButton.onClick.AddListener(CloseAppPanel);
        phoneIconButton.onClick.AddListener(TogglePhoneUI);
    }

    void TogglePhoneUI()
    {
        if (phoneUI != null)
        {
            phoneUI.SetActive(!phoneUI.activeSelf);
            if (phoneUI.activeSelf)
            {
                UpdatePhoneUI();
                appPanel.SetActive(false);
                Debug.Log("Phone UI opened via PhoneIconButton");
            }
            else
            {
                Debug.Log("Phone UI closed via PhoneIconButton");
            }
        }
    }

    void UpdatePhoneUI()
    {
        if (GameManager.Instance != null && missionText != null)
        {
            int current = GameManager.Instance.GetCurrentMissions();
            int quota = GameManager.Instance.GetMissionQuota();
            missionText.text = $"Mission: {current}/{quota}";
            Debug.Log($"Phone UI updated: Mission: {current}/{quota}");
        }
    }

    void OpenAppPanel()
    {
        if (appPanel != null && appContentText != null && GameManager.Instance != null)
        {
            appPanel.SetActive(true);
            int current = GameManager.Instance.GetCurrentMissions();
            int quota = GameManager.Instance.GetMissionQuota();
            appContentText.text = $"Mission Details: {current}/{quota}";
            Debug.Log("SchedulePanel opened");
        }
    }

    void CloseAppPanel()
    {
        if (appPanel != null)
        {
            appPanel.SetActive(false);
            Debug.Log("SchedulePanel closed via BackButton");
        }
        else
        {
            Debug.LogError("Cannot close SchedulePanel: appPanel is null!");
        }
    }
}