using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Stats.Data;
using UnityEngine;

namespace Buff
{
    public class BuffComponent : MonoBehaviour
    {
        private readonly List<BuffEntry> _buffs = new();

        private Coroutine _cleanupRoutine;
        
        public event Action<BuffEntry> OnBuffChanged;

        private void OnEnable()
        {
            if (_cleanupRoutine == null)
                _cleanupRoutine = StartCoroutine(CleanupLoop());
        }

        private void OnDisable()
        {
            if (_cleanupRoutine != null)
                StopCoroutine(_cleanupRoutine);
        }

        // ------------------------------------------
        // Ajouter un buff
        // ------------------------------------------
        public void AddBuff(StatType stat, float amount, float duration, bool isPercent = false)
        {
            var buff = new BuffEntry
            {
                Stat = stat,
                Amount = amount,
                IsPercentage = isPercent,
                ExpireTime = Time.time + duration
            };
            _buffs.Add(buff);
            
            OnBuffChanged?.Invoke(buff);
        }

        // ------------------------------------------
        // Coroutine de nettoyage
        // ------------------------------------------
        private IEnumerator CleanupLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(0.25f);

            while (true)
            {
                yield return wait;

                float now = Time.time;

                for (int i = _buffs.Count - 1; i >= 0; i--)
                {
                    if (_buffs[i].ExpireTime <= now)
                    {
                        var buff = _buffs[i];
                        _buffs.Remove(buff);
                        OnBuffChanged?.Invoke(buff);
                    }
                }
            }
        }

        // ------------------------------------------
        // Calcul stat finale
        // ------------------------------------------
        public float GetModifiedStat(StatType stat, float baseValue)
        {
            var flat = 0f;
            var percent = 0f;

            foreach (var b in _buffs.Where(b => b.Stat == stat))
            {
                if (b.IsPercentage)
                    percent += b.Amount;
                else
                    flat += b.Amount;
            }

            return (baseValue + flat) * (percent == 0 ? 1 : percent);
        }
    }
}
