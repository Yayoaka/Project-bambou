using System.Collections.Generic;
using System.Linq;
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
        
        [SerializeField] private CombatTextEntity textEntityPrefab;
        
        private readonly List<CombatTextEntity> _textPool = new();
        
        public void DoDamageText(HealthEventData healthEventData)
        {
            var textEntity = GetTextEntity();
            
            textEntity.Init(healthEventData);
        }

        private CombatTextEntity GetTextEntity()
        {
            if (_textPool.FirstOrDefault(x => x.IsAvailable) is { } textEntity) return textEntity;

            return AddTextEntityToPool();
        }

        private CombatTextEntity AddTextEntityToPool()
        {
            var combatTextEntity = Instantiate(textEntityPrefab, transform);
            _textPool.Add(combatTextEntity);
            return combatTextEntity;
        }
    }
}
