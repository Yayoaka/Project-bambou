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
        [SerializeField] private float duration = 3f;
        [SerializeField] private float tickDelay = 1f;
        [SerializeField] private bool loop = true;

        private List<EffectData> _effects;
        private IStatsComponent _sourceStats;
        private IAffectable _source;

        private readonly HashSet<IAffectable> _targetsInside = new();
        private readonly List<IAffectable> _buffer = new();

        private Coroutine _tickRoutine;
        private Coroutine _destroyRoutine;

        public void Init(
            List<EffectData> appliedEffects,
            IStatsComponent stats,
            IAffectable source)
        {
            if (!IsServer) return;

            _effects = appliedEffects;
            _sourceStats = stats;
            _source = source;

            _tickRoutine = StartCoroutine(TickLoop());
            _destroyRoutine = StartCoroutine(DestroyAfterDelay());
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(duration);

            if (_tickRoutine != null)
                StopCoroutine(_tickRoutine);

            GetComponent<NetworkObject>().Despawn();
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(tickDelay);

                _buffer.Clear();
                _buffer.AddRange(_targetsInside);

                foreach (var target in _buffer)
                {
                    foreach (var e in _effects)
                        EffectExecutor.Execute(e, _sourceStats, _source, target, Vector3.zero);
                }

                if (!loop)
                    break;
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
