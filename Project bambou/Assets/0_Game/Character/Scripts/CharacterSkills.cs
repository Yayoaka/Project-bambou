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
        private CharacterAnimationController _characterAnimationController;

        private IStatsComponent _stats;
        private IAffectable _affectable;

        private void Awake()
        {
            _stats = GetComponent<IStatsComponent>();
            _affectable = GetComponent<IAffectable>();
        }

        public void SetSpells(SpellData[] spells)
        {
            if (!IsServer) return;

            _spells = spells;
            _cooldowns = new float[_spells.Length];
        }

        public void SetAnimationController(CharacterAnimationController controller)
        {
            _characterAnimationController = controller;
        }

        public void TryCast(int index, Vector3 mousePosition, Vector3 direction)
        {
            TryCastServerRpc(index, mousePosition, direction);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TryCastServerRpc(
            int index, Vector3 mousePosition, Vector3 direction, RpcParams rpcParams = default)
        {
            var sender = rpcParams.Receive.SenderClientId;
            if (sender != OwnerClientId)
                return;

            if (!CanCast(index))
                return;

            Cast(index, mousePosition, direction);
        }

        private bool CanCast(int index)
        {
            return _cooldowns[index] <= Time.time;
        }

        private void Cast(int index, Vector3 mousePos, Vector3 dir)
        {
            if (!IsServer) return;

            var spell = _spells[index];
            _cooldowns[index] = Time.time + spell.cooldown;

            foreach (var effect in spell.gameplayEffects)
                EffectExecutor.Execute(effect, _stats, _affectable, _affectable, Vector3.zero);

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

            var spawnRot = cast.toCursor
                ? Quaternion.LookRotation(dir)
                : Quaternion.identity;

            var obj = Instantiate(cast.projectilePrefab, spawnPos, spawnRot);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            obj.GetComponent<Projectile>().Init(
                cast.appliedEffects,
                _stats,
                _affectable,
                dir);
        }

        private void SpawnZone(EffectCastData cast, Vector3 mousePos, Vector3 dir)
        {
            var spawnPos = cast.onCursor ? mousePos : transform.position;

            var spawnRot = cast.toCursor
                ? Quaternion.LookRotation(dir)
                : Quaternion.identity;

            var obj = Instantiate(cast.zonePrefab, spawnPos, spawnRot);
            var netObj = obj.GetComponent<NetworkObject>();
            netObj.Spawn();

            if (cast.followCaster)
            {
                obj.transform.SetParent(transform);
                SetZoneParentClientRpc(netObj.NetworkObjectId, NetworkObjectId);
            }
            
            obj.GetComponent<Zone>().Init(
                cast.appliedEffects,
                _stats,
                _affectable);
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
            _characterAnimationController.TriggerSkill(index + 1);
        }
    }
}
