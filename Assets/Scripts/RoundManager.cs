
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class RoundManager : MonoBehaviour
{
    public TextMeshProUGUI roundTimerText;
    public PlayerHealth playerHealth;
    public OpponentHealth opponentHealth;
    public Transform blueCorner;
    public Transform redCorner;

    [Header("Three Opponents")]
    public OpponentHealth opponent2;
    public OpponentHealth opponent3;

    public int currentRound = 1;
    public float roundDuration = 20f;
    public float breakDuration = 5f;

    [Header("Boxing Bell")]
    public AudioClip bellSound;

    private AudioSource audioSource;
    private OpponentHealth[] opponents;
    private int opponentIndex = 0;

    private float timeRemaining;
    private bool onBreak = false;
    private bool betweenOpponents = false;
    private bool matchOver = false;

    void Start()
    {
        opponents = new OpponentHealth[]
        {
            opponentHealth, opponent2, opponent3
        };

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (opponent2 != null)
            opponent2.gameObject.SetActive(false);

        if (opponent3 != null)
            opponent3.gameObject.SetActive(false);

        timeRemaining = roundDuration;
        UpdatePlayerTarget();
        RingBell();
    }

    void RingBell()
    {
        if (audioSource != null && bellSound != null)
            audioSource.PlayOneShot(bellSound);
    }

    void UpdatePlayerTarget()
    {
        PlayerPunch punch = playerHealth.GetComponent<PlayerPunch>();
        PlayerFacing facing = playerHealth.GetComponent<PlayerFacing>();

        if (punch != null)
            punch.SetOpponent(opponentHealth.transform);

        if (facing != null)
            facing.opponent = opponentHealth.transform;
    }

    void Update()
    {
        if (matchOver)
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().name
                );
            }
            return;
        }

        if (playerHealth.CurrentHealth <= 0)
        {
            EndMatch("YOU LOSE!");
            return;
        }

        if (betweenOpponents)
        {
            timeRemaining -= Time.deltaTime;

            roundTimerText.text =
                "OPPONENT " + (opponentIndex + 2) +
                " - GET READY! | " +
                Mathf.CeilToInt(Mathf.Max(0f, timeRemaining)) + "s";

            if (timeRemaining <= 0f)
                StartNextOpponent();

            return;
        }

        if (opponentHealth.CurrentHealth <= 0)
        {
            WinCurrentMatch();
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (onBreak)
        {
            roundTimerText.text =
                "FIGHT " + (opponentIndex + 1) +
                " | BREAK " +
                Mathf.CeilToInt(Mathf.Max(0f, timeRemaining)) + "s";

            if (timeRemaining <= 0f)
            {
                onBreak = false;
                currentRound++;
                timeRemaining = roundDuration;

                SetCombatEnabled(true);
                RingBell();
            }
        }
        else
        {
            roundTimerText.text =
                "FIGHT " + (opponentIndex + 1) +
                " | ROUND " + currentRound +
                " | " +
                Mathf.CeilToInt(Mathf.Max(0f, timeRemaining)) + "s";

            if (timeRemaining <= 0f)
            {
                RingBell();

                if (currentRound >= 3)
                {
                    DecideWinner();
                }
                else
                {
                    onBreak = true;
                    timeRemaining = breakDuration;
                    SetCombatEnabled(false);
                }
            }
        }
    }

    void WinCurrentMatch()
    {
        if (opponentIndex >= opponents.Length - 1)
        {
            EndMatch("YOU ARE THE CHAMPION!");
            return;
        }

        // Stop combat and hide the defeated opponent.
        SetCombatEnabled(false);
        opponentHealth.gameObject.SetActive(false);

        // Reset the player while waiting.
        playerHealth.ResetHealth();
        MoveToCorner(playerHealth.transform, blueCorner);

        // Begin the between-opponent countdown.
        betweenOpponents = true;
        onBreak = false;
        timeRemaining = breakDuration;

        RingBell();

        Debug.Log("OPPONENT " + (opponentIndex + 1) + " DEFEATED!");
    }

    void StartNextOpponent()
    {
        opponentIndex++;
        opponentHealth = opponents[opponentIndex];

        // Activate and reset the new opponent.
        opponentHealth.gameObject.SetActive(true);
        opponentHealth.ResetHealth();

        MoveToCorner(playerHealth.transform, blueCorner);
        MoveToCorner(opponentHealth.transform, redCorner);

        UpdatePlayerTarget();

        currentRound = 1;
        timeRemaining = roundDuration;
        betweenOpponents = false;
        onBreak = false;

        SetCombatEnabled(true);
        RingBell();

        Debug.Log("FIGHT " + (opponentIndex + 1) + " START!");
    }

    void MoveToCorner(Transform fighter, Transform corner)
    {
        if (fighter == null || corner == null)
            return;

        Rigidbody2D rb = fighter.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.position = corner.position;
        }
        else
        {
            fighter.position = corner.position;
        }
    }

    void SetCombatEnabled(bool canFight)
    {
        PlayerMovement movement =
            playerHealth.GetComponent<PlayerMovement>();

        PlayerPunch punch =
            playerHealth.GetComponent<PlayerPunch>();

        OpponentAI ai =
            opponentHealth.GetComponent<OpponentAI>();

        if (movement != null)
            movement.enabled = canFight;

        if (punch != null)
            punch.enabled = canFight;

        if (ai != null)
            ai.enabled = canFight;

        if (!canFight)
        {
            MoveToCorner(playerHealth.transform, blueCorner);
            MoveToCorner(opponentHealth.transform, redCorner);
        }
    }

    void DecideWinner()
    {
        if (playerHealth.CurrentHealth >
            opponentHealth.CurrentHealth)
        {
            WinCurrentMatch();
        }
        else if (playerHealth.CurrentHealth <
                 opponentHealth.CurrentHealth)
        {
            EndMatch("YOU LOSE!");
        }
        else
        {
            EndMatch("DRAW!");
        }
    }

    void EndMatch(string result)
    {
        matchOver = true;
        SetCombatEnabled(false);

        roundTimerText.text =
            result + "\nPRESS R TO REPLAY";

        Debug.Log(result);

        if (timeRemaining > 0f)
            RingBell();
    }
}
