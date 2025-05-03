using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class Player : PlayerManager
{
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
    [SerializeField] private bool isDashing;

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

    private float holdTimer = 0f;
    private bool isHolding = false;
    private Interactable currentInteractable;
    [SerializeField] private GameObject interactionHint;
    [SerializeField] private Slider holdProgressBar;
    private float xInput;
    public bool DidMoveOrInteract { get; private set; } = false;

    private MiniGameManager miniGameManager;

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

    protected override void Start()
    {
        base.Start();
        originalMoveSpeed = moveSpeed;
        originalDashSpeed = dashSpeed;

        miniGameManager = FindObjectOfType<MiniGameManager>();
        if (miniGameManager == null)
            Debug.LogError("MiniGameManager를 찾을 수 없습니다!");

        if (healthBar != null)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = Player_Hp;
        }
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStemina;
            staminaBar.value = Stemina;
        }
        if (holdProgressBar == null || interactionHint == null)
            Debug.LogError("HoldProgressBar 또는 InteractionHint가 할당되지 않았습니다!");

        for (int i = 0; i < inventorySlots.Length; i++)
        {
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
            if (borderSlots[i] != null)
            {
                borderSlots[i].gameObject.SetActive(false);
            }
        }

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
            GameManager.Instance.GameOver();
        }
    }

    private void CheckInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (xInput != 0)
        {
            if (IsVendingMachineOpen())
            {
                HideVendingMachinePanel();
                return;
            }
            if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
            {
                PhoneManager.Instance.TogglePhoneScreen();
                return;
            }
        }

        DidMoveOrInteract = (xInput != 0f || Input.GetKeyDown(KeyCode.E));

        if (Input.GetKey(KeyCode.LeftShift))
            DashAbility();
        if (Input.GetKeyUp(KeyCode.LeftShift))
            DashEnd();

        if (Input.GetKeyDown(KeyCode.Space))
            TakeDamage(1f);
    }

    private void CheckHoldInteraction()
    {
        if (miniGameManager.IsAnyMiniGameActive())
        {
            interactionHint.SetActive(false);
            if (Input.GetKeyDown(KeyCode.E))
            {
                miniGameManager.CancelAllGames();
            }
            return;
        }

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

    private void ResetHold()
    {
        holdTimer = 0f;
        isHolding = false;
        holdProgressBar.gameObject.SetActive(false);
    }

    private void CheckMiniGameCancellation()
    {
        if (!miniGameManager.IsAnyMiniGameActive())
            return;

        if (xInput != 0)
        {
            miniGameManager.CancelAllGames();
        }
    }

    private void CheckInventoryInput()
    {
        if (miniGameManager.IsAnyMiniGameActive())
            return;

        if (IsVendingMachineOpen())
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0f || Input.GetMouseButtonDown(1))
            {
                HideVendingMachinePanel();
                return;
            }
        }

        if (PhoneManager.Instance != null && PhoneManager.Instance.IsPhoneOpen)
        {
            float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
            if (scroll != 0f || Input.GetMouseButtonDown(1))
            {
                PhoneManager.Instance.TogglePhoneScreen();
                return;
            }
        }

        float scrollInput = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scrollInput > 0f && selectedSlot < 2)
        {
            selectedSlot++;
        }
        else if (scrollInput < 0f && selectedSlot > 0)
        {
            selectedSlot--;
        }

        for (int i = 0; i < borderSlots.Length; i++)
        {
            borderSlots[i].gameObject.SetActive(i == selectedSlot);
        }

        if (Input.GetMouseButtonDown(1) && inventory[selectedSlot] != null)
        {
            inventory[selectedSlot].Use(this);
            inventory[selectedSlot] = null;
            itemImages[selectedSlot].sprite = null;
            itemImages[selectedSlot].gameObject.SetActive(false);
        }
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = item;
                itemImages[i].sprite = item.icon;
                itemImages[i].gameObject.SetActive(true);
                return true;
            }
        }
        Debug.Log("인벤토리가 가득 찼습니다!");
        return false;
    }

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
        if (dashTime > 0)
            rb.linearVelocity = new Vector2(xInput * (moveSpeed + dashSpeed), rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
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
}