using UnityEngine;

[CreateAssetMenu(fileName = "New Can Item", menuName = "Inventory/Can Item")]
public class CanItem : Item
{
    // 수정: Use 메서드 시그니처 변경 및 RemoveItem 호출
    public override void Use(Player player, int slotIndex)
    {
        Debug.Log($"[CanItem] {itemName} 사용: 깡통 투척, 슬롯: {slotIndex}");
        player.ThrowCan();
        player.RemoveItem(slotIndex);
    }
}