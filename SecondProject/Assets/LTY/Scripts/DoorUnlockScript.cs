using UnityEngine;

public class DoorUnlockScript : MonoBehaviour
{
    public GameObject openDoorImage; // 열린 문 이미지 오브젝트
    private BoxCollider2D doorCollider; // 문 콜라이더
    private bool isDoorOpen = false;

    void Start()
    {
        doorCollider = GetComponent<BoxCollider2D>();
        if (openDoorImage != null)
        {
            openDoorImage.SetActive(false); // 시작 시 열린 문 이미지 비활성화
        }
        doorCollider.enabled = true; // 콜라이더 활성화 (트리거용)
    }

    // GameManager에서 호출하여 문 열기
    public void UnlockDoor()
    {
        if (!isDoorOpen)
        {
            isDoorOpen = true;
            if (openDoorImage != null)
            {
                openDoorImage.SetActive(true); // 열린 문 이미지 활성화
            }
            doorCollider.enabled = true; // 트리거 유지
            Debug.Log("문이 열렸다!");
        }
    }

    // 플레이어가 문에 도달했을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isDoorOpen)
        {
            GameManager.Instance.CheckStageClear();
        }
    }
}