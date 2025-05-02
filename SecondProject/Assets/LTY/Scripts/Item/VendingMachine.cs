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
            Debug.LogError("HealthItemButton�� �Ҵ���� �ʾҽ��ϴ�!");

        if (staminaItemButton != null)
            staminaItemButton.onClick.AddListener(() => BuyItem(staminaItem, staminaItemPrice));
        else
            Debug.LogError("StaminaItemButton�� �Ҵ���� �ʾҽ��ϴ�!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("CloseButton�� �Ҵ���� �ʾҽ��ϴ�!");
    }

    private void BuyItem(Item item, int price)
    {
        if (player.SpendCoin(price))
        {
            if (player.AddItem(item))
            {
                Debug.Log($"{item.itemName} ���� ����!");
            }
            else
            {
                player.AddCoin(price);
                Debug.Log("�κ��丮�� ���� á���ϴ�! ���� ȯ�ҵ�.");
            }
        }
    }

    private void ClosePanel()
    {
        player.HideVendingMachinePanel();
    }
}