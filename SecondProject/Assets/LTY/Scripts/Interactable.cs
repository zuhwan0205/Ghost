using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isHoldInteraction = false;
    public Item item;
    public Coin coin;
    public bool isVendingMachine = false;
    public string miniGameId; // "Glass" 또는 "Vent"로 설정 확인
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

        Player player = FindFirstObjectByType<Player>();

        if (!string.IsNullOrEmpty(miniGameId))
        {
            Debug.Log($"Starting minigame: {miniGameId} with interactable: {this}");
            FindFirstObjectByType<MiniGameManager>().StartMiniGame(miniGameId, this);
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