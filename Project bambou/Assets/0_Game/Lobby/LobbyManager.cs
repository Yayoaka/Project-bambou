using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using SceneLoader;
using GameState;
using GameState.States;
using Unity.Collections;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private int minPlayersToStart = 2;
    [SerializeField] private int maxPlayers = 4;

    private NetworkList<LobbyPlayerData> m_lobbyPlayers;
    private Dictionary<ulong, bool> m_playerReadyState = new Dictionary<ulong, bool>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        
        m_lobbyPlayers = new NetworkList<LobbyPlayerData>();
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AddHostPlayer();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        m_lobbyPlayers.OnListChanged += OnLobbyPlayersChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        m_lobbyPlayers.OnListChanged -= OnLobbyPlayersChanged;
    }
    
    private void AddHostPlayer()
    {
        ulong hostClientId = NetworkManager.ServerClientId;
        
        foreach (var player in m_lobbyPlayers)
        {
            if (player.clientId == hostClientId)
            {
                Debug.Log("L'hôte est déjà dans la liste");
                return;
            }
        }
        
        var hostPlayer = new LobbyPlayerData
        {
            clientId = hostClientId,
            playerName = $"Hôte_{hostClientId}",
            isReady = true,
            isHost = true
        };
        
        m_lobbyPlayers.Add(hostPlayer);
        Debug.Log($"Hôte ajouté au lobby : Client {hostClientId}");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected to lobby");

        var newPlayer = new LobbyPlayerData
        {
            clientId = clientId,
            playerName = $"Joueur_{clientId}",
            isReady = false,
            isHost = (clientId == NetworkManager.ServerClientId)
        };
        
        m_lobbyPlayers.Add(newPlayer);
        m_playerReadyState[clientId] = false;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected from lobby");
        
        for (int i = m_lobbyPlayers.Count - 1; i >= 0; i--)
        {
            if (m_lobbyPlayers[i].clientId == clientId)
            {
                m_lobbyPlayers.RemoveAt(i);
                break;
            }
        }
        
        m_playerReadyState.Remove(clientId);
    }

    private void OnLobbyPlayersChanged(NetworkListEvent<LobbyPlayerData> changeEvent)
    {
        LobbyUI lobbyUI = FindFirstObjectByType<LobbyUI>();
        if (lobbyUI != null)
        {
            lobbyUI.UpdatePlayerList();
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void ChangeCharacterServerRpc(ulong clientId, int characterIndex)
    {
        for (int i = 0; i < m_lobbyPlayers.Count; i++)
        {
            if (m_lobbyPlayers[i].clientId == clientId)
            {
                var playerData = m_lobbyPlayers[i];
                m_lobbyPlayers[i] = playerData;
                break;
            }
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(ulong clientId, bool isReady)
    {
        m_playerReadyState[clientId] = isReady;
        
        for (int i = 0; i < m_lobbyPlayers.Count; i++)
        {
            if (m_lobbyPlayers[i].clientId == clientId)
            {
                var playerData = m_lobbyPlayers[i];
                playerData.isReady = isReady;
                m_lobbyPlayers[i] = playerData;
                break;
            }
        }
    }

    public bool AreAllPlayersReady()
    {
        if (m_lobbyPlayers.Count < minPlayersToStart)
            return false;

        foreach (var player in m_lobbyPlayers)
        {
            if (player.isHost)
                continue;
                
            if (!player.isReady)
                return false;
        }
        
        return true;
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void StartGameServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        
        if (clientId != NetworkManager.ServerClientId)
        {
            Debug.LogWarning("Seul l'hôte peut démarrer la partie!");
            return;
        }

        /*if (!AreAllPlayersReady())
        {
            Debug.LogWarning("Tous les joueurs ne sont pas prêts!");
            NotifyNotAllReadyClientRpc();
            return;
        }*/
        
        LoadGameplayScene();
    }
    

    private void LoadGameplayScene()
    {
        if (SceneLoaderManager.Instance != null)
        {
            Debug.Log("Loading mission scene");
            SceneLoaderManager.Instance.LoadSceneAsync(new LoadingContext(GameState.GameStateType.Mission));
        }
    }

    [ClientRpc]
    private void NotifyNotAllReadyClientRpc()
    {
        Debug.Log("Tous les joueurs ne sont pas prêts!");
    }

    public LobbyPlayerData? GetPlayerData(ulong clientId)
    {
        foreach (var player in m_lobbyPlayers)
        {
            if (player.clientId == clientId)
                return player;
        }
        return null;
    }

    public List<LobbyPlayerData> GetAllPlayers()
    {
        var players = new List<LobbyPlayerData>();
        foreach (var player in m_lobbyPlayers)
        {
            players.Add(player);
        }
        return players;
    }

    public void ReturnToMainMenu()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
        }

        if (SceneLoaderManager.Instance != null)
        {
            SceneLoaderManager.Instance.LoadSceneAsync(new LoadingContext(GameState.GameStateType.Menu));
        }
    }
}

public struct LobbyPlayerData : INetworkSerializable, System.IEquatable<LobbyPlayerData>
{
    public ulong clientId;
    public FixedString64Bytes playerName;
    public bool isReady;
    public bool isHost;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref isReady);
        serializer.SerializeValue(ref isHost);
    }

    public bool Equals(LobbyPlayerData other)
    {
        return clientId == other.clientId &&
               playerName == other.playerName &&
               isReady == other.isReady &&
               isHost == other.isHost;
    }

    public override bool Equals(object obj)
    {
        return obj is LobbyPlayerData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return System.HashCode.Combine(clientId, playerName, isReady, isHost);
    }
}