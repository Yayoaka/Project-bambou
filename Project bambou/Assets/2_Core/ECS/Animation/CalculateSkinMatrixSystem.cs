using Enemies.Visual;
using Unity.Burst;
using Unity.Deformations;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace _2_Core.ECS.Animation
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct CalculateSkinMatrixSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (bones, bindPoses, root, localToWorld, entity)
                     in SystemAPI.Query<DynamicBuffer<BoneEntity>,
                             DynamicBuffer<BindPose>,
                             RefRO<RootEntity>,
                             RefRO<LocalToWorld>>()
                         .WithEntityAccess()
                         .WithAll<CalculateSkinMatrixTag>())
            {
                var skinMatrices = SystemAPI.GetBuffer<SkinMatrix>(entity); // ← accès RW autorisé

                if (bones.Length != bindPoses.Length || bones.Length != skinMatrices.Length)
                    continue;

                var rootInverse = math.inverse(localToWorld.ValueRO.Value);

                for (int i = 0; i < bones.Length; i++)
                {
                    var boneEntity = bones[i].Value;
                    if (!SystemAPI.Exists(boneEntity))
                        continue;

                    var boneL2W = SystemAPI.GetComponentRO<LocalToWorld>(boneEntity);
                    var bindPose = bindPoses[i].Value;

                    var skinMatrix = math.mul(rootInverse, math.mul(boneL2W.ValueRO.Value, bindPose));
                    skinMatrices[i] = new SkinMatrix
                    {
                        Value = new float3x4(
                            skinMatrix.c0.xyz,
                            skinMatrix.c1.xyz,
                            skinMatrix.c2.xyz,
                            skinMatrix.c3.xyz)
                    };
                }
            }
        }
    }
}