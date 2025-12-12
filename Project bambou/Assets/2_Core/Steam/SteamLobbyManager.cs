using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Steam
{
    public class SteamLobbyManager : MonoBehaviour
    {
        public static SteamLobbyManager Instance { get; private set; }

        private Callback<LobbyCreated_t> _lobbyCreated;
        private Callback<LobbyEnter_t> _lobbyEntered;
        private Callback<P2PSessionRequest_t> _p2pSessionRequest;

        private CSteamID _currentLobbyId;

        private void Awake()
        {
            Instance = this;

            // Steam callbacks
            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _p2pSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
        }

        public void CreateLobby()
        {
            SteamMatchmaking.CreateLobby(
                ELobbyType.k_ELobbyTypeFriendsOnly,
                4
            );
        }

        private void OnLobbyCreated(LobbyCreated_t result)
        {
            if (result.m_eResult != EResult.k_EResultOK)
            {
                Debug.LogError("Steam lobby creation failed");
                return;
            }

            _currentLobbyId = new CSteamID(result.m_ulSteamIDLobby);

            SteamMatchmaking.SetLobbyData(
                _currentLobbyId,
                "HostSteamId",
                SteamUser.GetSteamID().ToString()
            );

            Debug.Log($"Steam Lobby created: {_currentLobbyId}");

            NetworkManager.Singleton.StartHost();
        }

        /* ===================== CLIENT ===================== */

        public void JoinFriend(CSteamID friendSteamId)
        {
            if (!SteamFriends.GetFriendGamePlayed(friendSteamId, out var gameInfo))
                return;

            SteamMatchmaking.JoinLobby(gameInfo.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t result)
        {
            _currentLobbyId = new CSteamID(result.m_ulSteamIDLobby);

            var hostSteamIdString =
                SteamMatchmaking.GetLobbyData(_currentLobbyId, "HostSteamId");

            var hostSteamId = new CSteamID(ulong.Parse(hostSteamIdString));

            Debug.Log($"Joined lobby {_currentLobbyId}, host={hostSteamId}");

            SteamNetworking.AcceptP2PSessionWithUser(hostSteamId);

            var transport =
                NetworkManager.Singleton.NetworkConfig.NetworkTransport as SteamP2PTransport;

            if (transport == null)
            {
                Debug.LogError("SteamP2PTransport not found on NetworkManager");
                return;
            }

            transport.SetServerSteamId(hostSteamId.m_SteamID);

            NetworkManager.Singleton.StartClient();
        }

        /* ===================== P2P ===================== */

        private void OnP2PSessionRequest(P2PSessionRequest_t request)
        {
            Debug.Log($"Accepting P2P session from {request.m_steamIDRemote}");
            SteamNetworking.AcceptP2PSessionWithUser(request.m_steamIDRemote);
        }
    }
}
