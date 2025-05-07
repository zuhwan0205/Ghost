using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isHoldInteraction = false;
    public Item item;
    public Coin coin;
    public bool isVendingMachine = false;
    public bool isDirtyMirror = false;
    public bool isWireBox = false;
    public GameObject cleanMirror;
    public GameObject closedWireBox; // 닫힌 스위치 보드 오브젝트
    [SerializeField] private MirrorCleaningGame cleaningGame;
    [SerializeField] private WiringGameManager wiringGameManager;
    private bool isMiniGameCompleted = false; // 미니게임 완료 여부

    void Start()
    {
        // 거울 청소 미니게임 초기화
        if (isDirtyMirror && cleaningGame == null)
        {
            cleaningGame = FindFirstObjectByType<MirrorCleaningGame>();
            if (cleaningGame == null)
            {
                Debug.LogWarning("MirrorCleaningGame 스크립트를 Scene에서 찾을 수 없습니다!");
            }
        }

        // 전선 연결 미니게임 초기화
        if (isWireBox && wiringGameManager == null)
        {
            wiringGameManager = FindFirstObjectByType<WiringGameManager>();
            if (wiringGameManager == null)
            {
                Debug.LogWarning("WiringGameManager 스크립트를 Scene에서 찾을 수 없습니다!");
            }
        }

        // 닫힌 스위치 보드 초기 비활성화
        if (isWireBox && closedWireBox != null)
        {
            closedWireBox.SetActive(false);
        }
    }

    public virtual void Interact()
    {
        // 미니게임 완료 시 재상호작용 방지
        if ((isDirtyMirror || isWireBox) && isMiniGameCompleted)
        {
            Debug.Log("미니게임 이미 완료됨: 추가 상호작용 불가!");
            return;
        }

        Player player = Object.FindFirstObjectByType<Player>();

        // 전선 상자와 상호작용
        if (isWireBox)
        {
            Debug.Log("전선 상자 상호작용 시작!");
            if (wiringGameManager != null)
            {
                Debug.Log("전선 연결 미니게임 시작 호출!");
                wiringGameManager.StartMiniGame(this);
            }
            else
            {
                Debug.LogError("WiringGameManager 스크립트가 설정되지 않았습니다!");
            }
            return;
        }

        // 더러운 거울과 상호작용
        if (isDirtyMirror)
        {
            Debug.Log("더러운 거울 상호작용 시작!");
            if (cleaningGame != null)
            {
                Debug.Log("미니게임 시작 호출!");
                cleaningGame.StartMiniGame(this);
            }
            else
            {
                Debug.LogError("MirrorCleaningGame 스크립트가 설정되지 않았습니다!");
            }
            return;
        }

        // 동전, 아이템, 자판기 상호작용
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
                Debug.Log("자판기 상호작용: 휴대폰 UI 닫힘");
            }
            player.ShowVendingMachinePanel();
        }
    }

    public void OnMiniGameCompleted()
    {
        isMiniGameCompleted = true; // 완료 상태로 설정

        if (isDirtyMirror)
        {
            Debug.Log("미니게임 완료: 거울 교체!");
            gameObject.SetActive(false);
            if (cleanMirror != null)
            {
                cleanMirror.SetActive(true);
            }
        }
        else if (isWireBox)
        {
            Debug.Log("전선 연결 미니게임 완료: 스위치 보드 닫힘!");
            gameObject.SetActive(false); // 열린 스위치 보드 비활성화
            if (closedWireBox != null)
            {
                closedWireBox.SetActive(true); // 닫힌 스위치 보드 활성화
            }
            else
            {
                Debug.LogWarning("closedWireBox가 설정되지 않았습니다!");
            }
        }
    }
}