using UnityEngine;

public class DoorUnlockScript : MonoBehaviour
{
    public GameObject openDoorImage; // ���� �� �̹��� ������Ʈ
    private BoxCollider2D doorCollider; // �� �ݶ��̴�
    private bool isDoorOpen = false;

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (openDoorImage != null)
        {
            openDoorImage.SetActive(false); // ���� �� ���� �� �̹��� ��Ȱ��ȭ
        }
        doorCollider.enabled = true; // �ݶ��̴� Ȱ��ȭ (Ʈ���ſ�)
    }

    // GameManager���� ȣ���Ͽ� �� ����
    public void UnlockDoor()
    {
        if (!isDoorOpen)
        {
            isDoorOpen = true;
            if (openDoorImage != null)
            {
                openDoorImage.SetActive(true); // ���� �� �̹��� Ȱ��ȭ
            }
            doorCollider.enabled = true; // Ʈ���� ����
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