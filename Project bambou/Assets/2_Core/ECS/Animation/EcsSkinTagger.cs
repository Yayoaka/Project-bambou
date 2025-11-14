using Unity.Entities;
using UnityEngine;

namespace _2_Core.ECS.Animation
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public sealed class EcsSkinTagAuthoring : MonoBehaviour {}

    public sealed class EcsSkinTagBaker : Baker<EcsSkinTagAuthoring>
    {
        public override void Bake(EcsSkinTagAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Renderable);
            AddComponent<CalculateSkinMatrixTag>(e);
        }
    }
}