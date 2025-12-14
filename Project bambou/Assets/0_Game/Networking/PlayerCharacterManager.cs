using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Networking
{
    public class PlayerCharacterManager : NetworkBehaviour
    {
        [SerializeField] private CharacterBehaviour championPrefab;
    
        public static PlayerCharacterManager instance;

        private static readonly Dictionary<ulong, CharacterBehaviour> playerCharacters = new();
    
        public static List<GameObject> Characters => playerCharacters.Select(x => x.Value.gameObject).ToList();
    
        public static event Action<GameObject> OnPlayerSpawned;
        public static event Action<GameObject> OnPlayerUnspawned;

        private void Awake()
        {
            instance = this;
        }
        
        private void OnDestroy()
        {
            if (NetworkManager.Singleton == null) return;
        
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            if (playerCharacters.ContainsKey(clientId))
            {
                var existingCharacter = playerCharacters[clientId];

                if (!existingCharacter.IsSpawned)
                    existingCharacter.NetworkObject.SpawnWithOwnership(clientId);
                else
                    existingCharacter.NetworkObject.ChangeOwnership(clientId);

                return;
            }

            Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            var character = Instantiate(championPrefab, spawnPos, Quaternion.identity);
            playerCharacters.Add(clientId, character);

            character.NetworkObject.SpawnWithOwnership(clientId);
        
            OnPlayerSpawned?.Invoke(character.gameObject);
        }


        private void HandleClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            if (!playerCharacters.TryGetValue(clientId, out var character)) return;

            playerCharacters.Remove(clientId);
            OnPlayerUnspawned?.Invoke(character.gameObject);
            if (character != null) Destroy(character.gameObject);
        }

        public void ClearPlayers()
        {
            if (!IsServer)
                return;

            // Snapshot pour éviter toute modification pendant itération
            var snapshot = playerCharacters.ToArray();

            foreach (var kvp in snapshot)
            {
                var clientId = kvp.Key;
                var character = kvp.Value;

                if (character == null)
                    continue;

                var netObj = character.NetworkObject;

                // Event AVANT destruction
                OnPlayerUnspawned?.Invoke(character.gameObject);

                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn();

                Destroy(character.gameObject);
            }

            playerCharacters.Clear();

            Debug.Log("[PlayerCharacterManager] All players cleared.");
        }
    }
}
