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
                var q = new Queue<NetworkObject>();
                pool[item.prefab.PrefabIdHash] = q;

                for (int i = 0; i < item.prewarmCount; i++)
                {
                    var obj = Instantiate(item.prefab);
                    obj.gameObject.SetActive(false);
                    q.Enqueue(obj);
                }
            }
        }

        public NetworkObject Get(NetworkObject prefab)
        {
            var q = pool[prefab.PrefabIdHash];

            if (q.Count > 0)
            {
                var obj = q.Dequeue();
                obj.gameObject.SetActive(true);
                return obj;
            }

            // pool empty → instantiate new
            var newObj = Instantiate(prefab);
            return newObj;
        }

        public void Return(NetworkObject obj)
        {
            obj.gameObject.SetActive(false);
            obj.Despawn(false); // important: do NOT destroy
            pool[obj.PrefabIdHash].Enqueue(obj);
        }
    }
}