using Skills;
using Unity.Netcode;
using UnityEngine;

namespace Character
{
    public class CharacterSkills : NetworkBehaviour
    {
        private SpellData[] _spells;
        private float[] _cooldowns;
    
        public void SetSpells(SpellData[] spells)
        {
            if (!IsServer) return;

            _spells = spells;
            _cooldowns = new float[_spells.Length];
        }

        public void TryCast(int index, Vector3 direction)
        {
            TryCastServerRpc(index, direction);
        }

        [Rpc(SendTo.Server, RequireOwnership = false)]
        private void TryCastServerRpc(int index, Vector3 direction, RpcParams rpcParams = default)
        {
            var senderId = rpcParams.Receive.SenderClientId;
            if (senderId != OwnerClientId) 
                return;

            if (!CanCast(index)) return;

            Cast(index, direction);
        }

        private bool CanCast(int index)
        {
            return _cooldowns[index] <= Time.time;
        }

        private void Cast(int index, Vector3 direction)
        {
            if (!IsServer) return;
            
            var spell = _spells[index];
            _cooldowns[index] = Time.time + spell.Cooldown;

            foreach (var effect in spell.Effects)
            {
                switch (effect.type)
                {
                    case SpellType.Projectile:
                        CastProjectile(effect, direction);
                        break;
                }
            }
        }

        private void CastProjectile(EffectData effect, Vector3 direction)
        {
            if (!IsServer) return;
            
            GameObject obj = Instantiate(effect.effectPrefab, transform.position + Vector3.up * 1.2f, Quaternion.LookRotation(direction));
            NetworkObject netObj = obj.GetComponent<NetworkObject>();
        
            netObj.Spawn();

            obj.GetComponent<Projectile>().Initialize(direction);
        }
    }
}