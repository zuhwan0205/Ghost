using UnityEngine;

// ScriptableObject로 깡통 아이템 정의
[CreateAssetMenu(fileName = "New Can Item", menuName = "Inventory/Can Item")]
public class CanItem : Item
{
    public override void Use(Player player)
    {
        Debug.Log($"{itemName}을(를) 사용했습니다! 깡통 투척!");
        player.ThrowCan(); // 플레이어의 깡통 투척 함수 호출
    }
}