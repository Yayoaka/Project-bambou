using System;
using Entity;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterMovementController : EntityComponent<CharacterBehaviour>
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
                var newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
            }
        }
        
        public void RotateToMouse(Vector3 worldMousePos)
        {
            if (!IsOwner) return;
            
            var direction = worldMousePos - rb.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                var rot = Quaternion.LookRotation(direction);
                rb.MoveRotation(rot);
            }
        }
    }
}