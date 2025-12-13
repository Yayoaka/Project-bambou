using Interfaces;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace Collectibles
{
    public class MagneticCollectible : NetworkBehaviour, ICollectible, INetworkPoolable
    {
        public bool IsCollected { get; private set; }

        public void Init()
        {
            IsCollected = false;
        }

        public void StartMagnet(Transform target)
        {
            if (IsCollected) return;

            // Collect instantly
            if (NetworkObject != null && NetworkObject.IsSpawned)
                TryCollectServerRpc();
        }

        [Rpc(SendTo.Server)]
        private void TryCollectServerRpc()
        {
            if (IsCollected) return;

            IsCollected = true;
            OnCollectedServer();

            // Despawn directly
            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObjectPool.Instance.Return(NetworkObject);
        }

        protected virtual void OnCollectedServer() { }
        
        public override void OnNetworkDespawn()
        {
            gameObject.SetActive(false);
        }
        public void OnPoolAcquire()
        {
            gameObject.SetActive(true);
        }

        public void OnPoolRelease()
        {
            gameObject.SetActive(false);
        }
    }
}