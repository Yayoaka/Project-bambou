using System.Collections.Generic;
using UnityEngine;

namespace Enemies
{
    public class EnemiesPool : MonoBehaviour
    {
        public static EnemiesPool Instance { get; private set; }

        [SerializeField] private Enemy enemyPrefab;
        [SerializeField] private int initialCount = 50;
        
        private int InitialCount => Application.isEditor ? initialCount : initialCount * 10;

        private readonly Queue<Enemy> _pool = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Prewarm
            for (int i = 0; i < InitialCount; i++)
                AddNewEnemy();
        }

        private Enemy AddNewEnemy()
        {
            var e = Instantiate(enemyPrefab, transform);
            e.gameObject.SetActive(false);
            _pool.Enqueue(e);
            return e;
        }

        public Enemy Get()
        {
            if (_pool.Count == 0)
                AddNewEnemy();

            var e = _pool.Dequeue();
            e.gameObject.SetActive(true);
            return e;
        }

        public void Return(Enemy e)
        {
            e.gameObject.SetActive(false);
            _pool.Enqueue(e);
        }
    }
}
