using UnityEngine;

[CreateAssetMenu(fileName = "HealthItem", menuName = "Inventory/HealthItem")]
public class HealthItem : Item
{
    public float healAmount = 20f;

    public override void Use(Player player)
    {
        float newHp = player.Player_Hp + healAmount;
        if (newHp > player.GetMaxHp())
            newHp = player.GetMaxHp();
        player.Player_Hp = newHp;
        Debug.Log($"{itemName} 사용: 체력 {healAmount} 회복, 현재 HP: {player.Player_Hp}");
    }
}