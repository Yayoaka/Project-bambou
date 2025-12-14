using System;
using Unity.Collections;
using Unity.Netcode;

namespace Players
{
    public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
    {
        public ulong clientId;                     // 🔑 identité réseau
        public ulong steamId;
        public FixedString32Bytes steamName;
        public FixedString32Bytes characterId;     // "" = aucun perso
        public bool isReady;
        public bool isHost;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref steamId);
            serializer.SerializeValue(ref steamName);
            serializer.SerializeValue(ref characterId);
            serializer.SerializeValue(ref isReady);
            serializer.SerializeValue(ref isHost);
        }

        public bool Equals(PlayerData other)
        {
            return clientId == other.clientId
                   && steamId == other.steamId
                   && steamName.Equals(other.steamName)
                   && characterId.Equals(other.characterId)
                   && isReady == other.isReady
                   && isHost == other.isHost;
        }
    }
}