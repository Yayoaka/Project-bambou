using System;
using Enemies.Data;
using Network;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemies.Spawner
{
    public class EnemySpawner : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private NetworkObject prefab;

        [SerializeField] private EnemyDataSo data;
        [SerializeField] private int count = 50;
        [SerializeField] private float waveDelay = 30f;
        [SerializeField] private float minRadius = 500f;
        [SerializeField] private float maxradius = 1000f;
        
        [Header("Player")]
        public Transform player;
        public float minDistanceFromPlayer = 50f;
        
        private float timer = 1111;

        private void Update()
        {
            if (!IsServer) return;
            
            timer += Time.deltaTime;
            if (timer >= waveDelay && player != null)
            {
                timer = 0;
                SpawnAll();
            }
        }

        public override void OnNetworkSpawn()
        {
            PlayerCharacterManager.OnPlayerSpawned += AssignPlayer;
        }

        public void OnDestroy()
        {
            PlayerCharacterManager.OnPlayerSpawned -= AssignPlayer;
        }

        public void AssignPlayer(GameObject player)
        {
            this.player = player.transform;
        }

        private void SpawnAll()
        {
            for (var i = 0; i < count; i++)
                SpawnOne();
        }

        private void SpawnOne()
        {
            var enemyPos = RandomPosition();
            int maxTries = 20; // nb essai max
       
            do
            {
                enemyPos = RandomPosition();
                maxTries--;
            }
            while (Vector3.Distance(enemyPos, player.position) < minDistanceFromPlayer && maxTries > 0);

            if (maxTries <= 0)
            {
                Debug.LogWarning("Impossible de trouver une position de spawn valide après 20 tentatives.");
                return;
            }
            
            var pooled = NetworkObjectPool.Instance.Get(prefab);
            
            pooled.transform.position = enemyPos;
            pooled.transform.rotation = Quaternion.identity;
            
            pooled.Spawn();

            pooled.GetComponent<EnemyBehaviour>().Init(data);
        }

        private Vector3 RandomPosition()
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var dist = Random.Range(minRadius, maxradius);

            var x = Mathf.Cos(angle) * dist;
            var z = Mathf.Sin(angle) * dist;

            return new Vector3(x, 0f, z);
        }
    }
}