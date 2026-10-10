
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFacing : MonoBehaviour
{
    public Transform opponent;
    public float rotationOffset = 0f;

    void Update()
    {
        Vector2 direction;

        if (opponent != null && opponent.gameObject.activeInHierarchy)
        {
            // Face the opponent during the fight.
            direction = opponent.position - transform.position;
        }
        else
        {
            // After winning, face whichever direction you're walking.
            if (Keyboard.current == null)
                return;

            direction = new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) -
                (Keyboard.current.aKey.isPressed ? 1 : 0),

                (Keyboard.current.wKey.isPressed ? 1 : 0) -
                (Keyboard.current.sKey.isPressed ? 1 : 0)
            );
        }

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f, 0f, angle + rotationOffset
        );
    }
}
