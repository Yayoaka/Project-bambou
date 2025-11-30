using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyPoseFollower : MonoBehaviour
    {
        [SerializeField] private Transform[] myBones;
        [SerializeField] private Transform[] ghostBones;

        public void SetGhostRig(Transform ghostRoot)
        {
            // Build bone mapping
            ghostBones = ghostRoot.GetComponentsInChildren<Transform>();
            myBones = GetComponentsInChildren<Transform>();
        }

        void LateUpdate()
        {
            // Copy only rotation (and optional local position)
            for (var i = 0; i < myBones.Length; i++)
            {
                myBones[i].localRotation = ghostBones[i].localRotation;
                myBones[i].localPosition = ghostBones[i].localPosition; // optional
            }
        }
    }
}