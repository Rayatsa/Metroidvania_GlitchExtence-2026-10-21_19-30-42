using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip monsterHitSFX;
    [SerializeField] private AudioClip monsterDieSFX;
    private AudioSource audioSource;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject slashEffectPrefab;
    private float nextAttackTime = 0f;

    private Rigidbody2D rb;
    private Animator anim;
    private float startX;
    private float leftBound;
    private float rightBound;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        currentHealth = maxHealth;
        startX = transform.position.x;
        leftBound = startX - patrolDistance;
        rightBound = startX + patrolDistance;
    }

    void Update()
    {
        if (PlayerController.instance != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.instance.transform.position);

            if (distanceToPlayer <= attackRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

                if (anim != null)
                {
                    anim.SetFloat("Speed", 0f);
                }

                FacePlayer();

                if (Time.time >= nextAttackTime)
                {
                    Attack();
                    nextAttackTime = Time.time + attackCooldown;
                }

                return;
            }
        }

        Patrol();
    }

    private void Patrol()
    {
        float speedX = movingRight ? moveSpeed : -moveSpeed;
        rb.linearVelocity = new Vector2(speedX, rb.linearVelocity.y);

        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(speedX));
        }

        if (movingRight && transform.position.x >= rightBound) Flip();
        else if (!movingRight && transform.position.x <= leftBound) Flip();
    }

    private void FacePlayer()
    {
        if (PlayerController.instance == null) return;

        float playerX = PlayerController.instance.transform.position.x;
        if (playerX > transform.position.x && !movingRight)
        {
            Flip();
        }
        else if (playerX < transform.position.x && movingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void Attack()
    {
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (slashEffectPrefab != null && attackPoint != null)
        {
            GameObject slash = Instantiate(slashEffectPrefab, attackPoint.position, Quaternion.identity);

            Vector3 slashScale = slash.transform.localScale;
            slashScale.x = Mathf.Abs(slashScale.x) * (transform.localScale.x < 0 ? -1f : 1f);
            slash.transform.localScale = slashScale;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Jalankan animasi terkena tembakan
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        // Putar efek suara hit
        if (monsterHitSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(monsterHitSFX);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (monsterDieSFX != null)
        {
            AudioSource.PlayClipAtPoint(monsterDieSFX, transform.position);
        }

        if (StageManager.instance != null)
        {
            StageManager.instance.EnemyKilled();
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float currentX = Application.isPlaying ? startX : transform.position.x;
        Gizmos.DrawLine(new Vector3(currentX - patrolDistance, transform.position.y, 0),
                        new Vector3(currentX + patrolDistance, transform.position.y, 0));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}