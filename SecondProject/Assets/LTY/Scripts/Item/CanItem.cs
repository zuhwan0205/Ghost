using UnityEngine;

[CreateAssetMenu(fileName = "New Can Item", menuName = "Inventory/Can Item")]
public class CanItem : Item
{
    // ����: Use �޼��� �ñ״�ó ���� �� RemoveItem ȣ��
    public override void Use(Player player, int slotIndex)
    {
        Debug.Log($"[CanItem] {itemName} ���: ���� ��ô, ����: {slotIndex}");
        player.ThrowCan();
        player.RemoveItem(slotIndex);
    }
}