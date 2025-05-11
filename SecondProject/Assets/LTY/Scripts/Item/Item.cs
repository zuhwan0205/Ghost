using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    // ����: Use �޼��� �ñ״�ó ���� (���� �ε��� �߰�)
    public virtual void Use(Player player, int slotIndex)
    {
        Debug.Log($"[Item] {itemName}��(��) ����߽��ϴ�! ����: {slotIndex}");
    }
}