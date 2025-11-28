using Enemies.Visual;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies.Lod
{
    public class EnemyActivation : MonoBehaviour
    {
        public EnemyPoseFollower PoseFollower;
        public NavMeshAgent nav;
        public Collider[] colliders;

        private void SetActiveState(bool visible)
        {
            PoseFollower.enabled = visible;
            
            if (visible)
            {
                if (nav) nav.enabled = true;
                foreach (var c in colliders) c.enabled = true;
            }
            else
            {
                if (nav) nav.enabled = false;
                foreach (var c in colliders) c.enabled = false;
            }
        }

        void OnBecameVisible() => SetActiveState(true);
        void OnBecameInvisible() => SetActiveState(false);
    }
}