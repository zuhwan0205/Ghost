using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    // 수정: Use 메서드 시그니처 변경 (슬롯 인덱스 추가)
    public virtual void Use(Player player, int slotIndex)
    {
        Debug.Log($"[Item] {itemName}을(를) 사용했습니다! 슬롯: {slotIndex}");
    }
}