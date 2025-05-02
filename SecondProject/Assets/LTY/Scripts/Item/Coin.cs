using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int coinValue = 1;

    public void Collect(Player player)
    {
        player.AddCoin(coinValue);
        Debug.Log($"���� {coinValue}�� ȹ��! ���� ����: {player.GetCoinAmount()}");
        Destroy(gameObject);
    }
}