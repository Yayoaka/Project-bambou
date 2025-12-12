using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyGhostRigSelector : MonoBehaviour
    {
        private EnemyPoseFollower follower;
        [SerializeField] private string enemyID;

        void Start()
        {
            follower = GetComponent<EnemyPoseFollower>();

            var ghost = EnemyGhostRigSystem.Instance.GetGhostRigs[enemyID][Random.Range(0, EnemyGhostRigSystem.Instance.GetGhostRigs[enemyID].Length)];
            follower.SetGhostRig(ghost);
        }
    }
}