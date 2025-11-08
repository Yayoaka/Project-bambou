using Unity.Entities;

namespace _2_Core.ECS.Animation
{
    /// <summary>
    /// Tag utilisé pour marquer les entités dont les matrices de skin doivent être calculées.
    /// </summary>
    public struct CalculateSkinMatrixTag : IComponentData
    {
    }
}