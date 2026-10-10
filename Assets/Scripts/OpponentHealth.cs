
using UnityEngine;
using UnityEngine.UI;

public class OpponentHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthBar;

    [Header("AI Dodge")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.25f;
    public float dodgeCooldown = 2f;
    public float dodgeDuration = 0.3f;
    public Color dodgeColor = Color.cyan;

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.15f;
    public Color hitColor = Color.red;

    private int currentHealth;
    public int CurrentHealth => currentHealth;

    private float nextDodgeTime;
    private float dodgeFlashEndTime;
    private float hitFlashEndTime;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        if (spriteRenderer == null)
            return;

        if (Time.time < hitFlashEndTime)
            spriteRenderer.color = hitColor;
        else if (Time.time < dodgeFlashEndTime)
            spriteRenderer.color = dodgeColor;
        else
            spriteRenderer.color = originalColor;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        // AI has a chance to dodge incoming punches.
        if (Time.time >= nextDodgeTime &&
            Random.value < dodgeChance)
        {
            nextDodgeTime = Time.time + dodgeCooldown;
            dodgeFlashEndTime = Time.time + dodgeDuration;

            Debug.Log("OPPONENT DODGED!");
            return;
        }

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        hitFlashEndTime = Time.time + hitFlashDuration;

        if (healthBar != null)
            healthBar.value = currentHealth;

        Debug.Log("Opponent HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("KNOCKOUT! YOU WIN!");
            gameObject.SetActive(false);
        }
    }
    public void ResetHealth()
    {
        currentHealth = maxHealth;

        nextDodgeTime = 0f;
        dodgeFlashEndTime = 0f;
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
