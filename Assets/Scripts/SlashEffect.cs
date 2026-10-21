using UnityEngine;

public class SlashEffect : MonoBehaviour
{
    [SerializeField] private float destroyDelay = 0.3f; // Lama efek slash muncul sebelum hancur

    private void Start()
    {
        // Hapus efek slash otomatis setelah animasi selesai
        Destroy(gameObject, destroyDelay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Jika tebasan kena Player
        if (collision.CompareTag("Player"))
        {
            float directionX = collision.transform.position.x < transform.position.x ? -1f : 1f;
            Vector2 knockback = new Vector2(directionX * 6f, 4f);

            if (PlayerController.instance != null)
            {
                PlayerController.instance.TakeDamage(knockback);
            }
        }
    }
}