using UnityEngine;

public class BossExplosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float maxScale = 15f;
    [SerializeField] private float expandSpeed = 10f;
    [SerializeField] private float lifetime = 1.5f; // Ledakan otomatis hancur total dalam 1.5 detik
    [SerializeField] private LayerMask wallLayer;
    private bool hasHitPlayer = false;

    private void Start()
    {
        // Menjamin objek ledakan langsung hancur otomatis setelah durasi lifetime selesai
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Membesarkan skala ledakan hingga batas maxScale
        if (transform.localScale.x < maxScale)
        {
            transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!hasHitPlayer && collision.CompareTag("Player"))
        {
            Vector2 directionToPlayer = (collision.transform.position - transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(transform.position, collision.transform.position);

            // Cek apakah ada objek ber-layer Wall yang menghalangi
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, wallLayer);

            if (hit.collider == null)
            {
                hasHitPlayer = true;
                Vector2 knockback = directionToPlayer * 7f + Vector2.up * 3f;
                if (PlayerController.instance != null)
                {
                    PlayerController.instance.TakeDamage(knockback);
                }
            }
        }
    }
}