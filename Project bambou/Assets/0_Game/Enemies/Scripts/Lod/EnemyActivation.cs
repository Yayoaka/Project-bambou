using Enemies.Visual;
using Entity;
using UnityEngine;
using UnityEngine.AI;

namespace Enemies.Lod
{
    public class EnemyActivation : EntityComponent<EnemyBehaviour>
    {
        //TODO not working anymore but no time yet
        private EnemyPoseFollower _poseFollower;
        private NavMeshAgent _nav;
        private Collider _collider;

        public override void Init(EnemyBehaviour owner)
        {
            base.Init(owner);
            
            _poseFollower = GetComponentInChildren<EnemyPoseFollower>();
            _nav = GetComponent<NavMeshAgent>();
            _collider = GetComponent<Collider>();
        }

        public void SetActiveState(bool visible)
        {
            _poseFollower.enabled = visible;
            
            if (visible)
            {
                if (_nav) _nav.enabled = true;
                if (_collider) _collider.enabled = true;
            }
            else
            {
                if (_nav) _nav.enabled = false;
                if (_collider) _collider.enabled = false;
            }
        }

        void OnBecameVisible() => SetActiveState(true);
        void OnBecameInvisible() => SetActiveState(false);
    }
}