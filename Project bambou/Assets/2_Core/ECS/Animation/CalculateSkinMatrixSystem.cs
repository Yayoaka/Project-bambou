using _2_Core.ECS.Animation;
using Enemies.Visual;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Deformations; // SkinMatrix

namespace Tutorials.SimpleAnimation
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))] // important : après la maj des L2W
    public partial struct CalculateSkinMatrixSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (boneEntities, bindPoses, rootEntity, localToWorld, entity)
                     in SystemAPI.Query<DynamicBuffer<BoneEntity>,
                                        DynamicBuffer<BindPose>,
                                        RefRO<RootEntity>,
                                        RefRO<LocalToWorld>>()
                                  .WithAll<CalculateSkinMatrixTag>()
                                  .WithEntityAccess())
            {
                // buffer RW
                var skinMatrices = SystemAPI.GetBuffer<SkinMatrix>(entity);

                var bones        = boneEntities.Reinterpret<Entity>().AsNativeArray();
                var bindPoseArr  = bindPoses   .Reinterpret<float4x4>().AsNativeArray();

                if (bones.Length != bindPoseArr.Length || bones.Length != skinMatrices.Length)
                    continue;

                var rootL2W     = SystemAPI.GetComponentRO<LocalToWorld>(rootEntity.ValueRO.Value);
                var rootInverse = math.inverse(rootL2W.ValueRO.Value);

                for (int i = 0; i < bones.Length; i++)
                {
                    var boneL2W  = SystemAPI.GetComponentRO<LocalToWorld>(bones[i]);
                    var bindPose = bindPoseArr[i];

                    // root^-1 * bone * bindPose
                    var m = math.mul(rootInverse, math.mul(boneL2W.ValueRO.Value, bindPose));

                    skinMatrices[i] = new SkinMatrix
                    {
                        Value = new float3x4(
                            m.c0.xyz,
                            m.c1.xyz,
                            m.c2.xyz,
                            m.c3.xyz)
                    };
                }
            }
        }
    }
}
