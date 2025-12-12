using Enemies.Spawner;
using GameState;
using Unity.Netcode;
using UnityEngine;

namespace Wave
{
    public class WaveDirector : NetworkBehaviour
    {
        [SerializeField] private WaveTimeline timeline;

        private WaveContext _context;
        private int _stepIndex;
        private WaveStep _currentStep;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            _context = new WaveContext
            {
                spawner = GetComponent<EnemySpawner>()
            };

            StartNextStep();
        }

        private void Update()
        {
            if (!IsServer || _currentStep == null) return;

            if (_currentStep.OnStepUpdate(_context, Time.deltaTime))
            {
                StartNextStep();
            }
        }

        private void StartNextStep()
        {
            if (_stepIndex >= timeline.steps.Count)
            {
                _currentStep = null;
                OnTimelineCompleted();
                return;
            }

            _currentStep = timeline.steps[_stepIndex];
            _currentStep.OnStepEnter(_context);
            _stepIndex++;
        }
        
        private void OnTimelineCompleted()
        {
            switch (timeline.endPolicy)
            {
                case WaveEndPolicy.Stop:
                    _currentStep = null;
                    break;

                case WaveEndPolicy.Loop:
                    _stepIndex = 0;
                    StartNextStep();
                    break;

                case WaveEndPolicy.LoadNext:
                    timeline = timeline.nextTimeline;
                    _stepIndex = 0;
                    StartNextStep();
                    break;

                case WaveEndPolicy.Endless:
                    //TODO
                    break;

                case WaveEndPolicy.SignalGameState:
                    GameStateManager.Instance.ChangeState(GameStateType.Lobby);
                    break;
            }
        }
    }
}