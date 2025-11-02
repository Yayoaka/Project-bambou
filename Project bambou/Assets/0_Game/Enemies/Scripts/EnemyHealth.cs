using System;
using UnityEngine;

namespace Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IEnemyModule
    {
        private EnemyContext _ctx;
        private float _hp;
        
        public event Action<Enemy> OnDeath;

        public void Setup(EnemyContext ctx)
        {
            _ctx = ctx;
            _hp = _ctx.Stats.maxHealth;
        }

        public void OnActivated()
        {
            _hp = _ctx.Stats.maxHealth;
        }

        public void OnDeactivated() { }

        public void Damage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0f)
                Die();
        }
        
        private void Die()
        {
            OnDeath?.Invoke(_ctx.Enemy);
            EnemiesPool.Instance.Return(_ctx.Enemy);
        }
    }
}