using System.Collections.Generic;
using GameState.States;
using UnityEngine;

namespace GameState
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        private Dictionary<GameStateType, IGameState> _states;
        private IGameState _currentState;
        private GameStateType _currentType;

        void Awake()
        {
            Instance = this;
            BuildStates();
        }

        void BuildStates()
        {
            _states = new Dictionary<GameStateType, IGameState>
            {
                { GameStateType.Menu, new MenuGameState() },
                { GameStateType.Loading, new LoadingGameState() },
                { GameStateType.Lobby, new LobbyGameState() },
                { GameStateType.Mission, new MissionGameState() },
                { GameStateType.GameOver, new GameOverGameState() }
            };
        }

        public void ChangeState(GameStateType newType, IGameStateContext context = null)
        {
            if (newType == _currentType) return;

            _currentState?.Exit();
            _currentType = newType;

            if (_states.TryGetValue(newType, out var newState))
            {
                _currentState = newState;
                _currentState.Enter(context);
            }
            else
            {
                Debug.LogError($"No state registered for {newType}");
            }
        }

        void Update()
        {
            var dt = Time.deltaTime;
            _currentState?.Tick(dt);
        }
    }
}