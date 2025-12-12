using Enemies.Data;
using Unity.Netcode;
using UnityEngine;

namespace Wave
{
    [CreateAssetMenu(menuName = "Waves/Steps/Spawn Group")]
    public class SpawnGroupStep : WaveStep
    {
        public EnemyDataSo data;

        public int count = 10;
        public float spawnInterval = 0.2f;

        private int _spawned;
        private float _timer;

        public override void OnStepEnter(WaveContext context)
        {
            _spawned = 0;
            _timer = 0f;
        }

        public override bool OnStepUpdate(WaveContext context, float deltaTime)
        {
            _timer += deltaTime;

            if (_spawned < count && _timer >= spawnInterval)
            {
                _timer = 0f;
                context.spawner.Spawn(data);
                _spawned++;
            }

            return _spawned >= count;
        }
    }
}