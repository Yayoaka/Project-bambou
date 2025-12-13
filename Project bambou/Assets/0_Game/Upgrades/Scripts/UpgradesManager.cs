using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HUD;
using Unity.Netcode;
using UnityEngine;
using Upgrades.Data;
using Upgrades.EffectUpgrades.Data;
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

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeData> _allUpgrades = new();
        [SerializeField] private List<UpgradeData> _fallbackUpgrades = new();

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
        // MULTIPLE CHOICE (new + upgrades)
        // ---------------------------------------------------------
        public void CallUpgradeSelection()
        {
            if (!IsServer) return;

            _pendingUpgradeChoices.Clear();

            foreach (var kvp in _players)
            {
                var clientId = kvp.Key;
                var pdata = kvp.Value;

                var choices = ResolveMultipleChoices(pdata, 3);
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

        // ---------------------------------------------------------
        // SINGLE CHOICE (upgrade only)
        // ---------------------------------------------------------
        public void CallSingleUpgrade()
        {
            if (!IsServer) return;

            _pendingUpgradeChoices.Clear();

            foreach (var kvp in _players)
            {
                var clientId = kvp.Key;
                var pdata = kvp.Value;

                var choices = ResolveSingleChoice(pdata);

                if (choices.Count == 0)
                {
                    SendFallbackClientRpc();
                    continue;
                }

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

        // ---------------------------------------------------------
        // CLIENT RPCS
        // ---------------------------------------------------------
        [Rpc(SendTo.SpecifiedInParams)]
        private void SendChoicesClientRpc(int[] indices, int[] levels, RpcParams rpcParams = default)
        {
            Time.timeScale = 0;

            var choices = UnpackChoices(indices);

            CharacterHUDManager.Instance.ShowUpgradeChoices(
                choices,
                levels,
                selected =>
                {
                    if (selected == null) return;

                    var idx = _allUpgrades.IndexOf(selected);
                    if (idx < 0) return;

                    CharacterHUDManager.Instance.ShowWaitingScreen();
                    SelectUpgradeServerRpc(idx);
                });
        }

        [Rpc(SendTo.Everyone)]
        private void SendFallbackClientRpc()
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
        // SERVER RPC → APPLY
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

            if (data.isUpgradable)
            {
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
            }
            else
            {
                ApplyInstantUpgrade(pdata, data);
            }

            _pendingUpgradeChoices.Remove(sender);

            if (_pendingUpgradeChoices.Count == 0)
                OnAllPlayersFinishedUpgrading();
        }

        // ---------------------------------------------------------
        // TIMER
        // ---------------------------------------------------------
        private void StartUpgradeTimer()
        {
            if (_upgradeTimerCoroutine != null)
            {
                StopCoroutine(_upgradeTimerCoroutine);
                _upgradeTimerCoroutine = null;
            }

            _upgradeTimerCoroutine = StartCoroutine(UpgradeTimerRoutine());
        }

        private IEnumerator UpgradeTimerRoutine()
        {
            yield return new WaitForSecondsRealtime(UPGRADE_TIMEOUT);

            if (!IsServer) yield break;

            foreach (var clientId in _pendingUpgradeChoices.ToList())
            {
                if (!_players.TryGetValue(clientId, out var pdata))
                    continue;

                var choice = pdata.Levels.Count > 0
                    ? PickRandom(pdata.Levels.Keys.ToList())
                    : PickRandom(_fallbackUpgrades);

                if (choice == null)
                    continue;

                SelectUpgradeServerRpc(_allUpgrades.IndexOf(choice));
            }

            _pendingUpgradeChoices.Clear();
            OnAllPlayersFinishedUpgrading();
        }

        private void OnAllPlayersFinishedUpgrading()
        {
            if (_upgradeTimerCoroutine != null)
            {
                StopCoroutine(_upgradeTimerCoroutine);
                _upgradeTimerCoroutine = null;
            }

            ResumeGameClientRpc();
        }

        // ---------------------------------------------------------
        // CHOICE RESOLUTION
        // ---------------------------------------------------------
        private List<UpgradeData> ResolveMultipleChoices(PlayerUpgradeData pdata, int count)
        {
            var ownedUpgradable = pdata.Levels
                .Where(x => x.Value < 5)
                .Select(x => x.Key)
                .ToList();

            var notOwned = _allUpgrades
                .Except(_fallbackUpgrades)
                .Except(pdata.Levels.Keys)
                .ToList();

            var result = new List<UpgradeData>();
            const float ownedChance = 0.25f;

            for (var i = 0; i < count; i++)
            {
                UpgradeData pick = null;

                if (Random.value < ownedChance && ownedUpgradable.Count > 0)
                {
                    pick = PickRandom(ownedUpgradable);
                    ownedUpgradable.Remove(pick);
                }
                else if (notOwned.Count > 0)
                {
                    pick = PickRandom(notOwned);
                    notOwned.Remove(pick);
                }
                else if (ownedUpgradable.Count > 0)
                {
                    pick = PickRandom(ownedUpgradable);
                    ownedUpgradable.Remove(pick);
                }

                if (pick != null)
                    result.Add(pick);
            }

            return result;
        }

        private List<UpgradeData> ResolveSingleChoice(PlayerUpgradeData pdata)
        {
            var ownedUpgradable = pdata.Levels
                .Where(x => x.Value < 5)
                .Select(x => x.Key)
                .ToList();

            if (ownedUpgradable.Count > 0)
            {
                return new List<UpgradeData>
                {
                    PickRandom(ownedUpgradable)
                };
            }

            return PickFromFallback(1);
        }

        private List<UpgradeData> PickFromFallback(int count)
        {
            var result = new List<UpgradeData>();
            var pool = new List<UpgradeData>(_fallbackUpgrades);

            for (var i = 0; i < count && pool.Count > 0; i++)
            {
                var pick = PickRandom(pool);
                pool.Remove(pick);

                if (pick != null)
                    result.Add(pick);
            }

            return result;
        }

        // ---------------------------------------------------------
        // APPLY
        // ---------------------------------------------------------
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
                case EffectUpgradeData e:
                    pdata.UpgradeComp.DoEffect(e);
                    break;
            }
        }

        private void ApplyLevelUp(PlayerUpgradeData pdata, UpgradeData data, int level)
        {
            switch (data)
            {
                case PassiveUpgradeData p:
                    pdata.UpgradeComp.UpgradePassive(p, level);
                    break;
                case WeaponUpgradeData w:
                    pdata.UpgradeComp.UpgradeWeapon(w, level);
                    break;
            }
        }

        private void ApplyInstantUpgrade(PlayerUpgradeData pdata, UpgradeData data)
        {
            ApplyNewUpgrade(pdata, data);
        }

        // ---------------------------------------------------------
        // HELPERS
        // ---------------------------------------------------------
        private UpgradeData PickRandom(List<UpgradeData> list)
        {
            if (list == null || list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }

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
