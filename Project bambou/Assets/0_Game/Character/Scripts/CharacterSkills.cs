using System.Collections;
using System.Collections.Generic;
using Effect;
using Entity;
using HUD;
using Interfaces;
using Skills;
using Skills.Data;
using Skills.Entities;
using Stats;
using Stats.Data;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Character
{
    public class CharacterSkills : EntityComponent<CharacterBehaviour>
    {
        private SpellData[] _spells;
        private float[] _cooldowns;
        private Coroutine[] _autoCastRoutines;

        private CharacterAnimationController _anim;
        private IAffectable _affectable;

        private void Awake()
        {
            _affectable = GetComponent<IAffectable>();
        }

        // =====================================================================
        // INITIALISATION
        // =====================================================================
        public void SetSpells(SpellData[] spells)
        {
            _spells = spells;
            _cooldowns = new float[_spells.Length];
            _autoCastRoutines = new Coroutine[_spells.Length];

            if (!IsOwner) return;

            // Auto-cast setup
            for (int i = 0; i < spells.Length; i++)
            {
                if (spells[i].autoCast)
                    _autoCastRoutines[i] = StartCoroutine(AutoCastRoutine(i));
            }

            CharacterHUDManager.Instance.SetSpells(spells);
        }

        public void SetAnimationController(CharacterAnimationController controller)
        {
            _anim = controller;
        }

        // =====================================================================
        // CAST INPUT
        // =====================================================================
        public void TryCast(int index, Vector3 mousePosition, Vector3 direction)
        {
            if (_spells[index].autoCast) return;

            TryCastServerRpc(index, mousePosition, direction);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TryCastServerRpc(
            int index, Vector3 mousePosition, Vector3 direction, bool fromAuto = false, RpcParams rpcParams = default)
        {
            ulong sender = rpcParams.Receive.SenderClientId;
            if (sender != OwnerClientId)
                return;

            if (!CanCast(index) && !fromAuto)
                return;

            Cast(index, mousePosition, fromAuto, direction);
        }

        private bool CanCast(int index)
        {
            return _cooldowns[index] <= Time.time;
        }

        // =====================================================================
        // AUTO CAST
        // =====================================================================
        private IEnumerator AutoCastRoutine(int index)
        {
            var spell = _spells[index];

            while (true)
            {
                yield return new WaitForSeconds(spell.cooldown / Owner.Stats.GetStat(StatType.AttackSpeed));
                TryCastServerRpc(index, Owner.InputController.GetMousePosition, Owner.InputController.GetMouseDirection(), true);
            }
        }

        // =====================================================================
        // CAST EXECUTION (SERVER)
        // =====================================================================
        private void Cast(int index, Vector3 mousePos, bool fromAuto, Vector3 dir)
        {
            if (!IsServer) return;

            var spell = _spells[index];
            if (!fromAuto) _cooldowns[index] = Time.time + spell.cooldown / Owner.Stats.GetStat(StatType.Haste);

            // Gameplay effects
            foreach (var effect in spell.gameplayEffects)
                EffectExecutor.Execute(effect, Owner.Stats, NetworkObjectId, _affectable, Vector3.zero);

            // Cast effects
            foreach (var cast in spell.castEffects)
            {
                if (cast.spawnProjectile)
                    SpawnProjectile(cast, mousePos);

                if (cast.spawnZone)
                    SpawnZone(cast, mousePos);
            }

            if (spell.animate)
                CallAnimationRpc(index);
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void CallAnimationRpc(int index)
        {
            _anim.TriggerSkill(index + 1);
        }

        // =====================================================================
        // PROJECTILES
        // =====================================================================
        private void SpawnProjectile(EffectCastData cast, Vector3 mousePos)
        {
            // POSITION
            Vector3 spawnPos = ResolveSpawnPosition(cast.castMode, mousePos, cast.targetSearchRange);

            // DIRECTION
            Vector3? dirBase = ResolveDirection(cast.castMode, mousePos, cast.targetSearchRange);
            if (dirBase == null) return;

            int totalCount = cast.additionalProjectileCount + Mathf.RoundToInt(Owner.Stats.GetStat(StatType.ProjectileCount));
            List<Vector3> dirs = GetSpreadDirections(dirBase.Value, totalCount);

            foreach (var d in dirs)
            {
                Quaternion rot = Quaternion.LookRotation(d);
                GameObject obj = Instantiate(cast.projectilePrefab, spawnPos, rot);

                obj.transform.localScale = Vector3.one * Owner.Stats.GetStat(StatType.ProjectileSize);

                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();

                obj.GetComponent<Projectile>().Init(
                    cast,
                    Owner.Stats,
                    NetworkObjectId,
                    d
                );
            }
        }

        // =====================================================================
        // ZONES
        // =====================================================================
        private void SpawnZone(EffectCastData cast, Vector3 mousePos)
        {
            Vector3 basePos = ResolveSpawnPosition(cast.castMode, mousePos, cast.targetSearchRange);

            Vector3? dirBase = ResolveDirection(cast.castMode, mousePos, cast.targetSearchRange);
            if (dirBase == null) return;

            int totalZones = cast.additionalZoneCount + Mathf.RoundToInt(Owner.Stats.GetStat(StatType.ProjectileCount));

            List<Vector3> dirs = GetSpreadDirections(dirBase.Value, totalZones);

            foreach (var d in dirs)
            {
                Vector3 spawnPos = basePos + d * 0.1f;
                Quaternion rot = Quaternion.LookRotation(d);

                GameObject obj = Instantiate(cast.zonePrefab, spawnPos, rot);
                obj.transform.localScale = Vector3.one * Owner.Stats.GetStat(StatType.ProjectileSize);

                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();

                if (cast.followCaster)
                {
                    obj.transform.SetParent(transform);
                    SetZoneParentClientRpc(netObj.NetworkObjectId, NetworkObjectId);
                }

                obj.GetComponent<Zone>().Init(cast, Owner.Stats, NetworkObjectId);
            }
        }

        [Rpc(SendTo.NotServer, RequireOwnership = false)]
        private void SetZoneParentClientRpc(ulong zoneId, ulong parentId)
        {
            Transform zone = NetworkManager.Singleton.SpawnManager.SpawnedObjects[zoneId].transform;
            Transform parent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentId].transform;
            zone.SetParent(parent);
        }

        // =====================================================================
        // POSITION RESOLUTION
        // =====================================================================
        private Vector3 ResolveSpawnPosition(CastMode mode, Vector3 mousePos, float range)
        {
            switch (mode)
            {
                case CastMode.OnCursor:
                    return mousePos;

                case CastMode.OnClosestEnemy:
                    var c = GetClosestEnemyTransform(range);
                    return c ? c.position : transform.position;

                case CastMode.OnAnyTarget:
                    var r = GetRandomEnemyTransform(range);
                    return r ? r.position : transform.position;

                default:
                    return transform.position;
            }
        }

        // =====================================================================
        // DIRECTION RESOLUTION
        // =====================================================================
        private Vector3? ResolveDirection(CastMode mode, Vector3 mousePos, float range)
        {
            switch (mode)
            {
                case CastMode.ToCursor:
                    return (mousePos - transform.position).normalized;

                case CastMode.ToClosestEnemy:
                    return GetClosestEnemyDirection(range);

                case CastMode.ToAnyTarget:
                    return GetRandomEnemyDirection(range);

                case CastMode.OnCursor:
                case CastMode.OnClosestEnemy:
                case CastMode.OnAnyTarget:
                    return transform.forward; // direction irrelevant

                default:
                    return transform.forward;
            }
        }

        // =====================================================================
        // ENEMY HELPERS
        // =====================================================================
        private Transform GetClosestEnemyTransform(float range)
        {
            Vector3 pos = transform.position;
            Collider[] hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));

            float bestDist = Mathf.Infinity;
            Transform best = null;

            foreach (var h in hits)
            {
                float d = (h.transform.position - pos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = h.transform;
                }
            }

            return best;
        }

        private Transform GetRandomEnemyTransform(float range)
        {
            Vector3 pos = transform.position;
            Collider[] hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));

            if (hits.Length == 0) return null;
            return hits[Random.Range(0, hits.Length)].transform;
        }

        private Vector3? GetClosestEnemyDirection(float range)
        {
            var t = GetClosestEnemyTransform(range);
            if (!t) return null;
            return (t.position - transform.position).normalized;
        }

        private Vector3? GetRandomEnemyDirection(float range)
        {
            var t = GetRandomEnemyTransform(range);
            if (!t) return null;
            return (t.position - transform.position).normalized;
        }

        // =====================================================================
        // SPREAD LOGIC
        // =====================================================================
        private List<Vector3> GetSpreadDirections(Vector3 baseDir, int count)
        {
            List<Vector3> list = new();

            if (count <= 1)
            {
                list.Add(baseDir.normalized);
                return list;
            }

            float step = 360f / count;
            Quaternion baseRot = Quaternion.LookRotation(baseDir);

            for (int i = 0; i < count; i++)
            {
                float angle = step * i;
                Quaternion rot = baseRot * Quaternion.Euler(0f, angle, 0f);
                list.Add(rot * Vector3.forward);
            }

            return list;
        }
    }
}
