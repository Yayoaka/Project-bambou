using System;
using System.Collections.Generic;
using System.Linq;
using HUD;
using Unity.Netcode;
using UnityEngine;
using Upgrades.Data;
using Upgrades.PassiveUpgrades.Data;
using Upgrades.WeaponUpgrades.Data;

namespace Upgrades
{
    public class UpgradesManager : NetworkBehaviour
    {
        // ---------------------------------------------------------
        // SINGLETON
        // ---------------------------------------------------------
        public static UpgradesManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        // ---------------------------------------------------------
        // PER-PLAYER DATA
        // ---------------------------------------------------------
        private class PlayerUpgradeData
        {
            public IUpgradeComponent UpgradeComp;
            public Dictionary<UpgradeData, int> Levels = new();
        }

        private readonly Dictionary<ulong, PlayerUpgradeData> _players = new();
        private readonly HashSet<ulong> _pendingUpgradeChoices = new();

        [SerializeField] private List<UpgradeData> _allUpgrades = new();

        // ---------------------------------------------------------
        // REGISTER PLAYER (called when player spawns / is ready)
        // ---------------------------------------------------------
        public void RegisterPlayer(ulong clientId, IUpgradeComponent upgrades)
        {
            var data = new PlayerUpgradeData
            {
                UpgradeComp = upgrades,
                Levels = new Dictionary<UpgradeData, int>()
            };

            _players[clientId] = data;
        }

        // ---------------------------------------------------------
        // PUBLIC API (SERVER ONLY) – GLOBAL UPGRADE PHASE
        // ---------------------------------------------------------

        /// <summary>
        /// Everybody gets an upgrade selection (3 choices per player, adaptés à ce qu'il a).
        /// </summary>
        public void CallUpgradeSelection()
        {
            if (!IsServer) return;

            _pendingUpgradeChoices.Clear();

            foreach (var kvp in _players)
            {
                var clientId = kvp.Key;
                var pdata = kvp.Value;

                var choices = GenerateUpgradeChoices(pdata, 3);
                var packed = PackChoices(choices);

                if (choices.Count == 0)
                    continue;

                _pendingUpgradeChoices.Add(clientId);

                SendChoicesClientRpc(packed);
            }

            // Si personne n'a de choix (bizarre mais possible)
            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
        }

        /// <summary>
        /// Everybody gets a single upgrade choice among owned upgrades.
        /// If a player has no owned upgrades → fallback UI.
        /// </summary>
        public void CallSingleUpgrade()
        {
            if (!IsServer) return;

            _pendingUpgradeChoices.Clear();

            foreach (var kvp in _players)
            {
                var clientId = kvp.Key;
                var pdata = kvp.Value;

                var owned = pdata.Levels.Keys.ToList();

                if (owned.Count == 0)
                {
                    SendFallbackClientRpc();
                    continue;
                }

                var choice = PickRandom(owned);
                var packed = PackChoices(new List<UpgradeData> { choice });

                _pendingUpgradeChoices.Add(clientId);

                SendChoicesClientRpc(packed);
            }

            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
        }

        // ---------------------------------------------------------
        // CLIENT RPC → DISPLAY UI (per-player)
        // ---------------------------------------------------------

        [Rpc(SendTo.Everyone)]
        private void SendChoicesClientRpc(int[] indices, RpcParams rpcParams = default)
        {
            var choices = UnpackChoices(indices);

            CharacterHUDManager.Instance.ShowUpgradeChoices(choices, selected =>
            {
                if (selected == null) return;

                var idx = _allUpgrades.IndexOf(selected);
                if (idx < 0) return;

                SelectUpgradeServerRpc(idx);
            });
        }

        [Rpc(SendTo.Everyone)]
        private void SendFallbackClientRpc(RpcParams rpcParams = default)
        {
            CharacterHUDManager.Instance.ShowSingleUpgradeFallback();
        }

        // ---------------------------------------------------------
        // SERVER RPC → APPLY CHOICE
        // ---------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void SelectUpgradeServerRpc(int upgradeIndex, RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var sender = rpcParams.Receive.SenderClientId;

            if (!_players.TryGetValue(sender, out var pdata))
                return;

            if (upgradeIndex < 0 || upgradeIndex >= _allUpgrades.Count)
                return;

            var data = _allUpgrades[upgradeIndex];

            if (!pdata.Levels.TryGetValue(data, out var level))
            {
                pdata.Levels[data] = 1;
                ApplyNewUpgrade(pdata, data);
            }
            else
            {
                pdata.Levels[data] = level + 1;
                ApplyLevelUp(pdata, data);
            }

            if (_pendingUpgradeChoices.Count > 0)
            {
                _pendingUpgradeChoices.Remove(sender);

                if (_pendingUpgradeChoices.Count == 0)
                    OnAllPlayersFinishedUpgrading();
            }
        }

        // ---------------------------------------------------------
        // UPGRADE LOGIC
        // ---------------------------------------------------------

        private List<UpgradeData> GenerateUpgradeChoices(PlayerUpgradeData pdata, int count)
        {
            var owned = pdata.Levels.Keys.ToList();
            var notOwned = _allUpgrades.Except(owned).ToList();

            var result = new List<UpgradeData>();

            for (var i = 0; i < count; i++)
            {
                UpgradeData pick;

                if (notOwned.Count > 0)
                {
                    pick = PickRandom(notOwned);
                    notOwned.Remove(pick);
                }
                else
                {
                    pick = PickRandom(owned);
                }

                if (pick != null)
                    result.Add(pick);
            }

            return result;
        }

        private UpgradeData PickRandom(List<UpgradeData> list)
        {
            if (list == null || list.Count == 0) return null;
            var index = UnityEngine.Random.Range(0, list.Count);
            return list[index];
        }

        private void ApplyNewUpgrade(PlayerUpgradeData pdata, UpgradeData data)
        {
            switch (data)
            {
                case PassiveUpgradeData p:
                    pdata.UpgradeComp.AddPassive(p);
                    break;

                case WeaponUpgradeData w:
                    pdata.UpgradeComp.AddWeapon(w);
                    break;

                default:
                    Debug.LogError($"[UpgradesManager] Unknown upgrade type: {data}");
                    break;
            }
        }

        private void ApplyLevelUp(PlayerUpgradeData pdata, UpgradeData data)
        {
            switch (data)
            {
                case PassiveUpgradeData p:
                    // For now, same behavior as new upgrade (can be specialized later)
                    pdata.UpgradeComp.AddPassive(p);
                    break;

                case WeaponUpgradeData w:
                    pdata.UpgradeComp.AddWeapon(w);
                    break;

                default:
                    Debug.LogError($"[UpgradesManager] Unknown upgrade type: {data}");
                    break;
            }
        }

        // ---------------------------------------------------------
        // SERIALISATION (indices -> _allUpgrades)
        // ---------------------------------------------------------

        private int[] PackChoices(List<UpgradeData> list)
        {
            var indices = new int[list.Count];
            for (var i = 0; i < list.Count; i++)
                indices[i] = _allUpgrades.IndexOf(list[i]);

            return indices;
        }

        private List<UpgradeData> UnpackChoices(int[] indices)
        {
            var result = new List<UpgradeData>();

            foreach (var i in indices)
            {
                if (i >= 0 && i < _allUpgrades.Count)
                    result.Add(_allUpgrades[i]);
            }

            return result;
        }

        // ---------------------------------------------------------
        // END OF UPGRADE PHASE
        // ---------------------------------------------------------

        private void OnAllPlayersFinishedUpgrading()
        {
            Debug.Log("[UpgradesManager] All players finished selecting upgrades.");
            // TODO: resume gameplay, close upgrade overlay globally, etc.
        }
    }
}
