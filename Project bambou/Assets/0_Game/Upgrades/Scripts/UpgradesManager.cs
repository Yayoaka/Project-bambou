using System;
using System.Collections;
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

        private Coroutine _upgradeTimerCoroutine;
        private const float UPGRADE_TIMEOUT = 40f;

        // ---------------------------------------------------------
        // REGISTER PLAYER
        // ---------------------------------------------------------
        public void RegisterPlayer(ulong clientId, IUpgradeComponent upgrades)
        {
            _players[clientId] = new PlayerUpgradeData
            {
                UpgradeComp = upgrades,
                Levels = new Dictionary<UpgradeData, int>()
            };
        }


        // ---------------------------------------------------------
        // UPGRADE SELECTION ENTRY POINT
        // ---------------------------------------------------------
        public void CallUpgradeSelection()
        {
            if (!IsServer) return;

            _pendingUpgradeChoices.Clear();

            foreach (var kvp in _players)
            {
                var clientId = kvp.Key;
                var pdata = kvp.Value;

                var choices = GenerateUpgradeChoices(pdata, 3);
                if (choices.Count == 0)
                    continue;

                _pendingUpgradeChoices.Add(clientId);

                SendChoicesClientRpc(
                    PackChoices(choices),
                    PackLevels(choices, pdata),
                    RpcTarget.Single(clientId, RpcTargetUse.Temp)
                );
            }

            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
            else
                StartUpgradeTimer();
        }

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
                _pendingUpgradeChoices.Add(clientId);

                SendChoicesClientRpc(
                    PackChoices(new List<UpgradeData> { choice }),
                    PackLevels(new List<UpgradeData> { choice }, pdata),
                    RpcTarget.Single(clientId, RpcTargetUse.Temp)
                );
            }

            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
            else
                StartUpgradeTimer();
        }


        // ---------------------------------------------------------
        // CLIENT RPCS
        // ---------------------------------------------------------
        [Rpc(SendTo.SpecifiedInParams)]
        private void SendChoicesClientRpc(int[] indices, int[] levels, RpcParams rpcParams = default)
        {
            Time.timeScale = 0;

            var choices = UnpackChoices(indices);

            CharacterHUDManager.Instance.ShowUpgradeChoices(choices, levels, selected =>
            {
                if (selected == null) return;

                var idx = _allUpgrades.IndexOf(selected);
                if (idx < 0) return;

                CharacterHUDManager.Instance.ShowWaitingScreen();

                SelectUpgradeServerRpc(idx);
            });
        }

        [Rpc(SendTo.Everyone)]
        private void SendFallbackClientRpc(RpcParams rpcParams = default)
        {
            Time.timeScale = 0;
            CharacterHUDManager.Instance.ShowSingleUpgradeFallback();
        }

        [Rpc(SendTo.Everyone)]
        private void ResumeGameClientRpc()
        {
            Time.timeScale = 1;
            CharacterHUDManager.Instance.HideUpgradeChoices();
            CharacterHUDManager.Instance.HideWaitingScreen();
        }


        // ---------------------------------------------------------
        // SERVER RPC → CHOICE VALIDATION
        // ---------------------------------------------------------
        [Rpc(SendTo.Server)]
        private void SelectUpgradeServerRpc(int upgradeIndex, RpcParams rpcParams = default)
        {
            if (!IsServer) return;

            var sender = rpcParams.Receive.SenderClientId;
            if (!_players.TryGetValue(sender, out var pdata)) return;

            if (upgradeIndex < 0 || upgradeIndex >= _allUpgrades.Count)
                return;

            var data = _allUpgrades[upgradeIndex];

            // Apply
            if (!pdata.Levels.TryGetValue(data, out var level))
            {
                pdata.Levels[data] = 1;
                ApplyNewUpgrade(pdata, data);
            }
            else
            {
                pdata.Levels[data] = level + 1;
                ApplyLevelUp(pdata, data, level + 1);
            }

            // Mark as finished
            if (_pendingUpgradeChoices.Contains(sender))
                _pendingUpgradeChoices.Remove(sender);

            // If all players finished
            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
        }


        // ---------------------------------------------------------
        // TIMEOUT HANDLING (40s)
        // ---------------------------------------------------------
        private void StartUpgradeTimer()
        {
            if (_upgradeTimerCoroutine != null)
                StopCoroutine(_upgradeTimerCoroutine);

            _upgradeTimerCoroutine = StartCoroutine(UpgradeTimerRoutine());
        }

        private IEnumerator UpgradeTimerRoutine()
        {
            yield return new WaitForSecondsRealtime(UPGRADE_TIMEOUT);

            if (!IsServer) yield break;

            // Auto-resolve missing players
            foreach (var clientId in _pendingUpgradeChoices.ToList())
            {
                if (!_players.TryGetValue(clientId, out var pdata))
                    continue;

                // If no existing upgrades, skip
                var owned = pdata.Levels.Keys.ToList();
                UpgradeData autoChoice = owned.Count > 0 ?
                    PickRandom(owned) :
                    PickRandom(_allUpgrades);

                var idx = _allUpgrades.IndexOf(autoChoice);
                SelectUpgradeServerRpc(idx);
            }

            _pendingUpgradeChoices.Clear();
            OnAllPlayersFinishedUpgrading();
        }


        // ---------------------------------------------------------
        // END OF UPGRADE PHASE
        // ---------------------------------------------------------
        private void OnAllPlayersFinishedUpgrading()
        {
            StopCoroutine(_upgradeTimerCoroutine);

            ResumeGameClientRpc();
        }


        // ---------------------------------------------------------
        // UPGRADE LOGIC
        // ---------------------------------------------------------
        private List<UpgradeData> GenerateUpgradeChoices(PlayerUpgradeData pdata, int count)
        {
            var owned = pdata.Levels
                .Where(x => x.Value < 5)
                .Select(x => x.Key)
                .ToList();

            var notOwned = _allUpgrades.Except(pdata.Levels.Keys).ToList();

            var result = new List<UpgradeData>();
            const float ownedChance = 0.25f;

            for (int i = 0; i < count; i++)
            {
                UpgradeData pick = null;

                bool tryOwned = UnityEngine.Random.value < ownedChance;

                if (tryOwned && owned.Count > 0)
                {
                    pick = PickRandom(owned);
                    owned.Remove(pick);
                }
                else if (notOwned.Count > 0)
                {
                    pick = PickRandom(notOwned);
                    notOwned.Remove(pick);
                }
                else if (owned.Count > 0)
                {
                    pick = PickRandom(owned);
                    owned.Remove(pick);
                }

                if (pick != null)
                    result.Add(pick);
            }

            return result;
        }

        private UpgradeData PickRandom(List<UpgradeData> list)
        {
            if (list == null || list.Count == 0) return null;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        private void ApplyNewUpgrade(PlayerUpgradeData pdata, UpgradeData data)
        {
            switch (data)
            {
                case PassiveUpgradeData p: pdata.UpgradeComp.AddPassive(p); break;
                case WeaponUpgradeData w: pdata.UpgradeComp.AddWeapon(w); break;
            }
        }

        private void ApplyLevelUp(PlayerUpgradeData pdata, UpgradeData data, int level)
        {
            switch (data)
            {
                case PassiveUpgradeData p: pdata.UpgradeComp.UpgradePassive(p, level); break;
                case WeaponUpgradeData w: pdata.UpgradeComp.UpgradeWeapon(w, level); break;
            }
        }


        // ---------------------------------------------------------
        // SERIALISATION HELPERS
        // ---------------------------------------------------------
        private int[] PackChoices(List<UpgradeData> list)
        {
            var arr = new int[list.Count];
            for (int i = 0; i < list.Count; i++)
                arr[i] = _allUpgrades.IndexOf(list[i]);
            return arr;
        }

        private int[] PackLevels(List<UpgradeData> list, PlayerUpgradeData pdata)
        {
            var arr = new int[list.Count];
            for (int i = 0; i < list.Count; i++)
                arr[i] = pdata.Levels.TryGetValue(list[i], out var lvl) ? lvl : 0;
            return arr;
        }

        private List<UpgradeData> UnpackChoices(int[] indices)
        {
            var result = new List<UpgradeData>();
            foreach (var i in indices)
                if (i >= 0 && i < _allUpgrades.Count)
                    result.Add(_allUpgrades[i]);
            return result;
        }
    }
}
