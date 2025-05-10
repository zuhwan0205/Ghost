using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 싱글톤
    public TextMeshProUGUI healthText; // 체력 텍스트
    public UnityEngine.UI.Image staminaBarFill; // 스태미나 바
    public TextMeshProUGUI staminaText; // 스태미나 텍스트
    public UnityEngine.UI.Image[] itemSlots; // 아이템 슬롯 3개

    private PlayerHealth playerHealth;
    private PlayerController playerController;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 플레이어 컴포넌트 참조
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (playerHealth == null) Debug.LogError("PlayerHealth not found!");
        if (playerController == null) Debug.LogError("PlayerController not found!");
        if (healthText == null) Debug.LogError("HealthText not assigned!");
        if (staminaBarFill == null) Debug.LogError("StaminaBarFill not assigned!");
        if (staminaText == null) Debug.LogError("StaminaText not assigned!");
        if (itemSlots == null || itemSlots.Length != 3) Debug.LogError("ItemSlots not assigned or incorrect count!");

        // 초기 UI 설정
        UpdateHealthUI();
        UpdateStaminaUI();
        ClearItemSlots();
    }

    void Update()
    {
        // 실시간 업데이트
        UpdateHealthUI();
        UpdateStaminaUI();
    }

    public void UpdateHealthUI()
    {
        if (playerHealth != null && healthText != null)
        {
            int currentHealth = playerHealth.GetCurrentHealth();
            int maxHealth = playerHealth.GetMaxHealth();
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void UpdateStaminaUI()
    {
        if (playerController != null && staminaBarFill != null && staminaText != null)
        {
            float staminaPercent = playerController.GetStaminaPercentage();
            staminaBarFill.fillAmount = staminaPercent;
            float currentStamina = playerController.GetCurrentStamina();
            float maxStamina = playerController.GetStaminaMax();
            staminaText.text = $"{Mathf.RoundToInt(currentStamina)}/{Mathf.RoundToInt(maxStamina)}";
        }
    }

    public void AddItemToSlot(int slotIndex, Sprite itemSprite)
    {
        if (slotIndex >= 0 && slotIndex < itemSlots.Length && itemSprite != null)
        {
            itemSlots[slotIndex].sprite = itemSprite;
            itemSlots[slotIndex].color = Color.white; // 아이템 표시
            Debug.Log($"Added item to slot {slotIndex}");
        }
    }

    public void ClearItemSlots()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            itemSlots[i].sprite = null;
            itemSlots[i].color = new Color(0.5f, 0.5f, 0.5f, 1); // 회색 빈칸
        }
    }
}