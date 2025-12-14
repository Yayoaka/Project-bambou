using GameState;
using GameState.States;
using Players;
using SceneLoader;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(InitWhenReady());
        }

        private System.Collections.IEnumerator InitWhenReady()
        {
            while (PlayerDataManager.Instance == null ||
                   !PlayerDataManager.Instance.IsSpawned)
            {
                yield return null;
            }

            var players = PlayerDataManager.Instance.Players;

            players.OnListChanged += UpdatePlayersStates;
        }

        public void Ready()
        {
            PlayerDataManager.Instance.SetReadyServerRpc();
        }
        
        private void UpdatePlayersStates(NetworkListEvent<PlayerData> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Value) return;

            var players = PlayerDataManager.Instance.Players;

            var canStart = true;
            
            foreach (var player in players)
            {
                if (!player.isReady)
                {
                    canStart = false;
                }
            }
            
            if (canStart) StartGame();
        }

        private void StartGame()
        {
            SceneLoaderManager.Instance.LoadSceneAsync(new LoadingContext(GameStateType.Mission));
        }
    }
}