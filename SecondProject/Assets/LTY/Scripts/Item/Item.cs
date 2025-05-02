using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName; // ������ �̸� (��: "Health Potion")
    public Sprite icon;     // ������ �̹���
    public string description; // ������ ����

    // ������ ��� �� ȣ��Ǵ� �Լ�
    public virtual void Use(Player player)
    {
        Debug.Log($"{itemName}��(��) ����߽��ϴ�!");
        // ��: ü�� ȸ�� �������̶��
        // player.Player_Hp += 5f;
    }
}