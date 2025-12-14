using Steam;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Players
{
    public class PlayerDataManager : NetworkBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }

        public NetworkList<PlayerData> Players = new NetworkList<PlayerData>(default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        
        public bool isInitialized = false;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.OnClientConnectedCallback += OnClientConnected;
        }
        
        private void OnClientConnected(ulong clientId)
        {
            Players.Add(new PlayerData
            {
                clientId = clientId,
                isReady = false,
                isHost = clientId == NetworkManager.Singleton.LocalClientId
            });
        }

        // ---------------- CLIENT → SERVER ----------------

        [ServerRpc(RequireOwnership = false)]
        public void RegisterLocalPlayerServerRpc(
            ulong steamId,
            FixedString32Bytes steamName,
            ServerRpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;

            // Prevent double register
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].clientId == senderClientId)
                    return;
            }

            var isHost = senderClientId == NetworkManager.Singleton.LocalClientId;

            Players.Add(new PlayerData
            {
                clientId = senderClientId,
                steamId = steamId,
                steamName = steamName,
                characterId = default, // none
                isReady = false,
                isHost = isHost
            });
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetCharacterServerRpc(
            FixedString32Bytes characterId,
            ServerRpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];

                if (p.clientId != senderClientId)
                    continue;

                if (p.isReady) continue;

                p.characterId = characterId;
                
                Players[i] = p;
                return;
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(
            ServerRpcParams rpcParams = default)
        {
            var senderClientId = rpcParams.Receive.SenderClientId;

            for (int i = 0; i < Players.Count; i++)
            {
                var p = Players[i];

                if (p.clientId != senderClientId)
                    continue;

                if (p.isReady) continue;

                p.isReady = true;
                
                Players[i] = p;
                return;
            }
        }

        public FixedString32Bytes GetCharacter()
        {
            var senderClientId = NetworkManager.Singleton.LocalClientId;

            foreach (var player in Players)
            {
                if(player.clientId == senderClientId)
                    return player.characterId;
            }
            
            return null;
        }
    }
}