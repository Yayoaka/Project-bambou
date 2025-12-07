using System.Collections.Generic;
using Effect;
using Interfaces;
using Skills.Data;
using Stats;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;

namespace Skills.Entities
{
    public class Projectile : NetworkBehaviour
    {
        private float _speed = 10f;
        private float _lifetime = 3f;
        private bool _destroyOnHit = true;

        private Vector3 _direction;
        private float _timer;

        private IStatsComponent _sourceStats;
        private ulong _sourceId;
        private List<EffectData> _effectsOnHit;

        private readonly HashSet<IAffectable> _alreadyHit = new();

        public void Init(EffectCastData cast, IStatsComponent stats, ulong sourceId, Vector3 dir)
        {
            _effectsOnHit = cast.appliedEffects;
            _sourceStats = stats;
            _sourceId = sourceId;
            _direction = dir.normalized;
            _speed = cast.projectileSpeed;
            _lifetime = cast.projectileLifetime;
            _destroyOnHit = cast.destroyOnHit;

            _speed *= stats.GetStat(StatType.ProjectileSpeedMultiplier);
        }

        private void Update()
        {
            if (!IsServer) return;

            transform.position += _direction * (_speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
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
                EffectExecutor.Execute(e, _sourceStats, _sourceId, target, Vector3.zero);

            if (_destroyOnHit)
                GetComponent<NetworkObject>().Despawn();
        }
    }
}