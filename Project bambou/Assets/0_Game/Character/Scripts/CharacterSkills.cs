using System;
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

        public void SetSpells(SpellData[] spells)
        {
            _spells = spells;
            _cooldowns = new float[_spells.Length];
            _autoCastRoutines = new Coroutine[_spells.Length];

            if (!IsOwner) return;
            
            // Auto-cast initialisation
            for (int i = 0; i < spells.Length; i++)
            {
                if (spells[i].autoCast)
                    _autoCastRoutines[i] = StartCoroutine(AutoCastRoutine(i));
            }
            
            CharacterHUDManager.instance.SetSpells(spells);
        }

        public void SetAnimationController(CharacterAnimationController controller)
        {
            _anim = controller;
        }

        public void TryCast(int index, Vector3 mousePosition, Vector3 direction)
        {
            if (_spells[index].autoCast) return;
            
            TryCastServerRpc(index, mousePosition, direction);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TryCastServerRpc(
            int index, Vector3 mousePosition, Vector3 direction, bool fromAuto = false, RpcParams rpcParams = default)
        {
            var sender = rpcParams.Receive.SenderClientId;
            if (sender != OwnerClientId)
                return;

            if (!CanCast(index) && !fromAuto) //Not Cheat safe but no time fuck that
                return;

            Cast(index, mousePosition, direction);
        }

        private bool CanCast(int index)
        {
            return _cooldowns[index] <= Time.time;
        }

        private IEnumerator AutoCastRoutine(int index)
        {
            var spell = _spells[index];

            while (true)
            {
                yield return new WaitForSeconds(spell.cooldown);

                TryCastServerRpc(index, Owner.InputController.GetMousePosition, Owner.InputController.GetMouseDirection(), fromAuto: true);
            }
        }

        private void Cast(int index, Vector3 mousePos, Vector3 dir)
        {
            if (!IsServer) return;

            var spell = _spells[index];
            _cooldowns[index] = Time.time + spell.cooldown;

            // Apply gameplay effects instantly
            foreach (var effect in spell.gameplayEffects)
                EffectExecutor.Execute(effect, Owner.Stats, NetworkObjectId, _affectable, Vector3.zero);

            // Casted effects (projectile / zone)
            foreach (var cast in spell.castEffects)
            {
                if (cast.spawnProjectile)
                    SpawnProjectile(cast, mousePos, dir);

                if (cast.spawnZone)
                    SpawnZone(cast, mousePos, dir);
            }

            if (spell.animate)
                CallAnimationRpc(index);
        }

        private void SpawnProjectile(EffectCastData cast, Vector3 mousePos, Vector3 dir)
        {
            var spawnPos = cast.onCursor ? mousePos : transform.position;

            var finalDir = ResolveDirection(cast.castDirectionMode, mousePos, cast.targetSearchRange);
            if (finalDir == null) return;

            var directions = GetProjectileDirections((Vector3)finalDir, Convert.ToInt32(Owner.Stats.GetStat(StatType.ProjectileCount)));

            foreach (var projDir in directions)
            {
                var rot = Quaternion.LookRotation(projDir);
                var obj = Instantiate(cast.projectilePrefab, spawnPos, rot);

                obj.transform.localScale = Vector3.one * Owner.Stats.GetStat(StatType.ProjectileSize);

                var netObj = obj.GetComponent<NetworkObject>();
                netObj.Spawn();

                obj.GetComponent<Projectile>().Init(
                    cast.appliedEffects,
                    Owner.Stats,
                    NetworkObjectId,
                    projDir
                );
            }
        }

        private void SpawnZone(EffectCastData cast, Vector3 mousePos, Vector3 dir)
        {
            var spawnPos = cast.onCursor ? mousePos : transform.position;
            
            var finalDir = ResolveDirection(cast.castDirectionMode, mousePos, cast.targetSearchRange);
            
            if(finalDir == null) return;
            
            var spawnRot = Quaternion.LookRotation((Vector3)finalDir);

            var obj = Instantiate(cast.zonePrefab, spawnPos, spawnRot);

            obj.transform.localScale = Vector3.one * Owner.Stats.GetStat(StatType.ProjectileSize);
            
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            if (cast.followCaster)
            {
                obj.transform.SetParent(transform);
                SetZoneParentClientRpc(netObj.NetworkObjectId, NetworkObjectId);
            }

            obj.GetComponent<Zone>().Init(cast.appliedEffects, Owner.Stats, NetworkObjectId);
        }

        [Rpc(SendTo.NotServer, RequireOwnership = false)]
        private void SetZoneParentClientRpc(ulong zoneId, ulong parentId)
        {
            var zone = NetworkManager.Singleton.SpawnManager.SpawnedObjects[zoneId].transform;
            var parent = NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentId].transform;

            zone.SetParent(parent);
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void CallAnimationRpc(int index)
        {
            _anim.TriggerSkill(index + 1);
        }
        
        private Vector3? ResolveDirection(CastDirectionMode mode, Vector3 mousePos, float range)
        {
            switch (mode)
            {
                case CastDirectionMode.ToCursor:
                    return (mousePos - transform.position).normalized;

                case CastDirectionMode.ToClosestEnemy:
                    return GetClosestEnemyDirection(range);
                
                case CastDirectionMode.ToAnyTarget:
                    return GetRandomEnemyDirection(range);

                default:
                    return transform.forward;
            }
        }
        
        private Vector3? GetClosestEnemyDirection(float range)
        {
            var pos = transform.position;
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

            if (best == null)
                return null;

            return (best.position - pos).normalized;
        }
        
        private Vector3? GetRandomEnemyDirection(float range)
        {
            var pos = transform.position;
            Collider[] hits = Physics.OverlapSphere(pos, range, LayerMask.GetMask("Enemy"));

            if (hits.Length == 0) return null;
            
            var randomIndex = Random.Range(0, hits.Length);

            return (hits[randomIndex].transform.position - pos).normalized;
        }
        
        private List<Vector3> GetProjectileDirections(Vector3 baseDir, int count)
        {
            var list = new List<Vector3>();

            if (count <= 1)
            {
                list.Add(baseDir.normalized);
                return list;
            }

            // Angle entre chaque projectile
            var step = 360f / count;

            // Base rotation to align spread around forward direction
            var baseRot = Quaternion.LookRotation(baseDir);

            for (var i = 0; i < count; i++)
            {
                var angle = step * i;
                var rot = baseRot * Quaternion.Euler(0f, angle, 0f);

                list.Add(rot * Vector3.forward);
            }

            return list;
        }
    }
}
