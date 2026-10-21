using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;

    [Header("Audio Clips (SFX)")]
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip jumpSFX;
    [SerializeField] private AudioClip landSFX;
    [SerializeField] private AudioClip hurtSFX; // SFX saat Player terkena hit / damage
    [SerializeField] private AudioClip dieSFX;
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private float footstepInterval = 0.35f;
    private float footstepTimer;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;
    private bool isDead = false;

    [Header("Weapon Status")]
    public bool hasGun = false;
    private bool isHurt = false;

    [Header("Invincibility Settings")]
    [SerializeField] private float invincibilityDuration = 1f;
    private bool isInvincible = false;

    [Header("Shooting Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.35f; // Jeda waktu antar tembakan (detik)
    private float nextFireTime = 0f;                   // Penanda kapan player boleh menembak lagi

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    private float xAxis;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    private bool isGrounded;
    private bool wasGroundedLastFrame;

    [Header("Jump Settings (Variable Jump)")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

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
    }

    void Update()
    {
        if (isDead) return;

        wasGroundedLastFrame = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);

        if (!wasGroundedLastFrame && isGrounded)
        {
            PlaySFX(landSFX);
        }

        GetInputs();
        HandleFootsteps();
        JumpPhysics();
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    void GetInputs()
    {
        xAxis = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                xAxis = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                xAxis = 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                Jump();
            }

            // Input menembak (bisa klik cepat atau tahan tombol)
            bool isShootingInput = (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
                                   Keyboard.current.jKey.isPressed;

            // Tembakan hanya diproses jika waktu game sudah melewati nextFireTime
            if (isShootingInput && hasGun && !isHurt && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate; // Setel batas waktu tembakan berikutnya
                Shoot();
            }
        }
    }

    private void Move()
    {
        if (isHurt || isDead) return;

        rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);

        if (xAxis != 0)
        {
            Vector3 localScale = transform.localScale;
            localScale.x = Mathf.Abs(localScale.x) * Mathf.Sign(xAxis);
            transform.localScale = localScale;
        }
    }

    private void HandleFootsteps()
    {
        if (isGrounded && xAxis != 0 && !isHurt)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlaySFX(walkSFX);
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        PlaySFX(jumpSFX);
    }

    private void JumpPhysics()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && Keyboard.current != null && !Keyboard.current.spaceKey.isPressed)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    private void Shoot()
    {
        if (firePoint != null && bulletPrefab != null)
        {
            float angle = transform.localScale.x < 0 ? 180f : 0f;
            Quaternion bulletRotation = Quaternion.Euler(0f, 0f, angle);

            Instantiate(bulletPrefab, firePoint.position, bulletRotation);
            PlaySFX(shootSFX);

            if (anim != null)
            {
                anim.SetTrigger("Shoot");
            }
        }
    }

    public void TakeDamage(Vector2 knockbackDir)
    {
        if (!isHurt && !isInvincible && !isDead)
        {
            currentHealth--;
            Debug.Log("Nyawa Player: " + currentHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(HurtRoutine(knockbackDir));
            }
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        PlaySFX(dieSFX);

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        Debug.Log("Player telah Mati!");
        
        if (PauseManager.instance != null)
        {
            PauseManager.instance.ShowGameOver();
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator HurtRoutine(Vector2 knockbackDir)
    {
        isHurt = true;
        isInvincible = true;
        rb.linearVelocity = knockbackDir;

        PlaySFX(hurtSFX); // Memutar suara terkena hit

        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(0.3f);
        isHurt = false;

        float remainingInvincibility = invincibilityDuration - 0.3f;
        if (remainingInvincibility > 0)
        {
            yield return new WaitForSeconds(remainingInvincibility);
        }

        isInvincible = false;
    }

    private void UpdateAnimatorParameters()
    {
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(xAxis));
            anim.SetBool("isGrounded", isGrounded);
            anim.SetBool("hasGun", hasGun);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("GunPickup"))
        {
            hasGun = true;
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            float directionX = transform.position.x < collision.transform.position.x ? -1f : 1f;
            Vector2 knockback = new Vector2(directionX * 6f, 4f);

            TakeDamage(knockback);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    
}