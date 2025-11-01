using UnityEngine;

namespace Enemies
{
    public sealed class EnemyHealth : MonoBehaviour, IEnemyModule
    {
        private EnemyContext _ctx;
        private float _hp;

        public void Setup(EnemyContext ctx)
        {
            _ctx = ctx;
            _hp = _ctx.Stats.maxHealth;
        }

        public void OnActivated()
        {
            _hp = _ctx.Stats.maxHealth;
        }

        public void OnDeactivated()
        {
            // No-op for now
        }

        public void Damage(float amount)
        {
            // Minimal safe path
            var newHp = _hp - amount;
            _hp = newHp <= 0f ? 0f : newHp;
            if (_hp <= 0f) gameObject.SetActive(false);
        }
    }
}