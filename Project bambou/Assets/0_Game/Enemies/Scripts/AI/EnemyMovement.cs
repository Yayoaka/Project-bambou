using UnityEngine;

namespace Enemies.AI
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float moveLerp = 12f;
        [SerializeField] private float rotationSpeed = 10f;

        public void Move(Vector3 desired, float dt)
        {
            var vel = rb.linearVelocity;
            var horizontal = new Vector3(desired.x, 0f, desired.z);

            var newVel = Vector3.Lerp(
                vel,
                new Vector3(horizontal.x, vel.y, horizontal.z),
                dt * moveLerp
            );

            rb.linearVelocity = newVel;
        }

        public void Rotate(Vector3 dir, float dt)
        {
            if (dir.sqrMagnitude < 0.01f)
                return;

            // Force agent to always look exactly where it moves
            var rot = Quaternion.LookRotation(dir.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rot,
                dt * rotationSpeed
            );
        }
    }
}