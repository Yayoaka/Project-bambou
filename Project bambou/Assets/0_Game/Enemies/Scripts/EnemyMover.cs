using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

namespace Enemies
{
    public sealed class EnemyMover : MonoBehaviour, IEnemyModule, IEnemyTick
    {
        private EnemyContext _ctx;
        private Transform _target;
        
        private List<PlayerEntity> _players = new();

        public void Setup(EnemyContext ctx)
        {
            _ctx = ctx;

            if(!EnemiesManager.Instance) return;
            
            _players = EnemiesManager.Instance.GetPlayers;

            if(_players.Count == 0) return;
            
            var player = _players.First();
            
            _target = player != null ? player.transform : null;
        }

        public void Tick(float dt)
        {
            if (_target == null) return;

            var pos = _ctx.Transform.position;
            var dir = (_target.position - pos);
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f)
                return; // already at target

            dir.Normalize();

            pos += dir * (_ctx.Stats.moveSpeed * dt);
            _ctx.Transform.position = pos;

            // Rotate gradually toward movement direction
            _ctx.Transform.forward = Vector3.Lerp(_ctx.Transform.forward, dir, dt * 10f);
        }

        private void OnEnable()
        {
            EnemiesManager.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnDisable()
        {
            EnemiesManager.OnPlayerSpawned -= OnPlayerSpawned;
        }

        private void OnPlayerSpawned(PlayerEntity player)
        {
            _players.Add(player);
            _target ??= _players.First().transform;
        }

        public void OnActivated() { }
        public void OnDeactivated() { }
    }
}