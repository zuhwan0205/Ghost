using UnityEngine;

[CreateAssetMenu(fileName = "StaminaItem", menuName = "Inventory/StaminaItem")]
public class StaminaItem : Item
{
    public float staminaAmount = 30f;

    // 수정: Use 메서드 시그니처 변경 및 CanItem 교체 로직 추가
    public override void Use(Player player, int slotIndex)
    {
        float newStamina = player.GetStemina() + staminaAmount;
        if (newStamina > player.GetMaxStemina())
            newStamina = player.GetMaxStemina();
        player.SetStemina(newStamina);
        Debug.Log($"[StaminaItem] {itemName} 사용: 스태미나 {staminaAmount} 회복, 현재 스태미나: {player.GetStemina()}, 슬롯: {slotIndex}");

        Item canItem = Resources.Load<Item>("Can");
        if (canItem != null)
        {
            player.ReplaceItem(slotIndex, canItem);
            Debug.Log($"[StaminaItem] {itemName} 사용 후 슬롯 {slotIndex}에 깡통 아이템 추가");
        }
        else
        {
            Debug.LogError($"[StaminaItem] 깡통 아이템(Can)을 Resources에서 찾을 수 없음. 경로: Assets/Resources/Can.asset 확인!");
        }
    }
}