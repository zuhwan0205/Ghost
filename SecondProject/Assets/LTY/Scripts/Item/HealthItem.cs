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
        Debug.Log($"{itemName} ���: ü�� {healAmount} ȸ��, ���� HP: {player.Player_Hp}");
    }
}