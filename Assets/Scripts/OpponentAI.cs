
using UnityEngine;
using System.Collections;

public class OpponentAI : MonoBehaviour
{
    public Transform player;

    public GameObject aiLeftGlove;
    public GameObject aiRightGlove;

    public float moveSpeed = 2.5f;
    public float stoppingDistance = 1.5f;
    public float attackCooldown = 1.2f;

    private Rigidbody2D rb;
    private float nextAttackTime;
    private Coroutine punchAnimation;

    private Vector3 leftStart;
    private Vector3 rightStart;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (aiLeftGlove != null)
        {
            leftStart = aiLeftGlove.transform.localPosition;
            aiLeftGlove.SetActive(true);
        }

        if (aiRightGlove != null)
        {
            rightStart = aiRightGlove.transform.localPosition;
            aiRightGlove.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = player.position - transform.position;

        if (direction.magnitude > stoppingDistance)
            rb.linearVelocity = direction.normalized * moveSpeed;
        else
            rb.linearVelocity = Vector2.zero;
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
            return;

        if (Time.time < nextAttackTime)
            return;

        int punchType = Random.Range(0, 4);

        switch (punchType)
        {
            case 0:
                TryPunch("JAB", 2f, 3, aiLeftGlove);
                break;

            case 1:
                TryPunch("CROSS", 2f, 5, aiRightGlove);
                break;

            case 2:
                TryPunch("HOOK", 1.5f, 7, aiLeftGlove);
                break;

            case 3:
                TryPunch("UPPERCUT", 2f, 10, aiRightGlove);
                break;
        }
    }

    void TryPunch(string punchName, float range, int damage,
                  GameObject gloveObject)
    {
        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        if (distance > range)
            return;

        nextAttackTime = Time.time + attackCooldown;

        if (punchAnimation != null)
        {
            StopCoroutine(punchAnimation);
            ResetGloves();
        }

        if (gloveObject != null)
            punchAnimation = StartCoroutine(
                AnimatePunch(gloveObject)
            );

        Debug.Log("AI used " + punchName + "!");

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
            health.TakeDamage(damage);
    }

    IEnumerator AnimatePunch(GameObject gloveObject)
    {
        Transform glove = gloveObject.transform;

        Vector3 start = gloveObject == aiLeftGlove
            ? leftStart
            : rightStart;

        Vector3 worldDirection =
            player.position - transform.position;

        Vector3 localDirection = Vector3.right;

        if (worldDirection.sqrMagnitude > 0.001f)
        {
            localDirection =
                transform.InverseTransformDirection(
                    worldDirection.normalized
                );
        }

        Vector3 end = start + localDirection * 0.65f;

        float duration = 0.25f;
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
        if (aiLeftGlove != null)
            aiLeftGlove.transform.localPosition = leftStart;

        if (aiRightGlove != null)
            aiRightGlove.transform.localPosition = rightStart;
    }

    void OnDisable()
    {
        if (punchAnimation != null)
        {
            StopCoroutine(punchAnimation);
            punchAnimation = null;
        }

        ResetGloves();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
