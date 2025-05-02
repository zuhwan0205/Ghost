using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealthUI(); // 초기 UI 업데이트
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            currentHealth--;
            UIManager.Instance?.UpdateHealthUI(); // 체력 변경 시 UI 업데이트
            if (currentHealth <= 0)
                GameManager.Instance.GameOver();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}