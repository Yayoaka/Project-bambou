using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkObjectPool : MonoBehaviour
    {
        public static NetworkObjectPool Instance;

        [System.Serializable]
        public class PoolItem
        {
            public NetworkObject prefab;
            public int prewarmCount = 20;
        }

        public List<PoolItem> items = new List<PoolItem>();

        private Dictionary<uint, Queue<NetworkObject>> pool 
            = new Dictionary<uint, Queue<NetworkObject>>();

        void Awake()
        {
            Instance = this;

            foreach (var item in items)
            {
                uint key = item.prefab.PrefabIdHash;

                if (!pool.ContainsKey(key))
                    pool[key] = new Queue<NetworkObject>();

                for (int i = 0; i < item.prewarmCount; i++)
                {
                    var obj = Instantiate(item.prefab, transform, true);
                    obj.gameObject.SetActive(false);
                    pool[key].Enqueue(obj);
                }
            }
        }

        // ---------------------------------------------------------
        // GET (auto-register if needed)
        // ---------------------------------------------------------
        public NetworkObject Get(NetworkObject prefab)
        {
            uint key = prefab.PrefabIdHash;

            // Auto-add pool if missing
            if (!pool.ContainsKey(key))
            {
                Debug.LogWarning($"[Pool] Prefab {prefab.name} not registered. Creating pool automatically.");
                pool[key] = new Queue<NetworkObject>();
            }

            var q = pool[key];

            if (q.Count > 0)
            {
                var obj = q.Dequeue();
                obj.Spawn();
                obj.gameObject.SetActive(true);
                return obj;
            }

            // Instantiate new when empty
            var newObj = Instantiate(prefab, transform);
            newObj.Spawn();
            return newObj;
        }

        // ---------------------------------------------------------
        // RETURN (auto-register if needed)
        // ---------------------------------------------------------
        public void Return(NetworkObject obj)
        {
            uint key = obj.PrefabIdHash;

            // Auto-register missing pool
            if (!pool.ContainsKey(key))
            {
                Debug.LogWarning($"[Pool] Returned object {obj.name} had no pool entry. Creating queue automatically.");
                pool[key] = new Queue<NetworkObject>();
            }

            obj.gameObject.SetActive(false);

            // DO NOT destroy
            obj.Despawn(false);

            pool[key].Enqueue(obj);
        }
    }
}
