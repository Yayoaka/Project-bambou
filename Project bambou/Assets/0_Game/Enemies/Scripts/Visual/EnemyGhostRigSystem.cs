using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyGhostRigSystem : MonoBehaviour
    {
        [SerializeField] private Transform[] ghostRigAnimators;
        
        #region Singleton

        public static EnemyGhostRigSystem Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        
        #endregion

        public Transform[] GetGhostRigs => ghostRigAnimators;
    }
}
