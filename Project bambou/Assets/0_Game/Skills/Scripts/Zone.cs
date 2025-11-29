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

        private readonly HashSet<IAffectable> _targetsInside = new();
        private readonly List<IAffectable> _processingList = new();

        private Coroutine _damageRoutine;
        private Coroutine _destroyRoutine;

        public void Init(EffectData data, IStatsEntity sourceStats, IAffectable sourceEntity)
        {
            if (!IsServer) return;

            _data = data;
            _sourceStats = sourceStats;
            _sourceEntity = sourceEntity;

            _damageRoutine = StartCoroutine(DamageLoop());
            _destroyRoutine = StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(_data.duration);

            if (_damageRoutine != null)
                StopCoroutine(_damageRoutine);

            GetComponent<NetworkObject>().Despawn();
        }

        private IEnumerator DamageLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_data.tickDelay);

                Debug.Log(_targetsInside.Count);
                if (_targetsInside.Count > 0)
                {
                    // Copy so iterator is safe
                    _processingList.Clear();
                    _processingList.AddRange(_targetsInside);

                    var data = new HealthEventData()
                    {
                        Amount = _sourceStats.ComputeStat(_data.baseValue, _data.bonusPercentage, _data.effectType),
                        Critical = _sourceStats.ComputeCrit(_data.effectType),
                        Source = _sourceEntity,
                        Type = _data.effectType
                    };

                    foreach (var target in _processingList)
                        target.Damage(data);
                }

                if (!_data.loop)
                    break;
            }
        }

        #region Trigger

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (other.TryGetComponent(out IAffectable affectable))
                _targetsInside.Add(affectable);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;

            if (other.TryGetComponent(out IAffectable affectable))
                _targetsInside.Remove(affectable);
        }

        #endregion
    }
}
