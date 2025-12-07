using System.Collections;
using System.Collections.Generic;
using Effect;
using Interfaces;
using Skills;
using Skills.Data;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Skills.Entities
{
    public class Zone : NetworkBehaviour
    {
        [Header("Zone Settings")]
        private float _duration;
        private float _firstTickDelay;
        private float _tickDelay;
        private bool _loop;

        private List<EffectData> _effects;
        private IStatsComponent _sourceStats;
        private ulong _sourceId;

        private readonly HashSet<IAffectable> _targetsInside = new();
        private readonly List<IAffectable> _buffer = new();

        private Coroutine _tickRoutine;
        private Coroutine _destroyRoutine;

        public void Init(
            EffectCastData cast,
            IStatsComponent stats,
            ulong sourceId)
        {
            if (!IsServer) return;

            _effects = cast.appliedEffects;
            _sourceStats = stats;
            _sourceId = sourceId;
            _duration = cast.zoneLifetime;
            _firstTickDelay = cast.firstTickDelay;
            _tickDelay = cast.tickRate;
            _loop = cast.loop;
            

            _tickRoutine = StartCoroutine(TickLoop());
            _destroyRoutine = StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(_duration);

            if (_tickRoutine != null)
                StopCoroutine(_tickRoutine);

            GetComponent<NetworkObject>().Despawn();
        }

        private IEnumerator TickLoop()
        {
            yield return new WaitForSeconds(_firstTickDelay);
            
            while (true)
            {
                _buffer.Clear();
                _buffer.AddRange(_targetsInside);

                foreach (var target in _buffer)
                {
                    foreach (var e in _effects)
                        EffectExecutor.Execute(e, _sourceStats, _sourceId, target, Vector3.zero);
                }

                if (!_loop)
                    break;
                
                yield return new WaitForSeconds(_tickDelay);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (other.TryGetComponent(out IAffectable a))
                _targetsInside.Add(a);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsServer) return;

            if (other.TryGetComponent(out IAffectable a))
                _targetsInside.Remove(a);
        }
    }
}
