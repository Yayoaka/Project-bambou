using System;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterMovementController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

        [SerializeField] private Rigidbody rb;
        private Vector3 moveDirection;

        public void Move(Vector2 input)
        {
            moveDirection.Set(input.x, 0f, input.y);
        }

        private void FixedUpdate()
        {
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
                rb.MoveRotation(Quaternion.LookRotation(moveDirection));
            }
        }
    }
}