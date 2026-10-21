using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Musuh terkena serangan! Sisa HP: {currentHealth}");

        // (Opsional) Tambahkan efek flash/kedip saat terkena damage di sini kelak

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Musuh hancur!");
        // Hancurkan objek musuh
        Destroy(gameObject);
    }
}