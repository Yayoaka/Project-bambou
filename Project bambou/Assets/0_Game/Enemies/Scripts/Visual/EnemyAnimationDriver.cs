using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyAnimationDriver : MonoBehaviour
    {
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private GameObject rigRoot; // Root of the animated rig (the object that had the Animator)

        private float _time;

        void Awake()
        {
            // Make sure Animator is disabled if it exists
            var animator = rigRoot != null ? rigRoot.GetComponent<Animator>() : GetComponentInChildren<Animator>();
            if (animator != null)
                animator.enabled = false;
        }

        public void PlayWalk(float speed, float dt)
        {
            if (walkClip == null || rigRoot == null)
                return;

            // No movement => keep current pose (fake idle)
            if (speed < 0.05f)
                return;

            // Advance time based on movement speed
            _time += dt * speed;

            // Loop inside clip length
            var length = walkClip.length;
            if (length > 0f)
                _time = Mathf.Repeat(_time, length);

            // Sample the pose on the rig root
            walkClip.SampleAnimation(rigRoot, _time);
        }
    }
}