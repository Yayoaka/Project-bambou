using Unity.Entities;

namespace Enemies.Visual
{
    public struct PendingVisualEntity : IBufferElementData
    {
        public Entity Value;
    }
}