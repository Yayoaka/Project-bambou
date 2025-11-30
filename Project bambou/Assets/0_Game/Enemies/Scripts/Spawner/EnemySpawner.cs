using UnityEngine;

namespace Enemies.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int count = 50;
        [SerializeField] private float radius = 20f;

        void Start()
        {
            SpawnAll();
        }

        void SpawnAll()
        {
            for (var i = 0; i < count; i++)
                SpawnOne();
        }

        void SpawnOne()
        {
            var pos = RandomPosition();
            Instantiate(prefab, pos, Quaternion.identity);
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