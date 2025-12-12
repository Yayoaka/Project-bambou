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

        [Header("Connection")]
        [SerializeField] private float disconnectTimeoutSeconds = 15f;

        private NetworkManager _networkManager;

        private bool _isServer;
        private bool _isClient;

        private CSteamID _serverSteamId;
        private bool _serverSteamIdSet;

        private readonly Dictionary<ulong, CSteamID> _clientIdToSteamId = new();
        private readonly Dictionary<ulong, float> _clientLastSeenTime = new();
        private ulong _nextServerClientId = 1;

        private readonly Queue<QueuedEvent> _eventQueue = new();

        public override ulong ServerClientId => 0;

        private struct QueuedEvent
        {
            public NetworkEvent Type;
            public ulong ClientId;
            public ArraySegment<byte> Payload;
            public float ReceiveTime;
        }

        public override void Initialize(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        /// <summary>
        /// Must be called on the client before StartClient so the transport knows where to send packets.
        /// </summary>
        public void SetServerSteamId(ulong steamId)
        {
            _serverSteamId = new CSteamID(steamId);
            _serverSteamIdSet = true;

            // Client-side mapping: server is always clientId 0.
            _clientIdToSteamId[ServerClientId] = _serverSteamId;
            _clientLastSeenTime[ServerClientId] = Time.realtimeSinceStartup;
        }

        public override bool StartServer()
        {
            _isServer = true;
            _isClient = false;

            _clientIdToSteamId.Clear();
            _clientLastSeenTime.Clear();
            _eventQueue.Clear();
            _nextServerClientId = 1;

            return true;
        }

        public override bool StartClient()
        {
            _isClient = true;
            _isServer = false;

            _eventQueue.Clear();

            if (!_serverSteamIdSet)
            {
                Debug.LogError("[SteamP2PTransport] StartClient failed: ServerSteamId not set. Call SetServerSteamId(hostSteamId) before StartClient().");
                return false;
            }

            // Emit "connected to server" for Netcode.
            EnqueueConnect(ServerClientId);

            // Optional: ensure P2P session is accepted; your SteamLobbyManager already does it. :contentReference[oaicite:3]{index=3}
            SteamNetworking.AcceptP2PSessionWithUser(_serverSteamId);

            return true;
        }

        public override void Shutdown()
        {
            _isServer = false;
            _isClient = false;

            _clientIdToSteamId.Clear();
            _clientLastSeenTime.Clear();
            _eventQueue.Clear();

            _serverSteamIdSet = false;
            _serverSteamId = default;
        }

        public override void Send(ulong clientId, ArraySegment<byte> payload, NetworkDelivery delivery)
        {
            if (!_isServer && !_isClient)
                return;

            if (!_clientIdToSteamId.TryGetValue(clientId, out var steamId))
                return;

            // Respect payload.Offset; SteamNetworking expects a contiguous buffer.
            var bytes = new byte[payload.Count];
            Buffer.BlockCopy(payload.Array, payload.Offset, bytes, 0, payload.Count);

            var sendType = ResolveSendType(delivery);

            SteamNetworking.SendP2PPacket(
                steamId,
                bytes,
                (uint)bytes.Length,
                sendType
            );
        }

        public override NetworkEvent PollEvent(out ulong clientId, out ArraySegment<byte> payload, out float receiveTime)
        {
            clientId = 0;
            payload = default;
            receiveTime = Time.realtimeSinceStartup;

            if (!_isServer && !_isClient)
                return NetworkEvent.Nothing;

            PumpTimeoutDisconnects();

            if (_eventQueue.Count > 0)
            {
                var e = _eventQueue.Dequeue();
                clientId = e.ClientId;
                payload = e.Payload;
                receiveTime = e.ReceiveTime;
                return e.Type;
            }

            if (!SteamNetworking.IsP2PPacketAvailable(out var packetSize))
                return NetworkEvent.Nothing;

            if (packetSize <= 0 || packetSize > MaxPacketSize * 64)
                return NetworkEvent.Nothing;

            var buffer = new byte[packetSize];

            if (!SteamNetworking.ReadP2PPacket(buffer, packetSize, out var bytesRead, out var remoteSteamId))
                return NetworkEvent.Nothing;

            var now = Time.realtimeSinceStartup;

            if (_isServer)
            {
                var cid = GetOrCreateServerClientId(remoteSteamId, now);

                // We may have queued a Connect event first; if so, return it before the Data.
                if (_eventQueue.Count > 0)
                {
                    // Queue the data to be delivered after connect.
                    EnqueueData(cid, buffer, (int)bytesRead, now);

                    var e = _eventQueue.Dequeue();
                    clientId = e.ClientId;
                    payload = e.Payload;
                    receiveTime = e.ReceiveTime;
                    return e.Type;
                }

                clientId = cid;
            }
            else
            {
                // Client: all packets come from server (clientId 0).
                clientId = ServerClientId;
                _clientLastSeenTime[ServerClientId] = now;
            }

            payload = new ArraySegment<byte>(buffer, 0, (int)bytesRead);
            receiveTime = now;
            return NetworkEvent.Data;
        }

        public override ulong GetCurrentRtt(ulong clientId)
        {
            // Steamworks P2P RTT not directly available here; return 0 for now.
            return 0;
        }

        public override void DisconnectRemoteClient(ulong clientId)
        {
            if (!_isServer)
                return;

            if (!_clientIdToSteamId.TryGetValue(clientId, out var steamId))
                return;

            SteamNetworking.CloseP2PSessionWithUser(steamId);

            _clientIdToSteamId.Remove(clientId);
            _clientLastSeenTime.Remove(clientId);

            EnqueueDisconnect(clientId);
        }

        public override void DisconnectLocalClient()
        {
            if (_isClient && _serverSteamIdSet)
            {
                SteamNetworking.CloseP2PSessionWithUser(_serverSteamId);
                EnqueueDisconnect(ServerClientId);
            }

            Shutdown();
        }

        private ulong GetOrCreateServerClientId(CSteamID steamId, float now)
        {
            foreach (var kvp in _clientIdToSteamId)
            {
                if (kvp.Value == steamId)
                {
                    _clientLastSeenTime[kvp.Key] = now;
                    return kvp.Key;
                }
            }

            var newClientId = _nextServerClientId++;
            _clientIdToSteamId[newClientId] = steamId;
            _clientLastSeenTime[newClientId] = now;

            // Emit Connect first so Netcode knows this client exists.
            EnqueueConnect(newClientId);

            return newClientId;
        }

        private void PumpTimeoutDisconnects()
        {
            if (!_isServer)
                return;

            var now = Time.realtimeSinceStartup;

            // Copy keys to avoid modifying collection while iterating.
            var toDisconnect = ListPool<ulong>.Get();
            try
            {
                foreach (var kvp in _clientLastSeenTime)
                {
                    var cid = kvp.Key;
                    var last = kvp.Value;

                    if (cid == ServerClientId)
                        continue;

                    if (now - last >= disconnectTimeoutSeconds)
                        toDisconnect.Add(cid);
                }

                for (var i = 0; i < toDisconnect.Count; i++)
                {
                    var cid = toDisconnect[i];
                    DisconnectRemoteClient(cid);
                }
            }
            finally
            {
                ListPool<ulong>.Release(toDisconnect);
            }
        }

        private void EnqueueConnect(ulong clientId)
        {
            _eventQueue.Enqueue(new QueuedEvent
            {
                Type = NetworkEvent.Connect,
                ClientId = clientId,
                Payload = default,
                ReceiveTime = Time.realtimeSinceStartup
            });
        }

        private void EnqueueDisconnect(ulong clientId)
        {
            _eventQueue.Enqueue(new QueuedEvent
            {
                Type = NetworkEvent.Disconnect,
                ClientId = clientId,
                Payload = default,
                ReceiveTime = Time.realtimeSinceStartup
            });
        }

        private void EnqueueData(ulong clientId, byte[] buffer, int length, float now)
        {
            var copy = new byte[length];
            Buffer.BlockCopy(buffer, 0, copy, 0, length);

            _eventQueue.Enqueue(new QueuedEvent
            {
                Type = NetworkEvent.Data,
                ClientId = clientId,
                Payload = new ArraySegment<byte>(copy, 0, length),
                ReceiveTime = now
            });
        }

        private static EP2PSend ResolveSendType(NetworkDelivery delivery)
        {
            // Keep it simple: map all reliable deliveries to Reliable, everything else to Unreliable.
            switch (delivery)
            {
                case NetworkDelivery.Reliable:
                case NetworkDelivery.ReliableFragmentedSequenced:
                case NetworkDelivery.ReliableSequenced:
                    return EP2PSend.k_EP2PSendReliable;

                default:
                    return EP2PSend.k_EP2PSendUnreliable;
            }
        }

        // Small pooling helper to avoid per-frame allocations in timeout checks.
        private static class ListPool<T>
        {
            private static readonly Stack<List<T>> Pool = new();

            public static List<T> Get()
            {
                if (Pool.Count > 0)
                {
                    var list = Pool.Pop();
                    list.Clear();
                    return list;
                }

                return new List<T>(16);
            }

            public static void Release(List<T> list)
            {
                if (list == null)
                    return;

                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
