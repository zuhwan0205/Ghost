using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName; // 아이템 이름 (예: "Health Potion")
    public Sprite icon;     // 아이템 이미지
    public string description; // 아이템 설명

    // 아이템 사용 시 호출되는 함수
    public virtual void Use(Player player)
    {
        Debug.Log($"{itemName}을(를) 사용했습니다!");
        // 예: 체력 회복 아이템이라면
        // player.Player_Hp += 5f;
    }
}