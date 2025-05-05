using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isHoldInteraction = false;
    public Item item;
    public Coin coin;
    public bool isVendingMachine = false;
    public string miniGameId; // MiniGameManager에 전달할 ID
    public GameObject cleanMirror;
    public GameObject closedWireBox;
    private bool isMiniGameCompleted = false;

    public virtual void Interact()
    {
        if (isMiniGameCompleted && !string.IsNullOrEmpty(miniGameId))
        {
            Debug.Log("미니게임 이미 완료됨!");
            return;
        }

        Player player = FindFirstObjectByType<Player>(); // 22줄 수정

        if (!string.IsNullOrEmpty(miniGameId))
        {
            FindFirstObjectByType<MiniGameManager>().StartMiniGame(miniGameId, this); // 26줄 수정
            return;
        }

        if (coin != null)
        {
            coin.Collect(player);
        }
        else if (item != null)
        {
            if (player.AddItem(item))
            {
                Destroy(gameObject);
            }
        }
        else if (isVendingMachine)
        {
            if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
            {
                PhoneManager.Instance.TogglePhoneScreen();
            }
            player.ShowVendingMachinePanel();
        }
    }

    public void OnMiniGameCompleted()
    {
        isMiniGameCompleted = true;

        if (miniGameId == "MirrorCleaning" || miniGameId == "Mannequin")
        {
            gameObject.SetActive(false);
            if (cleanMirror != null)
            {
                cleanMirror.SetActive(true);
            }
        }
        else if (miniGameId == "Wiring")
        {
            gameObject.SetActive(false);
            if (closedWireBox != null)
            {
                closedWireBox.SetActive(true);
            }
        }
        else if (miniGameId == "Carpet" || miniGameId == "Glass" || miniGameId == "Light" || miniGameId == "Vent" || miniGameId == "Radio")
        {
            gameObject.SetActive(false);
        }
    }
}