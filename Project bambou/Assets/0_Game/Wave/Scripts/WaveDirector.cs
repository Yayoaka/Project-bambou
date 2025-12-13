using Enemies.Spawner;
using GameState;
using Unity.Netcode;
using UnityEngine;

namespace Wave
{
    public class WaveDirector : NetworkBehaviour
    {
        [SerializeField] private WaveAsset startWave;

        private WaveRunner _runner;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            _runner = GetComponent<WaveRunner>();
            WaveRunner.OnWaveCompleted += HandleWaveCompleted;

            if (startWave != null)
                _runner.StartWave(startWave);
        }

        private void HandleWaveCompleted(WaveAsset wave)
        {
            switch (wave.onComplete)
            {
                case WaveCompletionAction.NextWave:
                    if (wave.nextWave != null)
                        _runner.StartWave(wave.nextWave);
                    break;

                case WaveCompletionAction.ChangeGameState:
                    GameStateManager.Instance.ChangeState(wave.nextGameState);
                    break;
            }
        }

        private void OnDestroy()
        {
            WaveRunner.OnWaveCompleted -= HandleWaveCompleted;
        }
    }
}