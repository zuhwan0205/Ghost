using UnityEngine;
using UnityEngine.UI;

public class VendingMachine : MonoBehaviour
{
    [SerializeField] private Item healthItem;
    [SerializeField] private Item staminaItem;
    [SerializeField] private int healthItemPrice = 5;
    [SerializeField] private int staminaItemPrice = 3;
    [SerializeField] private Button healthItemButton;
    [SerializeField] private Button staminaItemButton;
    [SerializeField] private Button closeButton;

    private Player player;

    void Start()
    {
        player = Object.FindFirstObjectByType<Player>();

        if (healthItemButton != null)
            healthItemButton.onClick.AddListener(() => BuyItem(healthItem, healthItemPrice));
        else
            Debug.LogError("HealthItemButton이 할당되지 않았습니다!");

        if (staminaItemButton != null)
            staminaItemButton.onClick.AddListener(() => BuyItem(staminaItem, staminaItemPrice));
        else
            Debug.LogError("StaminaItemButton이 할당되지 않았습니다!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("CloseButton이 할당되지 않았습니다!");
    }

    private void BuyItem(Item item, int price)
    {
        if (player.SpendCoin(price))
        {
            if (player.AddItem(item))
            {
                Debug.Log($"{item.itemName} 구매 성공!");
            }
            else
            {
                player.AddCoin(price);
                Debug.Log("인벤토리가 가득 찼습니다! 동전 환불됨.");
            }
        }
    }

    private void ClosePanel()
    {
        player.HideVendingMachinePanel();
    }
}