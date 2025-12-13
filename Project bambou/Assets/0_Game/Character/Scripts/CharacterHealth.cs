using System;
using Effect.Stats.Data;
using Entity;
using Health;
using Health.CombatText;
using HUD;
using Interfaces;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterHealth : EntityComponent<CharacterBehaviour>, IHealthComponent
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        public event Action OnHealthChanged;
        public event Action OnDeath;

        private Stats.IStatsComponent _stats;

        private void Awake()
        {
            MaxHealth = -1;
            CurrentHealth = MaxHealth;

            _stats = GetComponent<Stats.IStatsComponent>();
            _stats.OnStatsChanged += UpdateStats;
            OnHealthChanged += OnHealthChangedEvent;
        }

        public void ApplyDamage(HealthEventData data)
        {
            if (!IsAlive || !IsServer)
                return;

            float amount = data.Amount;

            if (_stats != null)
            {
                amount = _stats.ComputeDamageTaken(amount, data.Type);
                amount = _stats.ModifyOutgoingEffect(amount, EffectModifierType.IncomingDamage);
            }

            float finalDamage = Mathf.Max(0, amount);
            CurrentHealth -= finalDamage;

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            
            data.Amount = finalDamage;
            data.HitPoint = transform.position;
            
            CombatTextSystem.Instance.DoText(data);
            OnHealthChanged?.Invoke();

            if (CurrentHealth <= 0f)
                HandleDeath(data);
        }

        public void ApplyHeal(HealthEventData data)
        {
            if (!IsAlive)
                return;

            float healAmount = data.Amount;

            if (_stats != null)
            {
                healAmount = _stats.ModifyOutgoingEffect(healAmount, EffectModifierType.OutgoingHeal);
            }

            healAmount = Mathf.Max(0, healAmount);
            CurrentHealth = Mathf.Min(CurrentHealth + healAmount, MaxHealth);

            data.Amount = healAmount;
            data.HitPoint = transform.position;

            CombatTextSystem.Instance.DoText(data);
            OnHealthChanged?.Invoke();
        }

        private void UpdateStats()
        {
            var maxHealth = _stats.GetStat(StatType.MaxHealth);

            if (Mathf.Approximately(MaxHealth, -1))
            {
                CurrentHealth = maxHealth;
                OnHealthChanged?.Invoke();
            }

            MaxHealth = maxHealth;
        }

        public void HandleDeath(HealthEventData data)
        {
            OnDeath?.Invoke();
        }

        private void OnHealthChangedEvent()
        {
            if(!IsServer || !IsSpawned)
                return;
            
            UpdateHealthClientRpc(CurrentHealth, MaxHealth, RpcTarget.Single(Owner.OwnerClientId, RpcTargetUse.Temp));
        }
        
        [Rpc(SendTo.SpecifiedInParams)]
        private void UpdateHealthClientRpc(
            float current,
            float max, RpcParams rpcParams = default)
        {
            CharacterHUDManager.Instance.SetCharacterHealth(current, max);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if(!IsServer || !IsSpawned)
                return;
            
            UpdateHealthClientRpc(CurrentHealth, MaxHealth, RpcTarget.Single(Owner.OwnerClientId, RpcTargetUse.Temp));
        }
    }
}