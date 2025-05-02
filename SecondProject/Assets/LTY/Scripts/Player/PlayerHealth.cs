using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 1;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance?.UpdateHealthUI(); // �ʱ� UI ������Ʈ
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            currentHealth--;
            UIManager.Instance?.UpdateHealthUI(); // ü�� ���� �� UI ������Ʈ
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