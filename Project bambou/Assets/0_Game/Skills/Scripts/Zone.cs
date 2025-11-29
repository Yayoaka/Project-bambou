using System.Collections;
using System.Collections.Generic;
using Health;
using Interfaces;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Skills
{
    public class Zone : NetworkBehaviour
    {
        private EffectData _data;
        private IStatsEntity _sourceStats;
        private IAffectable _sourceEntity;
        
        private readonly HashSet<IAffectable> _targetsInside = new ();
        
        public void Init(EffectData data, IStatsEntity sourceStats, IAffectable sourceEntity)
        {
            if (!IsServer) return;
            
            _data = data;
            _sourceStats = sourceStats;
            _sourceEntity = sourceEntity;
            
            StartCoroutine(DamageCoroutine());
            StartCoroutine(DestroyCoroutine());
        }
        
        private IEnumerator DestroyCoroutine()
        {
            yield return new WaitForSeconds(_data.duration);
            StopAllCoroutines();
            GetComponent<NetworkObject>().Despawn();
        }
        
        private IEnumerator DamageCoroutine()
        {
            yield return new WaitForSeconds(_data.tickDelay);

            if (_targetsInside.Count != 0)
            {
                var healthEventData = new HealthEventData()
                {
                    Amount = _sourceStats.ComputeStat(_data.baseValue, _data.bonusPercentage, _data.effectType),
                    Critical = _sourceStats.ComputeCrit(_data.effectType),
                    Source = _sourceEntity,
                    Type = _data.effectType
                };

                foreach (var enemy in _targetsInside)
                {
                    enemy.Damage(healthEventData);
                }
            }

            if(_data.loop)
                StartCoroutine(DamageCoroutine());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IAffectable affectable))
            {
                _targetsInside.Add(affectable);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out IAffectable affectable))
            {
                _targetsInside.Remove(affectable);
            }
        }
    }
}