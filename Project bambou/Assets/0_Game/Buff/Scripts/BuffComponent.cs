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
        private readonly List<BuffEntry> _permanentBuffs = new();

        private readonly List<BuffEntry> _timedBuffs = new();

        private Coroutine _cleanupRoutine;

        public event Action OnBuffChanged;

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

        public void AddBuff(StatType stat, float amount, float duration, bool isPercent = false)
        {
            var buff = new BuffEntry
            {
                Stat = stat,
                Amount = amount,
                IsPercentage = isPercent,
                ExpireTime = Time.time + duration
            };

            if (duration > 0)
            {
                _timedBuffs.Add(buff);
            }
            else
            {
                _permanentBuffs.Add(buff);
            }

            OnBuffChanged?.Invoke();
        }

        // --------------------------------------------------------
        // CLEAN TIMED BUFFS
        // --------------------------------------------------------
        private IEnumerator CleanupLoop()
        {
            var wait = new WaitForSeconds(0.25f);

            while (true)
            {
                yield return wait;

                float now = Time.time;

                for (int i = _timedBuffs.Count - 1; i >= 0; i--)
                {
                    if (_timedBuffs[i].ExpireTime <= now)
                    {
                        _timedBuffs.RemoveAt(i);
                        OnBuffChanged?.Invoke();
                    }
                }
            }
        }

        // --------------------------------------------------------
        // GET FINAL STAT
        // --------------------------------------------------------
        public (float, float) GetStatValue(StatType stat)
        {
            float flat = 0f;
            float percent = 0f;

            // Permanent
            foreach (var b in _permanentBuffs.Where(b => b.Stat == stat))
            {
                if (b.IsPercentage)
                    percent += b.Amount;
                else
                    flat += b.Amount;
            }

            // Timed
            foreach (var b in _timedBuffs.Where(b => b.Stat == stat))
            {
                if (b.IsPercentage)
                    percent += b.Amount;
                else
                    flat += b.Amount;
            }

            return (flat, percent);
        }
    }
}
