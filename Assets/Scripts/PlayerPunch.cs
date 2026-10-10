
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerPunch : MonoBehaviour
{
    public Transform opponent;

    public GameObject leftGlove;
    public GameObject rightGlove;

    [Header("Punch Sounds")]
    public AudioClip jabSound;
    public AudioClip crossSound;
    public AudioClip hookSound;
    public AudioClip uppercutSound;

    private AudioSource audioSource;

    private float nextPunchTime;
    private Coroutine punchAnimation;

    private Vector3 leftStart;
    private Vector3 rightStart;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (leftGlove != null)
        {
            leftStart = leftGlove.transform.localPosition;
            leftGlove.SetActive(true);
        }

        if (rightGlove != null)
        {
            rightStart = rightGlove.transform.localPosition;
            rightGlove.SetActive(true);
        }
    }

    public void SetOpponent(Transform newOpponent)
    {
        opponent = newOpponent;
        nextPunchTime = 0f;
    }

    void Update()
    {
        if (Keyboard.current == null || Time.time < nextPunchTime)
            return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            Punch("JAB", 2f, 0.3f, 3, leftGlove, jabSound);

        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            Punch("CROSS", 2f, 0.5f, 5, rightGlove, crossSound);

        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            Punch("HOOK", 1.5f, 0.7f, 7, leftGlove, hookSound);

        else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            Punch("UPPERCUT", 2f, 0.8f, 10, rightGlove, uppercutSound);
    }

    void Punch(string punchName, float range, float cooldown,
               int damage, GameObject gloveObject, AudioClip sound)
    {
        nextPunchTime = Time.time + cooldown;

        if (audioSource != null && sound != null)
            audioSource.PlayOneShot(sound);

        if (punchAnimation != null)
        {
            StopCoroutine(punchAnimation);
            ResetGloves();
        }

        if (gloveObject != null)
            punchAnimation = StartCoroutine(
                AnimatePunch(punchName, gloveObject)
            );

        Debug.Log("PLAYER THREW " + punchName + "!");

        if (opponent == null || !opponent.gameObject.activeInHierarchy)
            return;

        float distance = Vector2.Distance(
            transform.position,
            opponent.position
        );

        if (distance <= range)
        {
            OpponentHealth health =
                opponent.GetComponent<OpponentHealth>();

            if (health != null)
                health.TakeDamage(damage);
        }
    }

    IEnumerator AnimatePunch(string punchName, GameObject gloveObject)
    {
        Transform glove = gloveObject.transform;

        Vector3 start = gloveObject == leftGlove
            ? leftStart
            : rightStart;

        Vector3 direction = Vector3.right;

        if (opponent != null)
        {
            Vector3 worldDirection =
                opponent.position - transform.position;

            if (worldDirection.sqrMagnitude > 0.001f)
                direction = transform.InverseTransformDirection(
                    worldDirection.normalized
                );
        }

        float distance = punchName == "JAB" ? 0.8f : 1.0f;
        float duration = punchName == "JAB" ? 0.18f : 0.25f;

        Vector3 end = start + direction * distance;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float extension = Mathf.Sin(progress * Mathf.PI);

            glove.localPosition =
                Vector3.Lerp(start, end, extension);

            yield return null;
        }

        glove.localPosition = start;
        punchAnimation = null;
    }

    void ResetGloves()
    {
        if (leftGlove != null)
            leftGlove.transform.localPosition = leftStart;

        if (rightGlove != null)
            rightGlove.transform.localPosition = rightStart;
    }

    void OnDisable()
    {
        if (punchAnimation != null)
        {
            StopCoroutine(punchAnimation);
            punchAnimation = null;
        }

        ResetGloves();
    }
}
