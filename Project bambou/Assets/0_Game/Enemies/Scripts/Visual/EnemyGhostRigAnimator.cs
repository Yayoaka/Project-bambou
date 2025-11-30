using UnityEngine;

namespace Enemies.Visual
{
    public class GhostRigAnimator : MonoBehaviour
    {
        [SerializeField] private AnimationClip walkClip;
        [SerializeField] private GameObject rigRoot;

        private float _time;

        void Start()
        {
            // random offset for variety
            _time = Random.Range(0f, walkClip.length);

            // Disable Animator if present
            var anim = rigRoot.GetComponent<Animator>();
            if (anim != null) anim.enabled = false;
        }

        void Update()
        {
            var dt = Time.deltaTime;

            // Fixed walk speed since ghosts are not moving
            _time = Mathf.Repeat(_time + dt * 1.0f, walkClip.length);

            walkClip.SampleAnimation(rigRoot, _time);
        }
    }
}