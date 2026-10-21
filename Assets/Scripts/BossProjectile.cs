using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float destroyTime = 5f;
    private Transform playerTransform;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, destroyTime);

        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
        }
    }

    private void Update()
    {
        if (playerTransform != null)
        {
            // Bergerak mengejar arah player
            Vector2 direction = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;

            // Memutar rotasi peluru menghadap player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector2 knockback = (collision.transform.position - transform.position).normalized * 5f;
            if (PlayerController.instance != null)
            {
                PlayerController.instance.TakeDamage(knockback);
            }
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground"))
        {
            // Peluru hancur saat menabrak dinding pelindung
            Destroy(gameObject);
        }
    }
}