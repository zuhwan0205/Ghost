﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Rendering;
using System.Collections; // 수정: 코루틴 사용을 위해 추가

public class Player : PlayerManager
{
    #region 변수 선언
    [Header("플레이어 정보")]
    [SerializeField] public float Player_Hp = 10f;
    [SerializeField] private float maxHp = 10f;
    [SerializeField] private Slider healthBar;

    [Header("스태미나 정보")]
    [SerializeField] private float Stemina = 100f;
    [SerializeField] private float maxStemina = 100f;
    [SerializeField] private float SteminaConsumed;
    [SerializeField] private Slider staminaBar;

    [Header("이동 정보")]
    [SerializeField] public float moveSpeed;
    [HideInInspector] public float originalMoveSpeed;
    [HideInInspector] public float originalDashSpeed;

    [Header("대쉬 정보")]
    [SerializeField] public float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private bool isDashing = false;

    [Header("상호작용")]
    [SerializeField] private float interactionRadius = 1f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] public float holdTime = 2f;

    [Header("인벤토리")]
    [SerializeField] private List<Item> inventory = new List<Item>(3);
    [SerializeField] private Image[] inventorySlots;
    [SerializeField] private Image[] itemImages;
    [SerializeField] private Image[] borderSlots;
    [SerializeField] private int selectedSlot = 0;

    [Header("동전 및 자판기")]
    [SerializeField] private int coinAmount = 0;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject vendingMachinePanel;

    [Header("사운드설정")]
    [SerializeField] private AudioSource[] walkSources;
    [SerializeField] private AudioSource[] dashSources;
    [SerializeField] private string hitPlayerSound = "player_hit";
    [SerializeField] private string diePlayerSound = "player_die";
    [SerializeField] private AudioSource[] grabSources;     // 잡혔을 당시 재생되는 사운드
    [SerializeField] private AudioSource[] afterGrabSources;    //잡히고 나서의 재생되는 사운드
    [SerializeField] private AudioSource[] hideSources;         //잡히고 나서의 재생되는 사운드


    [Header("깡통 투척")]
    [SerializeField] private GameObject canPrefab;
    [SerializeField] private string throwCanSound = "can_throw";

    private float holdTimer = 0f;
    private bool isHolding = false;
    private Interactable currentInteractable;
    public bool isInteractionLocked = false;
    public float interactionTime = 0f;

    [SerializeField] private GameObject interactionHint;
    [SerializeField] private Slider holdProgressBar;

    private float xInput;
    public bool DidMoveOrInteract { get; private set; } = false;

    private WiringGameManager wiringGameManager;
    private MirrorCleaningGame mirrorCleaningGame;

    public bool isHiding = false;
    public HidingSpot currentSpot = null;

    private Animator anim;

    #endregion

    #region 플레이어 상태 접근 메서드
    public float GetMaxHp() => maxHp;
    public float GetMaxStemina() => maxStemina;
    public float GetStemina() => Stemina;
    public void SetStemina(float value) => Stemina = value;
    public int GetCoinAmount() => coinAmount;

    public void AddCoin(int amount)
    {
        coinAmount += amount;
        UpdateCoinUI();
    }

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

    public void ShowVendingMachinePanel()
    {
        if (vendingMachinePanel != null)
        {
            vendingMachinePanel.SetActive(true);
            UpdateCoinUI();
        }
    }

    public void HideVendingMachinePanel()
    {
        if (vendingMachinePanel != null)
            vendingMachinePanel.SetActive(false);
    }

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
    protected override void Start()
    {
        base.Start();

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed;
        originalDashSpeed = dashSpeed;

        wiringGameManager = FindFirstObjectByType<WiringGameManager>(FindObjectsInactive.Include);
        mirrorCleaningGame = FindFirstObjectByType<MirrorCleaningGame>(FindObjectsInactive.Include);
        if (wiringGameManager == null)
            Debug.LogWarning("WiringGameManager 스크립트를 Scene에서 찾을 수 없습니다!");
        if (mirrorCleaningGame == null)
            Debug.LogWarning("MirrorCleaningGame 스크립트를 Scene에서 찾을 수 없습니다!");

        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = Player_Hp;
        }
        else
            Debug.LogError("HealthBar가 할당되지 않았습니다!");

        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStemina;
            staminaBar.value = Stemina;
        }
        else
            Debug.LogError("StaminaBar가 할당되지 않았습니다!");

        if (holdProgressBar == null)
            Debug.LogError("HoldProgressBar가 할당되지 않았습니다!");
        if (interactionHint == null)
            Debug.LogError("InteractionHint가 할당되지 않았습니다!");

        if (inventorySlots.Length != 3)
            Debug.LogError("인벤토리 배경 슬롯은 3개여야 합니다!");
        if (itemImages.Length != 3)
            Debug.LogError("아이템 이미지 슬롯은 3개여야 합니다!");
        if (borderSlots == null || borderSlots.Length != 3)
        {
            Debug.LogError("테두리 슬롯은 3개여야 합니다!");
            return;
        }
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null)
                Debug.LogError($"인벤토리 배경 슬롯 {i}가 연결되지 않았습니다!");
            else if (!inventorySlots[i].gameObject.activeInHierarchy)
                Debug.LogWarning($"인벤토리 배경 슬롯 {i}가 비활성화 상태입니다!");
            inventorySlots[i].color = new Color(0f, 47f / 255f, 108f / 255f);
            inventorySlots[i].sprite = null;
        }
        for (int i = 0; i < 3; i++)
        {
            inventory.Add(null);
            if (itemImages[i] != null)
            {
                itemImages[i].color = Color.white;
                itemImages[i].sprite = null;
                itemImages[i].gameObject.SetActive(false);
            }
            else
                Debug.LogError($"아이템 이미지 슬롯 {i}가 연결되지 않았습니다!");
            if (borderSlots[i] != null)
            {
                borderSlots[i].gameObject.SetActive(false);
            }
            else
                Debug.LogError($"테두리 슬롯 {i}가 연결되지 않았습니다!");
        }

        

        if (coinText == null)
            Debug.LogError("CoinText가 할당되지 않았습니다!");
        if (vendingMachinePanel == null)
            Debug.LogError("VendingMachinePanel이 할당되지 않았습니다!");
        vendingMachinePanel.SetActive(false);
        UpdateCoinUI();
    }

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
        CheckMiniGameCancellation();
    }

    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = $"Coins: {coinAmount}";
    }

    private void UpdateUI()
    {
        if (healthBar != null)
            healthBar.value = Player_Hp;
        if (staminaBar != null)
            staminaBar.value = Stemina;
    }
    #endregion

    #region 플레이어 관리
    private void RecoveryStemina()
    {
        if (Stemina < maxStemina && !isDashing)
        {
            Stemina += Time.deltaTime * 0.5f;
            if (Stemina > maxStemina)
                Stemina = maxStemina;
        }
    }

    protected override void CollisionChecks()
    {
        base.CollisionChecks();
    }

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

    private void StopAudioGroup(AudioSource[] sources)
    {
        foreach (var source in sources)
        {
            if (source != null && source.isPlaying)
                source.Stop();
        }
    }

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
    private void CheckInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

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

    private void CheckHoldInteraction()
    {
        if (isInteractionLocked)
        {
            interactionHint.SetActive(false);
            holdProgressBar.gameObject.SetActive(false);
            return;
        }

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

    public void ResetHold()
    {
        holdTimer = 0f;
        isHolding = false;
        holdProgressBar.gameObject.SetActive(false);
        moveSpeed = originalMoveSpeed;
        dashSpeed = originalDashSpeed;
    }

    private void CheckMiniGameCancellation()
    {
        bool isMiniGameActive = (wiringGameManager != null && wiringGameManager.IsMiniGameActive) ||
                                (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf);
        if (!isMiniGameActive)
        {
            Debug.Log("미니게임이 활성화되지 않음: 취소 불필요");
            return;
        }

        if (xInput != 0)
        {
            CancelMiniGame();
            Debug.Log("플레이어 이동 감지: 미니게임 취소!");
            return;
        }
    }

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

    private void CheckInventoryInput()
    {
        bool isMiniGameActive = (wiringGameManager != null && wiringGameManager.IsMiniGameActive) ||
                                (mirrorCleaningGame != null && mirrorCleaningGame.gameObject.activeSelf);
        if (isMiniGameActive)
        {
            Debug.Log("미니게임 진행 중: 아이템 사용 및 슬롯 전환 비활성화");
            return;
        }

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

        if (borderSlots != null && borderSlots.Length > 0)
        {
            for (int i = 0; i < Mathf.Min(inventorySlots.Length, borderSlots.Length); i++)
            {
                if (borderSlots[i] != null)
                {
                    borderSlots[i].gameObject.SetActive(i == selectedSlot);
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

        // 수정: 아이템 사용 시 slotIndex 전달, 인벤토리/UI 직접 수정 제거
        if (Input.GetMouseButtonDown(1) && inventory[selectedSlot] != null)
        {
            Item item = inventory[selectedSlot];
            Debug.Log($"[Player] 아이템 사용: {item.itemName} in slot {selectedSlot}");
            item.Use(this, selectedSlot); // 수정: Use(this, selectedSlot) 호출
        }
    }

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
                    itemImages[i].gameObject.SetActive(true);
                    Debug.Log($"[Player] 슬롯 {i}에 이미지 설정: {item.itemName}");
                }
                return true;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

    // 수정: 인벤토리 아이템 교체 메서드 추가
    public void ReplaceItem(int slotIndex, Item newItem)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Count)
        {
            Debug.LogError($"[Player] 유효하지 않은 슬롯 인덱스: {slotIndex}");
            return;
        }
        inventory[slotIndex] = newItem;
        if (newItem != null)
        {
            if (itemImages[slotIndex] != null)
            {
                itemImages[slotIndex].sprite = newItem.icon;
                itemImages[slotIndex].gameObject.SetActive(true);
                Debug.Log($"[Player] 슬롯 {slotIndex}에 아이템 교체: {newItem.itemName}");
            }
            else
            {
                Debug.LogError($"[Player] itemImages[{slotIndex}]가 null입니다!");
            }
        }
        else
        {
            RemoveItem(slotIndex);
        }
    }

    // 수정: 인벤토리 아이템 제거 메서드 추가
    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Count)
        {
            Debug.LogError($"[Player] 유효하지 않은 슬롯 인덱스: {slotIndex}");
            return;
        }
        inventory[slotIndex] = null;
        if (itemImages[slotIndex] != null)
        {
            itemImages[slotIndex].sprite = null;
            itemImages[slotIndex].gameObject.SetActive(false);
            Debug.Log($"[Player] 슬롯 {slotIndex}에서 아이템 제거");
        }
    }

    public void SetHiding(bool hide, HidingSpot spot)
    {
        isHiding = hide;
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
            transform.position += Vector3.right * 0.3f * transform.localScale.x;
            Debug.Log("[Player] 나왔습니다.");

            PlayAudioGroup(hideSources);
        }
    }

    public bool IsHiding()
    {
        return isHiding;
    }

    // 수정: ThrowCan 최적화 (충돌 무시, 포물선 개선)
    public void ThrowCan()
    {
        if (canPrefab == null)
        {
            Debug.LogError("[Player] 깡통 프리팹이 인스펙터에 할당되지 않았습니다!");
            return;
        }
        Vector3 spawnPos = transform.position + new Vector3(facingRight ? 0f : 0f, 3f, 0);
        GameObject can = Instantiate(canPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D canRb = can.GetComponent<Rigidbody2D>();
        if (canRb == null)
        {
            Debug.LogError("[Player] 깡통 프리팹에 Rigidbody2D가 없습니다!");
            return;
        }
        Collider2D playerCol = GetComponent<Collider2D>();
        Collider2D canCol = can.GetComponent<Collider2D>();
        if (playerCol != null && canCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, canCol, true);
            StartCoroutine(EnableCollisionAfterDelay(playerCol, canCol, 1f));
        }
        Vector2 throwDirection = (facingRight ? Vector2.right : Vector2.left) + Vector2.up * 0.7f;
        float throwForce = 10f;
        canRb.AddForce(throwDirection.normalized * throwForce, ForceMode2D.Impulse);
        AudioManager.Instance.PlayAt(throwCanSound, transform.position);
        Debug.Log($"[Player] 깡통을 던졌다! 생성 위치: {spawnPos}, 방향: {throwDirection.normalized}, 힘: {throwForce}");
    }

    // 수정: 충돌 재활성화 코루틴 추가
    private IEnumerator EnableCollisionAfterDelay(Collider2D col1, Collider2D col2, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col1 != null && col2 != null)
        {
            Physics2D.IgnoreCollision(col1, col2, false);
        }
    }
    #endregion

    #region 플레이어 이동
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

    private void DashEnd()
    {
        isDashing = false;
        dashTime = 0f;
    }

    private void Movement()
    {
        if (isHiding)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

    private void FlipController()
    {
        if (rb.linearVelocity.x > 0 && !facingRight)
            Flip();
        else if (rb.linearVelocity.x < 0 && facingRight)
            Flip();
    }

    public void SetDashSpeed(float speed)
    {
        dashSpeed = speed;
    }
    #endregion
}