using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    public void Collect(Player player)
    {
        player.AddCoin(coinValue);
        Debug.Log($"동전 {coinValue}개 획득! 현재 동전: {player.GetCoinAmount()}");
        Destroy(gameObject);
    }
}