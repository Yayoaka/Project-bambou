using System;
using System.Collections.Generic;
using Character;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerCharacterManager : NetworkBehaviour
{
    [SerializeField] private CharacterBehaviour championPrefab;

    private readonly Dictionary<ulong, CharacterBehaviour> playerCharacters = new();
    
    public static event Action<GameObject> OnPlayerSpawned;
    public static event Action<GameObject> OnPlayerUnspawned;

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
}
