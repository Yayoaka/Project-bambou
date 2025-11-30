using Enemies.Data;
using Network;
using Unity.Netcode;
using UnityEngine;

namespace Enemies.Spawner
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private NetworkObject prefab;
        [SerializeField] private EnemyDataSo data;
        [SerializeField] private int count = 50;
        [SerializeField] private float radius = 20f;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                SpawnAll();
            }
        }

        void SpawnAll()
        {
            for (var i = 0; i < count; i++)
                SpawnOne();
        }

        void SpawnOne()
        {
            var pos = RandomPosition();
            
            var pooled = NetworkObjectPool.Instance.Get(prefab);
            
            pooled.transform.position = pos;
            pooled.transform.rotation = Quaternion.identity;

            pooled.Spawn();

            pooled.GetComponent<EnemyBehaviour>().Init(data);
        }

        Vector3 RandomPosition()
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var dist = Random.Range(0f, radius);

            var x = Mathf.Cos(angle) * dist;
            var z = Mathf.Sin(angle) * dist;

            return new Vector3(x, 0f, z);
        }
    }
}