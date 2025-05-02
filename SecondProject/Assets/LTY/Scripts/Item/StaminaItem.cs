using UnityEngine;

[CreateAssetMenu(fileName = "StaminaItem", menuName = "Inventory/StaminaItem")]
public class StaminaItem : Item
{
    public float staminaAmount = 30f;

    public override void Use(Player player)
    {
        float newStamina = player.GetStemina() + staminaAmount;
        if (newStamina > player.GetMaxStemina())
            newStamina = player.GetMaxStemina();
        player.SetStemina(newStamina);
        Debug.Log($"{itemName} 사용: 스태미나 {staminaAmount} 회복, 현재 스태미나: {player.GetStemina()}");
    }
}