
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthBar;

    [Header("Dodge")]
    public float dodgeDuration = 0.5f;
    public float dodgeCooldown = 1.5f;
    public Color dodgeColor = Color.cyan;
    public AudioClip dodgeSound;

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.15f;
    public Color hitColor = Color.red;

    private int currentHealth;
    public int CurrentHealth => currentHealth;

    private float dodgeEndTime;
    private float nextDodgeTime;
    private float hitFlashEndTime;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            Time.time >= nextDodgeTime)
        {
            dodgeEndTime = Time.time + dodgeDuration;
            nextDodgeTime = Time.time + dodgeCooldown;

            if (audioSource != null && dodgeSound != null)
                audioSource.PlayOneShot(dodgeSound);
        }

        if (spriteRenderer != null)
        {
            if (Time.time < hitFlashEndTime)
                spriteRenderer.color = hitColor;
            else if (Time.time < dodgeEndTime)
                spriteRenderer.color = dodgeColor;
            else
                spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        if (Time.time < dodgeEndTime)
        {
            Debug.Log("PLAYER DODGED!");
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        hitFlashEndTime = Time.time + hitFlashDuration;

        Debug.Log("Player HP: " + currentHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (currentHealth <= 0)
        {
            Debug.Log("KNOCKOUT! YOU LOSE!");
            gameObject.SetActive(false);
        }
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;

        dodgeEndTime = 0f;
        nextDodgeTime = 0f;
        hitFlashEndTime = 0f;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
}
