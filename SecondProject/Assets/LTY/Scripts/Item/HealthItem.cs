using UnityEngine;

[CreateAssetMenu(fileName = "HealthItem", menuName = "Inventory/HealthItem")]
public class HealthItem : Item
{
    public float healAmount = 20f;

    // ����: Use �޼��� �ñ״�ó ���� �� CanItem ��ü ���� �߰�
    public override void Use(Player player, int slotIndex)
    {
        float newHp = player.Player_Hp + healAmount;
        if (newHp > player.GetMaxHp())
            newHp = player.GetMaxHp();
        player.Player_Hp = newHp;
        Debug.Log($"[HealthItem] {itemName} ���: ü�� {healAmount} ȸ��, ���� HP: {player.Player_Hp}, ����: {slotIndex}");

        Item canItem = Resources.Load<Item>("Can");
        if (canItem != null)
        {
            player.ReplaceItem(slotIndex, canItem);
            Debug.Log($"[HealthItem] {itemName} ��� �� ���� {slotIndex}�� ���� ������ �߰�");
        }
        else
        {
            Debug.LogError($"[HealthItem] ���� ������(Can)�� Resources���� ã�� �� ����. ���: Assets/Resources/Can.asset Ȯ��!");
        }
    }
}