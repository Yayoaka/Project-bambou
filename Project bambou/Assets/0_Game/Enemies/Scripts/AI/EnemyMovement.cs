using UnityEngine;

namespace Enemies.AI
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float rotationSpeed = 10f;

        public void Move(Vector3 desired)
        {
            var current = rb.linearVelocity;
            var newVel = new Vector3(desired.x, current.y, desired.z);

            rb.linearVelocity = Vector3.Lerp(new Vector3(current.x, current.y, current.z), newVel, Time.deltaTime * 8f);
        }

        public void Rotate(Vector3 dir)
        {
            var rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * rotationSpeed);
        }
    }
}