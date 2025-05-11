using UnityEngine;

[CreateAssetMenu(fileName = "StaminaItem", menuName = "Inventory/StaminaItem")]
public class StaminaItem : Item
{
    public float staminaAmount = 30f;

    // ����: Use �޼��� �ñ״�ó ���� �� CanItem ��ü ���� �߰�
    public override void Use(Player player, int slotIndex)
    {
        float newStamina = player.GetStemina() + staminaAmount;
        if (newStamina > player.GetMaxStemina())
            newStamina = player.GetMaxStemina();
        player.SetStemina(newStamina);
        Debug.Log($"[StaminaItem] {itemName} ���: ���¹̳� {staminaAmount} ȸ��, ���� ���¹̳�: {player.GetStemina()}, ����: {slotIndex}");

        Item canItem = Resources.Load<Item>("Can");
        if (canItem != null)
        {
            player.ReplaceItem(slotIndex, canItem);
            Debug.Log($"[StaminaItem] {itemName} ��� �� ���� {slotIndex}�� ���� ������ �߰�");
        }
        else
        {
            Debug.LogError($"[StaminaItem] ���� ������(Can)�� Resources���� ã�� �� ����. ���: Assets/Resources/Can.asset Ȯ��!");
        }
    }
}