using UnityEngine;

public class HidingSpot : Interactable
{
    public Transform hidePosition;

    private Player playerScript;

    private void Start()
    {
        playerScript = FindFirstObjectByType<Player>();
    }

    // �÷��̾ �� �������� ��ȣ�ۿ�(EŰ)�� �� ����Ǵ� �Լ�
    public override void Interact()
    {
        if (playerScript == null) return;

        if (!playerScript.IsHiding())
        {
            playerScript.SetHiding(true, this); // ���� ������ �������� ����
        }
        else if (playerScript.currentSpot == this)
        {
            playerScript.SetHiding(false, null); // ���� ���� �� ������ ���� ����
        }
    }
}
