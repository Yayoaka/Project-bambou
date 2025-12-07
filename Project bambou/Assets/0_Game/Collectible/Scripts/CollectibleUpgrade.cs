using Unity.Netcode;
using UnityEngine;
using Upgrades;

namespace Collectible.Scripts
{
    public class CollectibleUpgrade : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer)  return;
            
            if (!other.CompareTag("Player")) return;
            
            UpgradesManager.Instance.CallUpgradeSelection();
            
            NetworkObject.Despawn();
        }
    }
}