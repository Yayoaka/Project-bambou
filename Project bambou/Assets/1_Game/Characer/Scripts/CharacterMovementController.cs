using UnityEngine;
using Unity.Netcode;

public class CharacterMovementController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    private Vector3 moveDirection;

    public void Move(Vector2 input)
    {
        if (!CanControl()) return;

        moveDirection.Set(input.x, 0f, input.y);

        Debug.unityLogger.Log("mouvement :" + moveDirection);
        animator?.SetFloat("speed", input.magnitude);
    }

    private void FixedUpdate()
    {
        if (!CanControl()) return;

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
            rb.MoveRotation(Quaternion.LookRotation(moveDirection));
        }
        else
        {
            animator?.SetFloat("speed", 0f);
        }
    }

    private bool CanControl()
    {
        return IsOwner && rb != null;
    }
}