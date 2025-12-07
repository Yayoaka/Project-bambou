using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using Unity.Netcode;
using Upgrades.PassiveUpgrades.Data;
using Upgrades.WeaponUpgrades.Data;
using Stats;
using Effect;
using Interfaces;
using Skills;
using Skills.Entities;
using Stats.Data;
using Random = UnityEngine.Random;

namespace Upgrades
{
    public class UpgradeComponent : NetworkBehaviour, IUpgradeComponent
    {
        // ----------------------------------------------------------
        // INTERNAL STORAGE
        // ----------------------------------------------------------
        private readonly List<PassiveUpgradeData> _passives = new();
        private readonly List<WeaponInstance> _weapons = new();
        private readonly Dictionary<int, WeaponInstance> _weaponById = new();
        private int _nextWeaponId;

        private CharacterBehaviour _character;
        private StatsComponent _stats;

        public event Action OnUpgradesChanged;

        // ----------------------------------------------------------
        // INTERNAL TYPES
        // ----------------------------------------------------------
        private class WeaponInstance
        {
            public int Id;
            public WeaponUpgradeData Data;
            public Coroutine Loop;
            public int Lvl;
        }

        private void Awake()
        {
            _character = GetComponent<CharacterBehaviour>();
            _stats = GetComponent<StatsComponent>();
        }

        // ----------------------------------------------------------
        // PUBLIC API
        // ----------------------------------------------------------

        public void AddPassive(PassiveUpgradeData data)
        {
            if (_passives.Contains(data))
                return;

            data.Lvl = 1;
            
            _passives.Add(data);

            foreach (var effect in data.PassiveEffects)
            {
                EffectExecutor.Execute(
                    effect,
                    _stats,
                    _character.NetworkObjectId,
                    null,
                    Vector3.zero
                );
            }

            OnUpgradesChanged?.Invoke();
        }
        
        public void UpgradePassive(PassiveUpgradeData data, int level)
        {
            if (!_passives.Contains(data))
                return;

            var passive = _passives.First(x => x == data);

            passive.Lvl = level;

            var effect = passive.PassiveEffects[level - 1];
                
            EffectExecutor.Execute(
                effect,
                _stats,
                _character.NetworkObjectId,
                null,
                Vector3.zero
            );

            OnUpgradesChanged?.Invoke();
        }

        public void AddWeapon(WeaponUpgradeData data)
        {
            if (!IsServer)
            {
                Debug.LogWarning($"{name} : AddWeapon should be called on server.");
                return;
            }

            // Simple: one instance per weapon type for now
            foreach (var w in _weapons)
            {
                if (w.Data == data)
                    return;
            }

            var instance = new WeaponInstance
            {
                Id = _nextWeaponId++,
                Data = data,
                Lvl = 1
            };

            _weapons.Add(instance);
            _weaponById[instance.Id] = instance;
            instance.Loop = StartCoroutine(WeaponLoop(instance.Id));

            OnUpgradesChanged?.Invoke();
        }

        public void UpgradeWeapon(WeaponUpgradeData weapon, int level)
        {
            if (!IsServer)
            {
                Debug.LogWarning($"{name} : AddWeapon should be called on server.");
                return;
            }

            var foundWeapon = _weapons.FirstOrDefault(x => x.Data == weapon);
            
            if (foundWeapon == null)
                return;

            foundWeapon.Lvl = level;
            OnUpgradesChanged?.Invoke();
        }

        public (float, float) GetStat(StatType type)
        {
            var flat = 0f;
            var percent = 0f;
            
            foreach (var passive in _passives.Where(x => x.PassiveEffects.Any(x => x.targetStat == type)))
            {
                var effect = passive.PassiveEffects[passive.Lvl - 1];
                
                if (effect.isPercent)
                    percent += effect.baseValue;
                else
                    flat += effect.baseValue;
            }
            
            return (flat, percent);
        }

        // ----------------------------------------------------------
        // WEAPON AUTOCAST LOOP (SERVER)
        // ----------------------------------------------------------
        private IEnumerator WeaponLoop(int weaponId)
        {
            if (!_weaponById.TryGetValue(weaponId, out var instance))
                yield break;

            var data = instance.Data;

            var baseCooldown = data.Cooldown;
            if (baseCooldown <= 0f)
                baseCooldown = 1f;

            while (true)
            {
                var haste = Mathf.Max(0.1f, _stats.GetStat(StatType.Haste));
                var cd = baseCooldown / haste;

                yield return new WaitForSeconds(cd);

                // Ask owner client for targeting info
                RequestTargetClientRpc(weaponId);
            }
        }

        // ----------------------------------------------------------
        // TARGETING RPCs
        // ----------------------------------------------------------

        [Rpc(SendTo.Owner)]
        private void RequestTargetClientRpc(int weaponId)
        {
            if (!IsOwner)
                return;

            var cursor = _character.InputController.GetMousePosition;
            var dir = _character.InputController.GetMouseDirection();

            SendTargetToServerRpc(weaponId, cursor, dir);
        }

        [Rpc(SendTo.Server)]
        private void SendTargetToServerRpc(
            int weaponId,
            Vector3 cursor,
            Vector3 direction,
            RpcParams rpcParams = default)
        {
            var sender = rpcParams.Receive.SenderClientId;
            if (sender != OwnerClientId)
                return;

            
            ExecuteWeaponCast(weaponId, cursor, direction);
        }

        // ----------------------------------------------------------
        // WEAPON CAST EXECUTION (SERVER)
        // ----------------------------------------------------------
        private void ExecuteWeaponCast(int weaponId, Vector3 cursorPos, Vector3 inputDir)
        {
            if (!IsServer)
                return;

            if (!_weaponById.TryGetValue(weaponId, out var instance))
                return;

            var cast = instance.Data.WeaponEffects[instance.Lvl - 1];
            
            // ---------- SPECIAL CASE : MULTI-TARGET ----------
            if (cast.castMode == CastMode.OnAnyTarget)
            {
                int count = cast.additionalZoneCount + Mathf.RoundToInt(_stats.GetStat(StatType.ProjectileCount));
                if (count <= 0) count = 1;

                var targets = GetRandomUniqueEnemies(cast.targetSearchRange, count);

                foreach (var t in targets)
                    SpawnZones(cast, t.position, transform.forward); // direction not important

                return;
            }

            // ---------- NORMAL LOGIC ----------
            var spawnPos = ResolveSpawnPosition(cast.castMode, cursorPos, cast.targetSearchRange);
            var dirBase = ResolveDirection(cast.castMode, cursorPos, cast.targetSearchRange, inputDir);

            if (dirBase == null)
                return;

            var dir = dirBase.Value;

            foreach (var e in cast.appliedEffects)
                EffectExecutor.Execute(e, _stats, _character.NetworkObjectId, null, spawnPos);

            if (cast.spawnProjectile)
                SpawnProjectiles(cast, spawnPos, dir);

            if (cast.spawnZone)
                SpawnZones(cast, spawnPos, dir);
        }

        // ----------------------------------------------------------
        // PROJECTILES
        // ----------------------------------------------------------
        private void SpawnProjectiles(EffectCastData cast, Vector3 spawnPos, Vector3 baseDir)
        {
            var count = cast.additionalProjectileCount + Mathf.RoundToInt(_stats.GetStat(StatType.ProjectileCount));
            if (count <= 0)
                count = 1;

            var dirs = GetSpreadDirections(baseDir, count);

            foreach (var d in dirs)
            {
                var rot = Quaternion.LookRotation(d);
                var obj = Instantiate(cast.projectilePrefab, spawnPos, rot);

                obj.transform.localScale = Vector3.one * _stats.GetStat(StatType.ProjectileSize);

                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();

                obj.GetComponent<Projectile>().Init(
                    cast,
                    _stats,
                    _character.NetworkObjectId,
                    d
                );
            }
        }

        // ----------------------------------------------------------
        // ZONES
        // ----------------------------------------------------------
        private void SpawnZones(EffectCastData cast, Vector3 spawnPos, Vector3 baseDir)
        {
            var count = cast.additionalZoneCount + Mathf.RoundToInt(_stats.GetStat(StatType.ProjectileCount));
            if (count <= 0)
                count = 1;

            var dirs = GetSpreadDirections(baseDir, count);

            foreach (var d in dirs)
            {
                var pos = spawnPos + d * 0.1f;
                var rot = Quaternion.LookRotation(d);

                var obj = Instantiate(cast.zonePrefab, pos, rot);
                obj.transform.localScale = Vector3.one * _stats.GetStat(StatType.ProjectileSize);

                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();

                if (cast.followCaster)
                    obj.transform.SetParent(_character.transform);

                obj.GetComponent<Zone>().Init(cast, _stats, _character.NetworkObjectId);
            }
        }

        // ----------------------------------------------------------
        // POSITION RESOLUTION (CastMode)
        // ----------------------------------------------------------
        private Vector3 ResolveSpawnPosition(CastMode mode, Vector3 cursor, float range)
        {
            switch (mode)
            {
                case CastMode.OnCursor:
                    return cursor;

                case CastMode.OnClosestEnemy:
                {
                    var c = GetClosestEnemy(range);
                    return c ? c.position : transform.position;
                }

                case CastMode.OnAnyTarget:
                {
                    var r = GetRandomEnemy(range);
                    return r ? r.position : transform.position;
                }

                default:
                    return transform.position;
            }
        }

        // ----------------------------------------------------------
        // DIRECTION RESOLUTION (CastMode)
        // ----------------------------------------------------------
        private Vector3? ResolveDirection(
            CastMode mode,
            Vector3 cursor,
            float range,
            Vector3 inputDir)
        {
            switch (mode)
            {
                case CastMode.ToCursor:
                    return (cursor - transform.position).normalized;

                case CastMode.ToClosestEnemy:
                    return GetClosestEnemyDirection(range);

                case CastMode.ToAnyTarget:
                    return GetRandomEnemyDirection(range);

                case CastMode.Default:
                    return inputDir.sqrMagnitude > 0.0001f
                        ? inputDir.normalized
                        : transform.forward;

                case CastMode.OnCursor:
                case CastMode.OnClosestEnemy:
                case CastMode.OnAnyTarget:
                    // Direction not really important for these, use forward
                    return transform.forward;

                default:
                    return transform.forward;
            }
        }

        // ----------------------------------------------------------
        // ENEMY HELPERS
        // ----------------------------------------------------------
        private Transform GetClosestEnemy(float range)
        {
            var pos = transform.position;
            var hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));

            var best = Mathf.Infinity;
            Transform bestT = null;

            foreach (var h in hits)
            {
                var d = (h.transform.position - pos).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestT = h.transform;
                }
            }

            return bestT;
        }

        private Transform GetRandomEnemy(float range)
        {
            var pos = transform.position;
            var hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));
            if (hits.Length == 0) return null;
            return hits[Random.Range(0, hits.Length)].transform;
        }
        
        private List<Transform> GetRandomUniqueEnemies(float range, int count)
        {
            var pos = transform.position;
            var hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));

            var all = new List<Transform>(hits.Length);
            foreach (var h in hits)
                all.Add(h.transform);

            if (all.Count == 0)
                return new List<Transform>();

            // Shuffle (Fisher–Yates)
            for (int i = 0; i < all.Count; i++)
            {
                int j = Random.Range(i, all.Count);
                (all[i], all[j]) = (all[j], all[i]);
            }

            // Take the first N
            return all.GetRange(0, Mathf.Min(count, all.Count));
        }

        private Vector3? GetClosestEnemyDirection(float range)
        {
            var t = GetClosestEnemy(range);
            if (!t) return null;
            return (t.position - transform.position).normalized;
        }

        private Vector3? GetRandomEnemyDirection(float range)
        {
            var t = GetRandomEnemy(range);
            if (!t) return null;
            return (t.position - transform.position).normalized;
        }

        // ----------------------------------------------------------
        // SPREAD (360°)
        // ----------------------------------------------------------
        private List<Vector3> GetSpreadDirections(Vector3 baseDir, int count)
        {
            var dirs = new List<Vector3>();

            if (count <= 1)
            {
                dirs.Add(baseDir.normalized);
                return dirs;
            }

            var step = 360f / count;
            var rotBase = Quaternion.LookRotation(baseDir);

            for (var i = 0; i < count; i++)
            {
                var angle = step * i;
                var rot = rotBase * Quaternion.Euler(0f, angle, 0f);
                dirs.Add(rot * Vector3.forward);
            }

            return dirs;
        }
    }
}
