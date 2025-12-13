using System;
using System.Collections;
using Enemies.Spawner;
using Unity.Netcode;
using UnityEngine;

namespace Wave
{
    public class WaveRunner : NetworkBehaviour
    {
        public static event Action<WaveAsset> OnWaveCompleted;

        [SerializeField] private EnemySpawner _spawner;
        private Coroutine _waveRoutine;

        public void StartWave(WaveAsset wave)
        {
            if (!IsServer)
                return;

            if (_waveRoutine != null)
                StopCoroutine(_waveRoutine);

            _waveRoutine = StartCoroutine(RunWave(wave));
        }

        private IEnumerator RunWave(WaveAsset wave)
        {
            foreach (var action in wave.actions)
                yield return ExecuteAction(action);

            _waveRoutine = null;
            OnWaveCompleted?.Invoke(wave);
        }

        private IEnumerator ExecuteAction(WaveAction action)
        {
            switch (action.type)
            {
                case WaveActionType.Spawn:
                    yield return SpawnGroup(action);
                    break;

                case WaveActionType.WaitTime:
                    yield return new WaitForSeconds(action.duration);
                    break;

                case WaveActionType.WaitUntilClear:
                    yield return new WaitUntil(() =>
                        Enemies.EnemyManager.AliveCount <= 0);
                    break;

                case WaveActionType.WaitUntilAliveCount:
                    yield return new WaitUntil(() =>
                        Enemies.EnemyManager.AliveCount <= action.aliveCount);
                    break;
            }
        }

        private readonly System.Collections.Generic.List<Vector3> _positions = new();

        private IEnumerator SpawnGroup(WaveAction action)
        {
            if (action.startDelay > 0)
                yield return new WaitForSeconds(action.startDelay);

            var origin = _spawner.GetSpawnOrigin();

            if (action.pattern != null)
            {
                action.pattern.GeneratePositions(
                    origin,
                    action.enemy,
                    action.count,
                    _positions);
            }

            for (var i = 0; i < _positions.Count; i++)
            {
                _spawner.Spawn(action.enemy, _positions[i]);

                if (action.spawnInterval > 0)
                    yield return new WaitForSeconds(action.spawnInterval);
            }
        }
    }
}