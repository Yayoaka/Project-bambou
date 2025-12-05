using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Affectable
{
    public class ShieldComponent : MonoBehaviour
    {
        private class ShieldEntry
        {
            public float Amount;
            public float ExpireTime;
        }

        private readonly List<ShieldEntry> _shields = new();

        private void Update()
        {
            var now = Time.time;

            for (var i = _shields.Count - 1; i >= 0; i--)
            {
                var s = _shields[i];

                if (s.Amount <= 0f || s.ExpireTime <= now)
                    _shields.RemoveAt(i);
            }
        }

        public void AddShield(float amount, float duration)
        {
            if (amount <= 0f || duration <= 0f)
                return;

            _shields.Add(new ShieldEntry
            {
                Amount = amount,
                ExpireTime = Time.time + duration
            });
        }
        
        public float AbsorbDamage(float damage)
        {
            if (damage <= 0f || _shields.Count == 0)
                return damage;

            var remaining = damage;

            for (var i = _shields.Count - 1; i >= 0; i--)
            {
                var s = _shields[i];

                if (remaining <= s.Amount)
                {
                    s.Amount -= remaining;
                    return 0f;
                }

                remaining -= s.Amount;
                _shields.RemoveAt(i);
            }

            return remaining;
        }

        public float TotalShield
        {
            get
            {
                return _shields.Sum(s => s.Amount);
            }
        }
    }
}
