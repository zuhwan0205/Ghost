using UnityEngine;

// ScriptableObject�� ���� ������ ����
[CreateAssetMenu(fileName = "New Can Item", menuName = "Inventory/Can Item")]
public class CanItem : Item
{
    public override void Use(Player player)
    {
        Debug.Log($"{itemName}��(��) ����߽��ϴ�! ���� ��ô!");
        player.ThrowCan(); // �÷��̾��� ���� ��ô �Լ� ȣ��
    }
}