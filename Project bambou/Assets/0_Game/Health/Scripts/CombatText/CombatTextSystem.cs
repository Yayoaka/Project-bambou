using System.Collections.Generic;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace Health.CombatText
{
    public class CombatTextSystem : MonoBehaviour
    {
        #region Singleton

        private static CombatTextSystem _instance;
        
        public static CombatTextSystem Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        #endregion
        
        [SerializeField] private NetworkObject textEntityPrefab;
        
        private readonly List<CombatTextEntity> _textPool = new();
        
        public void DoText(HealthEventData healthEventData)
        {
            var textEntity = NetworkObjectPool.Instance.Get(textEntityPrefab);
            
            var text = textEntity.GetComponent<CombatTextEntity>();
            
            textEntity.Spawn();
            
            text.Init(healthEventData);
        }
    }
}
