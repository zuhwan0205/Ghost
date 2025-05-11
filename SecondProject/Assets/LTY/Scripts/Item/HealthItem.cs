using UnityEngine;

[CreateAssetMenu(fileName = "HealthItem", menuName = "Inventory/HealthItem")]
public class HealthItem : Item
{
    public float healAmount = 20f;

    // 수정: Use 메서드 시그니처 변경 및 CanItem 교체 로직 추가
    public override void Use(Player player, int slotIndex)
    {
        float newHp = player.Player_Hp + healAmount;
        if (newHp > player.GetMaxHp())
            newHp = player.GetMaxHp();
        player.Player_Hp = newHp;
        Debug.Log($"[HealthItem] {itemName} 사용: 체력 {healAmount} 회복, 현재 HP: {player.Player_Hp}, 슬롯: {slotIndex}");

        Item canItem = Resources.Load<Item>("Can");
        if (canItem != null)
        {
            player.ReplaceItem(slotIndex, canItem);
            Debug.Log($"[HealthItem] {itemName} 사용 후 슬롯 {slotIndex}에 깡통 아이템 추가");
        }
        else
        {
            Debug.LogError($"[HealthItem] 깡통 아이템(Can)을 Resources에서 찾을 수 없음. 경로: Assets/Resources/Can.asset 확인!");
        }
    }
}