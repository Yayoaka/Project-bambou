using Unity.Entities;
using Unity.Mathematics;

namespace _2_Core.ECS.Animation
{
    public struct CalculateSkinMatrixTag : IComponentData {}   // tag

    public struct BoneEntity : IBufferElementData { public Entity Value; }
    public struct BindPose   : IBufferElementData { public float4x4 Value; }

// Buffer consommé par Entities Graphics (ShaderGraph "Compute Deformation")
    public struct SkinMatrix : IBufferElementData { public float3x4 Value; }

    public struct RootEntity : IComponentData { public Entity Value; }
}