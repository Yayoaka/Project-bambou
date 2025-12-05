using Effect;
using Unity.Netcode;
using UnityEngine;

namespace Health
{
    public struct HealthEventData : INetworkSerializable
    {
        public float Amount;
        public Vector3 HitPoint;
        public EffectType Type;
        public bool Critical;
        public ulong SourceId;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Amount);
            serializer.SerializeValue(ref HitPoint);
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref Critical);
            serializer.SerializeValue(ref SourceId);
        }
    }
}