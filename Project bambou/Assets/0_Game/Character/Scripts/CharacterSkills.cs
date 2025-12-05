using Entity;
using Skills;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterSkills : EntityComponent<CharacterBehaviour>
    {
        private SpellData[] _spells;
        private float[] _cooldowns;
        private CharacterAnimationController _characterAnimationController;
    
        public void SetSpells(SpellData[] spells)
        {
            if (!IsServer) return;

            _spells = spells;
            _cooldowns = new float[_spells.Length];
        }

        public void SetAnimationController(CharacterAnimationController characterAnimationController)
        {
            _characterAnimationController = characterAnimationController;
        }

        public void TryCast(int index, Vector3 mousePosition, Vector3 direction)
        {
            TryCastServerRpc(index, mousePosition, direction);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TryCastServerRpc(int index, Vector3 mousePosition, Vector3 direction, RpcParams rpcParams = default)
        {
            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != OwnerClientId) 
                return;

            if (!CanCast(index)) return;

            Cast(index, mousePosition, direction);
        }

        private bool CanCast(int index)
        {
            return _cooldowns[index] <= Time.time;
        }

        private void Cast(int index, Vector3 mousePosition, Vector3 direction)
        {
            if (!IsServer) return;
            
            var spell = _spells[index];
            _cooldowns[index] = Time.time + spell.Cooldown;

            foreach (var effect in spell.Effects)
            {
                switch (effect.type)
                {
                    case SpellType.Projectile:
                        CastProjectile(effect, mousePosition, direction);
                        break;
                    case SpellType.Zone:
                        CastZone(effect, mousePosition, direction);
                        break;
                }
            }

            if (spell.Animate) CallAnimationRpc(index);
        }

        private void CastProjectile(EffectData effect, Vector3 mousePosition, Vector3 direction)
        {
            if (!IsServer) return;

            var obj = Instantiate(effect.effectPrefab, effect.onCursor ? mousePosition : transform.position,
                effect.toCursor ? Quaternion.LookRotation(direction) : Quaternion.identity);
            var netObj = obj.GetComponent<NetworkObject>();
        
            netObj.Spawn();

            obj.GetComponent<Projectile>().Init(effect, Owner.Stats, Owner, direction);
        }
        
        private void CastZone(EffectData effect, Vector3 mousePosition, Vector3 direction)
        {
            if (!IsServer) return;

            var obj = Instantiate(effect.effectPrefab, effect.onCursor ? mousePosition : transform.position,
                effect.toCursor ? Quaternion.LookRotation(direction) : Quaternion.identity);
            var netObj = obj.GetComponent<NetworkObject>();
        
            netObj.Spawn();
            
            if (effect.followCaster)
            {
                obj.transform.SetParent(transform);
                
                SetZoneParentClientRpc(netObj.NetworkObjectId, NetworkObjectId);
            }

            obj.GetComponent<Zone>().Init(effect, Owner.Stats, Owner);
        }
        
        [Rpc(SendTo.NotServer, RequireOwnership = false)]
        private void SetZoneParentClientRpc(ulong zoneId, ulong parentId)
        {
            var zoneObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[zoneId].transform;
            var parentObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[parentId].transform;

            zoneObj.SetParent(parentObj);
        }

        [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
        private void CallAnimationRpc(int index)
        {
            _characterAnimationController.TriggerSkill(index + 1);
        }
    }
}