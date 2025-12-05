using System.Collections;
using System.Collections.Generic;
using Stats.Data;
using UnityEngine;

namespace Buff
{
    public class BuffComponent : MonoBehaviour
    {
        private readonly List<BuffEntry> _buffs = new();

        private Coroutine _cleanupRoutine;

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
            _buffs.Add(new BuffEntry
            {
                Stat = stat,
                Amount = amount,
                IsPercentage = isPercent,
                ExpireTime = Time.time + duration
            });
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
                        _buffs.RemoveAt(i);
                }
            }
        }

        // ------------------------------------------
        // Calcul stat finale
        // ------------------------------------------
        public float GetModifiedStat(StatType stat, float baseValue)
        {
            float flat = 0f;
            float percent = 0f;

            foreach (var b in _buffs)
            {
                if (b.Stat != stat)
                    continue;

                if (b.IsPercentage)
                    percent += b.Amount;
                else
                    flat += b.Amount;
            }

            return (baseValue + flat) * (1 + percent);
        }
    }
}
