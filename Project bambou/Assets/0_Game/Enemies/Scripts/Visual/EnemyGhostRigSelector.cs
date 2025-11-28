using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyGhostRigSelector : MonoBehaviour
    {
        private EnemyPoseFollower follower;

        void Start()
        {
            follower = GetComponent<EnemyPoseFollower>();

            var ghost = EnemyGhostRigSystem.Instance.GetGhostRigs[Random.Range(0, EnemyGhostRigSystem.Instance.GetGhostRigs.Length)];
            follower.SetGhostRig(ghost);
        }
    }
}