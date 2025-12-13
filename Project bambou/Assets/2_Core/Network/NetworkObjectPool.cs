using System.Collections.Generic;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkObjectPool : NetworkBehaviour
    {
        public static NetworkObjectPool Instance;

        [System.Serializable]
        public class PoolItem
        {
            public NetworkObject prefab;
            public int prewarmCount = 10;
        }

        [SerializeField] private List<PoolItem> items = new();

        private readonly Dictionary<uint, Queue<NetworkObject>> _available = new();
        private readonly HashSet<NetworkObject> _inUse = new();
        private readonly Dictionary<uint, NetworkObject> _prefabByKey = new();

        private void Awake()
        {
            Instance = this;

            // Build prefab lookup for runtime additions
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.prefab == null)
                    continue;

                var key = item.prefab.PrefabIdHash;
                if (!_prefabByKey.ContainsKey(key))
                    _prefabByKey.Add(key, item.prefab);
            }
        }

        // ------------------------------------------------------
        // PREWARM (SERVER ONLY)
        // ------------------------------------------------------
        public override void OnNetworkSpawn()
        {
            if (!IsServer)
                return;

            Prewarm();
        }

        public void Prewarm()
        {
            foreach (var item in items)
            {
                EnsurePoolExists(item.prefab);

                for (var i = 0; i < item.prewarmCount; i++)
                    CreateAndEnqueue(item.prefab);
            }
        }

        // ------------------------------------------------------
        // GET
        // ------------------------------------------------------
        public NetworkObject Get(
            NetworkObject prefab,
            Vector3 position,
            Quaternion rotation = default)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkPool] Get called on client.");
                return null;
            }

            EnsurePoolExists(prefab);

            var key = prefab.PrefabIdHash;
            var q = _available[key];

            // Pool empty → grow dynamically
            if (q.Count == 0)
            {
                CreateAndEnqueue(prefab);
            }

            var obj = q.Dequeue();
            _inUse.Add(obj);

            obj.transform.SetPositionAndRotation(position, rotation);
            CallPoolAcquire(obj);

            return obj;
        }

        // ------------------------------------------------------
        // RETURN
        // ------------------------------------------------------
        public void Return(NetworkObject obj)
        {
            if (!IsServer || obj == null)
                return;

            if (!_inUse.Remove(obj))
                return;

            InternalReturn(obj, obj.PrefabIdHash);
        }
        
        public void ReturnOrAdopt(NetworkObject obj)
        {
            if (!IsServer || obj == null)
                return;

            var key = obj.PrefabIdHash;

            // If already pooled → normal return
            if (_inUse.Remove(obj))
            {
                InternalReturn(obj, key);
                return;
            }

            // -----------------------------------------
            // ADOPT EXTERNAL OBJECT
            // -----------------------------------------
            Debug.LogWarning($"[NetworkPool] Adopting external NetworkObject {obj.name}");

            // Ensure pool exists
            if (!_available.ContainsKey(key))
                _available[key] = new Queue<NetworkObject>();

            // Make sure it is spawned
            if (!obj.IsSpawned)
                obj.Spawn();

            // Reset pooled state
            CallPoolRelease(obj);

            _available[key].Enqueue(obj);
        }
        
        private void InternalReturn(NetworkObject obj, uint key)
        {
            CallPoolRelease(obj);

            if (!_available.ContainsKey(key))
                _available[key] = new Queue<NetworkObject>();

            _available[key].Enqueue(obj);
        }

        // ------------------------------------------------------
        // INTERNAL HELPERS
        // ------------------------------------------------------
        private void EnsurePoolExists(NetworkObject prefab)
        {
            var key = prefab.PrefabIdHash;

            if (_available.ContainsKey(key))
                return;

            Debug.LogWarning($"[NetworkPool] Prefab {prefab.name} was not registered. Auto-adding to pool.");

            _available[key] = new Queue<NetworkObject>();

            if (!_prefabByKey.ContainsKey(key))
                _prefabByKey.Add(key, prefab);
        }

        private void CreateAndEnqueue(NetworkObject prefab)
        {
            var obj = Instantiate(prefab);
            obj.Spawn(); // SPAWN UNE SEULE FOIS, objet persistant

            CallPoolRelease(obj);

            var key = prefab.PrefabIdHash;
            _available[key].Enqueue(obj);
        }

        // ------------------------------------------------------
        // INTERFACE DISPATCH
        // ------------------------------------------------------
        private static void CallPoolAcquire(NetworkObject obj)
        {
            var poolables = obj.GetComponentsInChildren<INetworkPoolable>(true);
            for (var i = 0; i < poolables.Length; i++)
                poolables[i].OnPoolAcquire();
        }

        private static void CallPoolRelease(NetworkObject obj)
        {
            var poolables = obj.GetComponentsInChildren<INetworkPoolable>(true);
            for (var i = 0; i < poolables.Length; i++)
                poolables[i].OnPoolRelease();
        }
    }
}
