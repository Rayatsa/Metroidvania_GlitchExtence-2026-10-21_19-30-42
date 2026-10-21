using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    [Header("Fixed Movement Waypoints")]
    [SerializeField] private Transform[] flyWaypoints; // Titik-titik terbang di udara
    [SerializeField] private Transform groundPoint;    // Titik mendarat di bawah
    [SerializeField] private float moveSpeed = 5f;

    [Header("Attack Assets")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject explosionPrefab;

    [Header("Audio SFX")]
    [SerializeField] private AudioClip flySFX;
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip explosionSFX;
    [SerializeField] private AudioClip bossRoarSFX;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private AudioClip dieSFX;

    private AudioSource audioSource;
    private Animator anim;
    private bool isDead = false;
    private int currentWaypointIndex = 0;

    [Header("Facing Direction")]
    [SerializeField] private bool facingRight = false; // Set TRUE jika gambar awal sprite Bos menghadap KANAN

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        anim = GetComponent<Animator>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        currentHealth = maxHealth;
        StartCoroutine(BossPatternRoutine());
    }

    private void Update()
    {
        if (!isDead)
        {
            FacePlayer();
        }
    }

    private void FacePlayer()
    {
        if (PlayerController.instance == null) return;

        float playerX = PlayerController.instance.transform.position.x;

        // Player di sebelah kanan, tetapi Bos masih menghadap kiri
        if (playerX > transform.position.x && !facingRight)
        {
            Flip();
        }
        // Player di sebelah kiri, tetapi Bos masih menghadap kanan
        else if (playerX < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private IEnumerator BossPatternRoutine()
    {
        while (!isDead)
        {
            // 1. PINDAH KE TITIK TERBANG TERTENTU
            if (flyWaypoints != null && flyWaypoints.Length > 0)
            {
                Transform targetPoint = flyWaypoints[currentWaypointIndex];
                PlaySFX(flySFX);
                yield return MoveToPosition(targetPoint.position);

                currentWaypointIndex = (currentWaypointIndex + 1) % flyWaypoints.Length;
            }

            // 2. FASE TEMBAKAN RENTETAN (3 Kali Tembak)
            PlaySFX(bossRoarSFX);
            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 3; i++)
            {
                if (anim != null) anim.SetTrigger("Shoot");
                PlaySFX(shootSFX);

                if (projectilePrefab != null && firePoint != null)
                {
                    Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                }

                yield return new WaitForSeconds(1.2f);
            }

            // 3. FASE LEDAKAN (Hanya Dipanggil Sekali)
            yield return new WaitForSeconds(0.8f);
            if (anim != null) anim.SetTrigger("Attack");
            PlaySFX(bossRoarSFX);
            yield return new WaitForSeconds(0.3f);

            PlaySFX(explosionSFX);
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(2f);

            // 4. TURUN KE TITIK BAWAH (Ground Point)
            if (groundPoint != null)
            {
                PlaySFX(flySFX);
                yield return MoveToPosition(groundPoint.position);

                // 5. DIAM DI BAWAH (Memberi kesempatan pemain menyerang)
                yield return new WaitForSeconds(4f);
            }
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        PlaySFX(hitSFX);

        if (anim != null) anim.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        StopAllCoroutines();
        PlaySFX(dieSFX);

        if (anim != null) anim.SetTrigger("Die");
        if (StageManager.instance != null) StageManager.instance.EnemyKilled();

        Destroy(gameObject, 1.5f);

        if (PauseManager.instance != null)
        {
            PauseManager.instance.ShowCongrats();
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // --- DAMAGE SAAT BERSENTUHAN DENGAN PLAYER ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DealContactDamage(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DealContactDamage(collision.gameObject);
        }
    }

    private void DealContactDamage(GameObject playerObj)
    {
        // Hitung arah dorongan (knockback) menjauhi posisi Bos
        float directionX = playerObj.transform.position.x < transform.position.x ? -1f : 1f;
        Vector2 knockback = new Vector2(directionX * 6f, 4f);

        if (PlayerController.instance != null)
        {
            PlayerController.instance.TakeDamage(knockback);
        }
    }
}