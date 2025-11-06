using UnityEngine;

public class CharacterMovementController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public void Move(Vector2 input)
    {
        moveDirection = new Vector3(input.x, 0f, input.y);
        animator.SetFloat("speed", moveDirection.magnitude);
        //Debug.Log(moveDirection.magnitude);
    }

    private void FixedUpdate()
    {
        if (moveDirection != Vector3.zero)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
}