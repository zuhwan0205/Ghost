using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float staminaMax = 100f;
    public float staminaDrain = 10f;
    public float staminaRegen = 5f;
    private float currentStamina;
    private bool isRunning;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentStamina = staminaMax;
        UIManager.Instance?.UpdateStaminaUI(); // 초기 UI 업데이트
    }

    void Update()
    {
        // 좌우 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        float speed = isRunning ? runSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // 스프라이트 방향 전환
        if (moveInput != 0)
            spriteRenderer.flipX = moveInput < 0;

        // 애니메이션
        anim.SetBool("isRunning", moveInput != 0 && !isRunning);
        anim.SetBool("isSprinting", isRunning);

        // 달리기
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && moveInput != 0)
        {
            isRunning = true;
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;
        }
        else
        {
            isRunning = false;
            if (currentStamina < staminaMax)
            {
                currentStamina += staminaRegen * Time.deltaTime;
                if (currentStamina > staminaMax) currentStamina = staminaMax;
            }
        }

        // UI 업데이트
        UIManager.Instance?.UpdateStaminaUI();
    }

    public float GetStaminaPercentage()
    {
        return currentStamina / staminaMax;
    }

    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public float GetStaminaMax()
    {
        return staminaMax;
    }
}