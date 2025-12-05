using System.Collections.Generic;
using Effect;
using Interfaces;
using Skills.Data;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Skills.Entities
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool destroyOnHit = true;

        private Vector3 _direction;
        private float _timer;

        private IStatsComponent _sourceStats;
        private IAffectable _sourceEntity;
        private List<EffectData> _effectsOnHit;

        private readonly HashSet<IAffectable> _alreadyHit = new();

        public void Init(List<EffectData> effectsOnHit, IStatsComponent stats, IAffectable source, Vector3 dir)
        {
            _effectsOnHit = effectsOnHit;
            _sourceStats = stats;
            _sourceEntity = source;
            _direction = dir.normalized;
        }

        private void Update()
        {
            if (!IsServer) return;

            transform.position += _direction * (speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
                GetComponent<NetworkObject>().Despawn();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;

            if (!other.TryGetComponent(out IAffectable target))
                return;

            if (_alreadyHit.Contains(target))
                return;

            _alreadyHit.Add(target);

            foreach (var e in _effectsOnHit)
                EffectExecutor.Execute(e, _sourceStats, _sourceEntity, target, Vector3.zero);

            if (destroyOnHit)
                GetComponent<NetworkObject>().Despawn();
        }
    }
}