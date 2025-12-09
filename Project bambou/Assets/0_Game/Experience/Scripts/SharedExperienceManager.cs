using System;
using Unity.Netcode;
using UnityEngine;
using Upgrades;

namespace Experience
{
    public class SharedExperienceManager : NetworkBehaviour
    {
        public static SharedExperienceManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        // ----------------------------------------------------------
        // NETWORK VARIABLES (partagées entre tous les joueurs)
        // ----------------------------------------------------------
        public NetworkVariable<int> Level = new(
            1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> CurrentXP = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        [SerializeField] private AnimationCurve xpCurve =
            AnimationCurve.Linear(1, 10, 50, 500);

        // ----------------------------------------------------------
        // EVENTS (HB / UI)
        // ----------------------------------------------------------
        public event Action<int, int, int> OnXPChanged; 
        // lvl, xp, xpNeeded

        public event Action<int> OnLevelUp;

        private void Start()
        {
            Level.OnValueChanged += (_, newLvl) => HandleLevelUp(newLvl);
            CurrentXP.OnValueChanged += (_, __) => NotifyHUD();

            NotifyHUD();
        }

        // ----------------------------------------------------------
        // PUBLIC API : tout le monde peut donner de l’XP
        // ----------------------------------------------------------
        public void AddXP(int amount)
        {
            if (amount <= 0) return;
            AddXPServerRpc(amount);
        }

        [Rpc(SendTo.Server)]
        private void AddXPServerRpc(int amount)
        {
            if (!IsServer) return;

            CurrentXP.Value += amount;
            TryLevelUp();
        }

        // ----------------------------------------------------------
        // LOGIC
        // ----------------------------------------------------------
        private void TryLevelUp()
        {
            int xpNeeded = GetXPRequired(Level.Value);

            while (CurrentXP.Value >= xpNeeded)
            {
                CurrentXP.Value -= xpNeeded;
                Level.Value++;

                // upgrade all players simultanément
                UpgradesManager.Instance.CallUpgradeSelection();

                xpNeeded = GetXPRequired(Level.Value);
            }

            NotifyHUD();
        }

        private int GetXPRequired(int lvl)
        {
            return Mathf.RoundToInt(xpCurve.Evaluate(lvl));
        }

        // ----------------------------------------------------------
        // UI UPDATE
        // ----------------------------------------------------------
        private void HandleLevelUp(int newLevel)
        {
            OnLevelUp?.Invoke(newLevel);
            NotifyHUD();
        }

        private void NotifyHUD()
        {
            int needed = GetXPRequired(Level.Value);
            OnXPChanged?.Invoke(Level.Value, CurrentXP.Value, needed);
        }
    }
}
