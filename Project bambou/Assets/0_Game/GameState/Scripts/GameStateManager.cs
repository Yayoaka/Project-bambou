using System.Collections;
using System.Collections.Generic;
using GameState.States;
using Unity.Netcode;
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

            StartCoroutine(RegisterWhenReady());
        }

        private IEnumerator RegisterWhenReady()
        {
            while (NetworkManager.Singleton == null)
            {
                yield return null;
            }
            
            
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
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
        
        

        #region Connexion

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            if (_currentType == GameStateType.Mission)
            {
                response.Approved = false;
                response.Reason = "Game already started";
                return;
            }

            response.Approved = true;
            response.CreatePlayerObject = true;
        }

        #endregion
    }
}