using UnityEngine;

public class DoorUnlockScript : MonoBehaviour
{
    private Animator doorAnimator; // 문 애니메이터
    private BoxCollider2D doorCollider; // 문 콜라이더
    private bool isDoorOpen = false;

    void Start()
    {
        doorAnimator = GetComponent<Animator>();
        doorCollider = GetComponent<BoxCollider2D>();
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("isOpen", false);
        }
        doorCollider.enabled = true; // 문 잠김
    }

    // GameManager에서 호출하여 문 열기
    public void UnlockDoor()
    {
        if (!isDoorOpen)
        {
            isDoorOpen = true;
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("isOpen", true); // 문 열림 애니메이션
            }
            doorCollider.enabled = false; // 플레이어 통과 가능
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