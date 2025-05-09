using UnityEngine;

public class DoorUnlockScript : MonoBehaviour
{
    private Animator doorAnimator; // �� �ִϸ�����
    private BoxCollider2D doorCollider; // �� �ݶ��̴�
    private bool isDoorOpen = false;

    void Start()
    {
        doorAnimator = GetComponent<Animator>();
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", false);
        }
        doorCollider.enabled = true; // �� ���
    }

    // GameManager���� ȣ���Ͽ� �� ����
    public void UnlockDoor()
    {
        if (!isDoorOpen)
        {
            isDoorOpen = true;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("isOpen", true); // �� ���� �ִϸ��̼�
            }
            doorCollider.enabled = false; // �÷��̾� ��� ����
            Debug.Log("���� ���ȴ�!");
        }
    }

    // �÷��̾ ���� �������� ��
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isDoorOpen)
        {
            GameManager.Instance.CheckStageClear();
        }
    }
}