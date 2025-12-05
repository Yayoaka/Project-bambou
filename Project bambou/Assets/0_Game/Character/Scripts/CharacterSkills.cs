using System.Collections;
using Effect;
using Entity;
using Interfaces;
using Skills;
using Skills.Data;
using Skills.Entities;
using Stats;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterSkills : EntityComponent<CharacterBehaviour>
    {
        private SpellData[] _spells;
        private float[] _cooldowns;
        private Coroutine[] _autoCastRoutines;

        private CharacterAnimationController _anim;

        private IStatsComponent _stats;
        private IAffectable _affectable;

        private void Awake()
        {
            _stats = GetComponent<IStatsComponent>();
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
        }

        public void SetAnimationController(CharacterAnimationController controller)
        {
            _anim = controller;
        }

        public void TryCast(int index, Vector3 mousePosition, Vector3 direction)
        {
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
                EffectExecutor.Execute(effect, _stats, NetworkObjectId, _affectable, Vector3.zero);

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
            
            if(finalDir == null) return;
            
            var spawnRot = Quaternion.LookRotation((Vector3)finalDir);

            var obj = Instantiate(cast.projectilePrefab, spawnPos, spawnRot);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            obj.GetComponent<Projectile>().Init(
                cast.appliedEffects,
                _stats,
                NetworkObjectId,
                (Vector3)finalDir);
        }

        private void SpawnZone(EffectCastData cast, Vector3 mousePos, Vector3 dir)
        {
            var spawnPos = cast.onCursor ? mousePos : transform.position;
            
            var finalDir = ResolveDirection(cast.castDirectionMode, mousePos, cast.targetSearchRange);
            
            if(finalDir == null) return;
            
            var spawnRot = Quaternion.LookRotation((Vector3)finalDir);

            var obj = Instantiate(cast.zonePrefab, spawnPos, spawnRot);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            if (cast.followCaster)
            {
                obj.transform.SetParent(transform);
                SetZoneParentClientRpc(netObj.NetworkObjectId, NetworkObjectId);
            }

            obj.GetComponent<Zone>().Init(cast.appliedEffects, _stats, NetworkObjectId);
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
    }
}
