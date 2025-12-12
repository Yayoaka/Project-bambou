using System;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

namespace Steam
{
    public class SteamP2PTransport : NetworkTransport
    {
        private const int MaxPacketSize = 1200;

        private bool _isServer;
        private bool _isClient;

        private readonly Dictionary<ulong, CSteamID> _clientSteamIds = new();

        public override ulong ServerClientId => 0;

        public override void Initialize(NetworkManager networkManager)
        {
        }

        public override bool StartServer()
        {
            _isServer = true;
            return true;
        }

        public override bool StartClient()
        {
            _isClient = true;
            return true;
        }

        public override void Shutdown()
        {
            _isServer = false;
            _isClient = false;
            _clientSteamIds.Clear();
        }

        public override void Send(
            ulong clientId,
            ArraySegment<byte> payload,
            NetworkDelivery delivery)
        {
            if (!_clientSteamIds.TryGetValue(clientId, out var steamId))
                return;

            SteamNetworking.SendP2PPacket(
                steamId,
                payload.Array,
                (uint)payload.Count,
                EP2PSend.k_EP2PSendReliable
            );
        }

        public override NetworkEvent PollEvent(
            out ulong clientId,
            out ArraySegment<byte> payload,
            out float receiveTime)
        {
            clientId = 0;
            payload = default;
            receiveTime = Time.realtimeSinceStartup;

            if (!SteamNetworking.IsP2PPacketAvailable(out var packetSize))
                return NetworkEvent.Nothing;

            var buffer = new byte[packetSize];

            if (!SteamNetworking.ReadP2PPacket(
                    buffer,
                    packetSize,
                    out var bytesRead,
                    out var steamId))
                return NetworkEvent.Nothing;

            clientId = GetOrCreateClientId(steamId);
            payload = new ArraySegment<byte>(buffer, 0, (int)bytesRead);

            return NetworkEvent.Data;
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            return 0;
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            if (!_clientSteamIds.TryGetValue(clientId, out var steamId))
                return;

            SteamNetworking.CloseP2PSessionWithUser(steamId);
            _clientSteamIds.Remove(clientId);
        }

        public override void DisconnectLocalClient()
        {
            Shutdown();
        }

        private ulong GetOrCreateClientId(CSteamID steamId)
        {
            foreach (var kvp in _clientSteamIds)
            {
                if (kvp.Value == steamId)
                    return kvp.Key;
            }

            var newClientId = (ulong)_clientSteamIds.Count + 1;
            _clientSteamIds[newClientId] = steamId;
            return newClientId;
        }
    }
}
