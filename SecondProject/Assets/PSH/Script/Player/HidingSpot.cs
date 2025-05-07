using UnityEngine;

public class HidingSpot : Interactable
{
    public Transform hidePosition;

    private Player playerScript;

    private void Start()
    {
        playerScript = FindFirstObjectByType<Player>();
    }

    // 플레이어가 이 구조물과 상호작용(E키)할 때 실행되는 함수
    public override void Interact()
    {
        if (playerScript == null) return;

        if (!playerScript.IsHiding())
        {
            playerScript.SetHiding(true, this); // 현재 구조물 기준으로 숨기
        }
        else if (playerScript.currentSpot == this)
        {
            playerScript.SetHiding(false, null); // 숨기 해제 및 구조물 참조 제거
        }
    }
}
