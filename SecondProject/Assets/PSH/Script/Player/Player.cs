using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;

// 플레이어의 상태와 동작을 관리하는 클래스
public class Player : PlayerManager
{
    #region 변수 선언
    [Header("플레이어 정보")]
    [SerializeField] public float Player_Hp = 10f;      // 현재 체력
    [SerializeField] private float maxHp = 10f;         // 최대 체력
    [SerializeField] private Slider healthBar;          // 체력바 UI

    [Header("스태미나 정보")]
    [SerializeField] private float Stemina = 100f;      // 현재 스태미나
    [SerializeField] private float maxStemina = 100f;   // 최대 스태미나
    [SerializeField] private float SteminaConsumed;     // 스태미나 소모량
    [SerializeField] private Slider staminaBar;         // 스태미나바 UI

    [Header("이동 정보")]
    [SerializeField] public float moveSpeed;            // 이동 속도
    [HideInInspector] public float originalMoveSpeed;   // 초기 이동 속도 (변경 전 저장용)
    [HideInInspector] public float originalDashSpeed;   // 초기 대시 속도 (변경 전 저장용)

    [Header("대쉬 정보")]
    [SerializeField] public float dashSpeed;           // 대시 속도
    [SerializeField] private float dashTime;            // 대시 지속 시간
    [SerializeField] private bool isDashing = false;     // 대시 중인지 여부
     
    [Header("상호작용")]
    [SerializeField] private float interactionRadius = 1f;  // 상호작용 범위
    [SerializeField] private LayerMask interactableLayer;   // 상호작용 가능한 오브젝트 레이어
    [SerializeField] public float holdTime = 2f;            // 상호작용 유지 시간

    [Header("인벤토리")]
    [SerializeField] private List<Item> inventory = new List<Item>(3); // 아이템 목록 (최대 3개)
    [SerializeField] private Image[] inventorySlots;                   // 인벤토리 배경 UI 슬롯
    [SerializeField] private Image[] itemImages;                      // 아이템 이미지 UI 슬롯
    [SerializeField] private Image[] borderSlots;                     // 인벤토리 테두리 UI 슬롯
    [SerializeField] private int selectedSlot = 0;                     // 현재 선택된 슬롯 번호

    [Header("동전 및 자판기")]
    [SerializeField] private int coinAmount = 0;            // 현재 동전 개수
    [SerializeField] private TextMeshProUGUI coinText;      // 동전 개수 표시 UI
    [SerializeField] private GameObject vendingMachinePanel;// 자판기 UI 패널

    [Header("사운드설정")]
    [SerializeField] private AudioSource[] walkSources;     // 플레이어가 맞았을 때 재생되는 사운드
    [SerializeField] private AudioSource[] dashSources;     // 대시할 때 재생되는 사운드
    [SerializeField] private string hitPlayerSound = "player_hit";
    [SerializeField] private string diePlayerSound = "player_die";
    [SerializeField] private AudioSource[] grabSources;     // 잡혔을 당시 재생되는 사운드
    [SerializeField] private AudioSource[] afterGrabSources;    //잡히고 나서의 재생되는 사운드
    [SerializeField] private AudioSource[] hideSources;         //잡히고 나서의 재생되는 사운드


    // === [추가] 깡통 투척용 프리팹 ===
    [Header("깡통 투척")]
    [SerializeField] private GameObject canPrefab; // 깡통 프리팹
    [SerializeField] private string throwCanSound = "can_throw"; // 깡통 투척 사운드

    private float holdTimer = 0f;               // 상호작용 유지 시간 계산용 
    private bool isHolding = false;             // 상호작용 유지 중인지 여부
    private Interactable currentInteractable;   // 현재 상호작용 중인 오브젝트
    public bool isInteractionLocked = false;    // 상호작용 잠금 기능
    public float interactionTime = 0f;

    [SerializeField] private GameObject interactionHint;    // 상호작용 가능 표시 UI
    [SerializeField] private Slider holdProgressBar;        // 상호작용 진행 바 UI

    private float xInput;   // 좌우 이동 입력 값
    public bool DidMoveOrInteract { get; private set; } = false;

    // 미니게임 참조 추가
    private WiringGameManager wiringGameManager;
    private MirrorCleaningGame mirrorCleaningGame;

    public bool isHiding = false; // 숨기 상태 여부
    public HidingSpot currentSpot = null; // 접촉 중인 숨는 구조체


    private Animator anim;

    #endregion

    #region 플레이어 상태 접근 메서드
    // 최대 체력을 반환
    public float GetMaxHp() => maxHp;

    // 최대 스태미나를 반환
    public float GetMaxStemina() => maxStemina;

    // 현재 스태미나를 반환
    public float GetStemina() => Stemina;

    // 스태미나 값을 설정
    public void SetStemina(float value) => Stemina = value;

    // 현재 동전 개수를 반환
    public int GetCoinAmount() => coinAmount;

    // 동전을 추가하고 UI를 업데이트
    public void AddCoin(int amount)
    {
        coinAmount += amount;
        UpdateCoinUI();
    }

    // 동전을 사용 (구매 시 호출)
    public bool SpendCoin(int amount)
    {
        if (coinAmount >= amount)
        {
            coinAmount -= amount;
            UpdateCoinUI();
            return true;
        }
        Debug.Log("동전 부족!");
        return false;
    }

    // 자판기 UI를 표시
    public void ShowVendingMachinePanel()
    {
        if (vendingMachinePanel != null)
        {
            vendingMachinePanel.SetActive(true);
            UpdateCoinUI();
        }
    }

    // 자판기 UI를 숨김
    public void HideVendingMachinePanel()
    {
        if (vendingMachinePanel != null)
            vendingMachinePanel.SetActive(false);
    }

    // 자판기 UI가 열려 있는지 확인
    private bool IsVendingMachineOpen()
    {
        return vendingMachinePanel != null && vendingMachinePanel.activeSelf;
    }

    public float CurrentSpeed
    {
        get
        {
            return Mathf.Abs(rb.linearVelocity.x);
        }
    }

    #endregion

    #region 초기화 및 업데이트
    // 게임 시작 시 초기 설정
    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed;
        originalDashSpeed = dashSpeed;

        // 미니게임 참조 초기화
        wiringGameManager = FindFirstObjectByType<WiringGameManager>(FindObjectsInactive.Include);
        mirrorCleaningGame = FindFirstObjectByType<MirrorCleaningGame>(FindObjectsInactive.Include);
        if (wiringGameManager == null)
            Debug.LogWarning("WiringGameManager 스크립트를 Scene에서 찾을 수 없습니다!");
        if (mirrorCleaningGame == null)
            Debug.LogWarning("MirrorCleaningGame 스크립트를 Scene에서 찾을 수 없습니다!");

        // 체력바 초기화
        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = Player_Hp;
        }
        else
            Debug.LogError("HealthBar가 할당되지 않았습니다!");

        // 스태미나바 초기화
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStemina;
            staminaBar.value = Stemina;
        }
        else
            Debug.LogError("StaminaBar가 할당되지 않았습니다!");

        // 상호작용 UI 초기화
        if (holdProgressBar == null)
            Debug.LogError("HoldProgressBar가 할당되지 않았습니다!");
        if (interactionHint == null)
            Debug.LogError("InteractionHint가 할당되지 않았습니다!");

        // 인벤토리 슬롯 초기화 및 연결 확인
        if (inventorySlots.Length != 3)
            Debug.LogError("인벤토리 배경 슬롯은 3개여야 합니다!");
        if (itemImages.Length != 3)
            Debug.LogError("아이템 이미지 슬롯은 3개여야 합니다!");
        if (borderSlots == null || borderSlots.Length != 3)
        {
            Debug.LogError("테두리 슬롯은 3개여야 합니다!");
            return; // 배열이 유효하지 않으면 초기화 중단
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
                Debug.LogError($"인벤토리 배경 슬롯 {i}가 연결되지 않았습니다!");
            else if (!inventorySlots[i].gameObject.activeInHierarchy)
                Debug.LogWarning($"인벤토리 배경 슬롯 {i}가 비활성화 상태입니다!");
            inventorySlots[i].color = new Color(0f, 47f / 255f, 108f / 255f); // 군청색
            inventorySlots[i].sprite = null; // 배경은 스프라이트 없음
        }
        for (int i = 0; i < 3; i++)
        {
            inventory.Add(null);
            if (itemImages[i] != null)
            {
                itemImages[i].color = Color.white; // 아이템 원본 색상 유지
                itemImages[i].sprite = null;
                itemImages[i].gameObject.SetActive(false); // 아이템 먹기 전에는 비활성화
            }
            else
                Debug.LogError($"아이템 이미지 슬롯 {i}가 연결되지 않았습니다!");
            if (borderSlots[i] != null)
            {
                borderSlots[i].gameObject.SetActive(false); // 초기에는 테두리 비활성화
            }
            else
                Debug.LogError($"테두리 슬롯 {i}가 연결되지 않았습니다!");
        }

        // === [추가] 테스트용 깡통 아이템 추가 ===
        Item canItem = Resources.Load<Item>("Can");
        if (canItem != null)
        {
            AddItem(canItem);
            Debug.Log("[Player] 테스트용 깡통 아이템 추가됨");
        }
        else
        {
            Debug.LogError("[Player] 깡통 아이템(Can)을 Resources에서 찾을 수 없음. 경로: Assets/Resources/Can.asset 확인!");
        }

        // 동전 및 자판기 UI 초기화
        if (coinText == null)
            Debug.LogError("CoinText가 할당되지 않았습니다!");
        if (vendingMachinePanel == null)
            Debug.LogError("VendingMachinePanel이 할당되지 않았습니다!");
        vendingMachinePanel.SetActive(false);
        UpdateCoinUI();
    }

    // 매 프레임마다 호출 (플레이어 상태 업데이트)
    protected override void Update()
    {
        base.Update();

        CheckInput();
        CheckHoldInteraction();
        CheckInventoryInput();
        Movement();
        FlipController();
        RecoveryStemina();
        UpdateUI();
        CheckMiniGameCancellation(); // 미니게임 취소 확인
    }

    // 동전 UI를 업데이트 (화면에 동전 개수 표시)
    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {coinAmount}";
    }

    // 체력바와 스태미나바 UI를 업데이트
    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = Player_Hp;
        if (staminaBar != null)
            staminaBar.value = Stemina;
    }
    #endregion

    #region 플레이어 관리
    // 스태미나를 천천히 회복
    private void RecoveryStemina()
    {
        if (Stemina < maxStemina && !isDashing)
        {
            Stemina += Time.deltaTime * 0.5f;
            if (Stemina > maxStemina)
                Stemina = maxStemina;
        }
    }

    // 물리 충돌 확인 (부모 클래스에서 상속)
    protected override void CollisionChecks()
    {
        base.CollisionChecks();
    }

    // 데미지를 받아 체력 감소
    public void TakeDamage(float _damage)
    {
        Player_Hp -= _damage;
        if (Player_Hp < 0)
        {
            Player_Hp = 0;
            AudioManager.Instance.PlayAt(diePlayerSound, transform.position);
            GameManager.Instance.GameOver();
        }
    }

    public void TakeGrab()
    {
        anim.SetBool("Grab", true);

        StopAudioGroup(walkSources);
        StopAudioGroup(dashSources);
        PlayAudioGroup(grabSources);
    }

    public void AfterGrab()
    {
        anim.SetBool("Grab", false);
        PlayAudioGroup(afterGrabSources);
        StopAudioGroup(grabSources);
    }



    // 사운드 끄기 
    private void StopAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && source.isPlaying)
                source.Stop();
        }
    }

    // 사운드 재생 
    private void PlayAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && !source.isPlaying)
                source.Play();
        }
    }


    #endregion

    #region 플레이어 조작
    // 키보드 입력 확인 (이동, 대시, 데미지 등)
    private void CheckInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        // 자판기 또는 휴대폰 UI가 열려 있을 때 이동 입력 감지
        if (xInput != 0)
        {
            if (IsVendingMachineOpen())
            {
                HideVendingMachinePanel();
                Debug.Log("이동 입력 감지: 자판기 UI 닫힘");
                return;
            }
            if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
            {
                PhoneManager.Instance.TogglePhoneScreen();
                Debug.Log("이동 입력 감지: 휴대폰 UI 닫힘");
                return;
            }
        }

        DidMoveOrInteract = (xInput != 0f || Input.GetKeyDown(KeyCode.E));

        if (Input.GetKey(KeyCode.LeftShift))
            DashAbility();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            DashEnd();

        if (Input.GetKeyDown(KeyCode.H))
            TakeDamage(1f);
    }

    // 상호작용 확인 (E키로 오브젝트와 상호작용)
    private void CheckHoldInteraction()
    {
        if (isInteractionLocked)
        {
            interactionHint.SetActive(false);
            holdProgressBar.gameObject.SetActive(false);
            return; // E키 기능 완전 차단
        }

        // 미니게임 진행 중이면 E 키 무시 또는 취소 처리
        bool isMiniGameActive = (wiringGameManager != null && wiringGameManager.IsMiniGameActive) ||
                                (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf);
        if (isMiniGameActive)
        {
            interactionHint.SetActive(false);
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (wiringGameManager != null && wiringGameManager.IsMiniGameActive)
                {
                    wiringGameManager.CancelGame();
                    Debug.Log("E 키 입력: 전선 연결 미니게임 취소!");
                }
                if (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf)
                {
                    mirrorCleaningGame.CancelGame();
                    Debug.Log("E 키 입력: 거울 닦기 미니게임 취소!");
                }
            }
            return;
        }

        if (Input.GetKey(KeyCode.E)) interactionTime += Time.deltaTime;
        else interactionTime = 0f;

            Collider2D hit = Physics2D.OverlapCircle(transform.position, interactionRadius, interactableLayer);
        interactionHint.SetActive(hit != null);

        if (Input.GetKeyDown(KeyCode.E) && hit)
        {
            currentInteractable = hit.GetComponent<Interactable>();
            if (currentInteractable != null)
            {
                if (currentInteractable.isHoldInteraction)
                {
                    isHolding = true;
                    holdTimer = 0f;

                    //홀드중에는 이동불가
                    moveSpeed = 0f;
                    dashSpeed = 0f;
                }
                else
                {
                    currentInteractable.Interact();
                }
            }
        }

        if (isHolding && currentInteractable != null && currentInteractable.isHoldInteraction)
        {
            holdTimer += Time.deltaTime;
            holdProgressBar.gameObject.SetActive(true);
            holdProgressBar.value = holdTimer / holdTime;
            if (holdTimer >= holdTime)
            {
                currentInteractable.Interact();
                ResetHold();
            }
        }
        else
        {
            holdProgressBar.gameObject.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.E))
            ResetHold();
    }

    // 상호작용 유지 상태 초기화
    public void ResetHold()
    {
        holdTimer = 0f;
        isHolding = false;
        holdProgressBar.gameObject.SetActive(false);

        //홀드 해제시 이동속도 복구
        moveSpeed = originalMoveSpeed;
        dashSpeed = originalDashSpeed;

    }

    // 미니게임 취소 확인 메서드 (조건 개선)
    private void CheckMiniGameCancellation()
    {
        // 미니게임 UI가 실제로 활성화된 경우에만 체크
        bool isMiniGameActive = (wiringGameManager != null && wiringGameManager.IsMiniGameActive) ||
                                (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf);
        if (!isMiniGameActive)
        {
            Debug.Log("미니게임이 활성화되지 않음: 취소 불필요");
            return;
        }

        // 이동 입력 감지로만 취소
        if (xInput != 0)
        {
            CancelMiniGame();
            Debug.Log("플레이어 이동 감지: 미니게임 취소!");
            return;
        }
    }

    // 미니게임 취소 메서드
    private void CancelMiniGame()
    {
        if (wiringGameManager != null && wiringGameManager.IsMiniGameActive)
        {
            wiringGameManager.CancelGame();
        }
        if (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf)
        {
            mirrorCleaningGame.CancelGame();
        }
    }

    // 인벤토리 입력 확인 (아이템 사용, 슬롯 전환)
    private void CheckInventoryInput()
    {
        // 미니게임 활성화 여부 확인
        bool isMiniGameActive = (wiringGameManager != null && wiringGameManager.IsMiniGameActive) ||
                                (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf);
        if (isMiniGameActive)
        {
            Debug.Log("미니게임 진행 중: 아이템 사용 및 슬롯 전환 비활성화");
            return;
        }

        // 자판기 UI가 열려 있을 때
        if (IsVendingMachineOpen())
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0f || Input.GetMouseButtonDown(1))
            {
                HideVendingMachinePanel();
                Debug.Log("아이템 사용 또는 슬롯 전환 감지: 자판기 UI 닫힘");
                return;
            }
        }

        // 휴대폰 UI가 열려 있을 때
        if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0f || Input.GetMouseButtonDown(1))
            {
                PhoneManager.Instance.TogglePhoneScreen();
                Debug.Log("아이템 사용 또는 슬롯 전환 감지: 휴대폰 UI 닫힘");
                return;
            }
            Debug.Log("휴대폰 화면 열려 있음: 인벤토리 입력 무시");
            return;
        }

        float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scrollInput > 0f && selectedSlot < 2)
        {
            selectedSlot++;
            Debug.Log($"아이템 슬롯 전환: {selectedSlot}");
        }
        else if (scrollInput < 0f && selectedSlot > 0)
        {
            selectedSlot--;
            Debug.Log($"아이템 슬롯 전환: {selectedSlot}");
        }

        // 테두리 활성화/비활성화 (배열 범위 체크 추가)
        if (borderSlots != null && borderSlots.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(inventorySlots.Length, borderSlots.Length); i++)
            {
                if (borderSlots[i] != null)
                {
                    borderSlots[i].gameObject.SetActive(i == selectedSlot); // 선택된 슬롯만 테두리 활성화
                }
                else
                {
                    Debug.LogWarning($"테두리 슬롯 {i}가 null입니다!");
                }
            }
        }
        else
        {
            Debug.LogWarning("borderSlots 배열이 null이거나 비어 있습니다!");
        }

        // 아이템 사용 (우클릭)
        if (Input.GetMouseButtonDown(1) && inventory[selectedSlot] != null)
        {
            Debug.Log($"아이템 사용: {inventory[selectedSlot].itemName}");
            inventory[selectedSlot].Use(this);
            inventory[selectedSlot] = null;
            if (itemImages[selectedSlot] != null)
            {
                itemImages[selectedSlot].sprite = null;
                itemImages[selectedSlot].gameObject.SetActive(false); // 아이템 이미지 비활성화
            }
        }
    }

    // 인벤토리에 아이템 추가
    public bool AddItem(Item item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = item;
                if (item == null)
                {
                    Debug.LogError("추가하려는 아이템이 null입니다!");
                    return false;
                }
                if (item.icon == null)
                    Debug.LogWarning($"아이템 {item.itemName}의 이미지가 없습니다!");
                if (itemImages[i] != null)
                {
                    itemImages[i].sprite = item.icon;
                    itemImages[i].gameObject.SetActive(true); // 아이템 이미지 활성화
                    Debug.Log($"슬롯 {i}에 이미지 설정: {itemImages[i].sprite}");
                }
                return true;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

    // 플레이어의 숨기 상태를 설정하는 함수 
    public void SetHiding(bool hide, HidingSpot spot)
    {
        isHiding = hide;

        // 현재 오브젝트에서 바로 가져오기
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        Transform light1 = transform.Find("플레이어 주변광");
        Transform light2 = transform.Find("휴대폰 불빛");

        if (light1 != null)
            light1.gameObject.SetActive(!hide);

        if (light2 != null)
            light2.gameObject.SetActive(!hide);


        if (spriteRenderer != null)
            spriteRenderer.enabled = !hide;

        if (anim != null)
            anim.enabled = !hide;

        // 이동 제한 및 태그/레이어 변경
        moveSpeed = hide ? 0f : originalMoveSpeed;
        dashSpeed = hide ? 0f : originalDashSpeed;
        tag = hide ? "Hide" : "Player";
        gameObject.layer = LayerMask.NameToLayer(hide ? "Hide" : "Player");

        if (hide)
        {
            currentSpot = spot;
            Debug.Log("[Player] 숨었습니다.");

            PlayAudioGroup(hideSources);

        }
        else
        {
            // 숨기 해제 시 살짝 앞으로 이동 (현재 바라보는 방향 기준)
            transform.position += Vector3.right * 0.3f * transform.localScale.x;
            Debug.Log("[Player] 나왔습니다.");

            PlayAudioGroup(hideSources);
        }
    }

    // 외부에서 숨기 상태를 확인할 수 있도록
    public bool IsHiding()
    {
        return isHiding;
    }

    // === [추가] 깡통 투척 메서드 ===
    public void ThrowCan()
    {
        if (canPrefab == null)
        {
            Debug.LogError("[Player] 깡통 프리팹이 인스펙터에 할당되지 않았습니다!");
            return;
        }
        // 생성 위치: 플레이어 앞(2f), 어깨 높이(y=1.2f)
        Vector3 spawnPos = transform.position + new Vector3(facingRight ? 1f : -2f, 3f, 0);
        GameObject can = Instantiate(canPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D canRb = can.GetComponent<Rigidbody2D>();
        if (canRb == null)
        {
            Debug.LogError("[Player] 깡통 프리팹에 Rigidbody2D가 없습니다!");
            return;
        }
        // 방향: 수평+위쪽 포물선
        Vector2 throwDirection = (facingRight ? Vector2.right : Vector2.left) + Vector2.up * 0.5f;
        float throwForce = 15f;
        canRb.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);
        AudioManager.Instance.PlayAt(throwCanSound, transform.position);
        Debug.Log($"[Player] 깡통을 던졌다! 생성 위치: {spawnPos}, 방향: {throwDirection}, 힘: {throwForce}");
    }

    #endregion

    #region 플레이어 이동
    // 대시 기능 활성화
    private void DashAbility()
    {
        if (Stemina > 0)
        {
            isDashing = true;
            Stemina -= Time.deltaTime * SteminaConsumed;
            dashTime = Time.deltaTime;

            if (Stemina < 0)
            {
                Stemina = 0;
                dashTime = 0;
            }
        }
    }

    // 대시 기능 종료
    private void DashEnd()
    {
        isDashing = false;
        dashTime = 0f;
    }

    // 플레이어 이동 처리
    private void Movement()
    {
        if (isHiding)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // 강제로 정지
            anim.SetBool("Run", false);
            anim.SetBool("Walk", false);
            StopAudioGroup(walkSources);
            StopAudioGroup(dashSources);

            return;
        }

        bool isMoving = Mathf.Abs(xInput) > 0.01f;
        bool isDashing = dashTime > 0;

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(xInput * (moveSpeed + dashSpeed), rb.linearVelocity.y);
            anim.SetBool("Run", true);
            anim.SetBool("Walk", false);
            PlayAudioGroup(dashSources);
            StopAudioGroup(walkSources);
        }
        else
        {
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);

            if (isMoving)
            {
                anim.SetBool("Run", false);
                anim.SetBool("Walk", true);
                PlayAudioGroup(walkSources);
                StopAudioGroup(dashSources);
            }
            else
            {
                anim.SetBool("Run", false);
                anim.SetBool("Walk", false);
                StopAudioGroup(walkSources);
                StopAudioGroup(dashSources);
            }
        }
    }

    // 플레이어 방향 전환 (좌우 반전)
    private void FlipController()
    {
        if (rb.linearVelocity.x > 0 && !facingRight)
            Flip();
        else if (rb.linearVelocity.x < 0 && facingRight)
            Flip();
    }

    // 대시 속도를 설정
    public void SetDashSpeed(float speed)
    {
        dashSpeed = speed;
    }
    #endregion

}