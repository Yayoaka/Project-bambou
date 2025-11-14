using _2_Core.ECS.Animation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))] // s'exécute pendant le rendu
public partial struct CalculateSkinMatrixSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var em = state.EntityManager;

        foreach (var (root, entity) in SystemAPI.Query<RefRO<RootEntity>>().WithEntityAccess())
        {
            // Récupère tous les enfants de ce rig (mesh, bones, etc.)
            if (!em.HasBuffer<LinkedEntityGroup>(entity))
                continue;

            var linkedGroup = em.GetBuffer<LinkedEntityGroup>(entity);
            Entity? meshEntity = null;

            // Cherche le mesh enfant qui contient le buffer SkinMatrix
            foreach (var child in linkedGroup)
            {
                if (em.HasBuffer<SkinMatrix>(child.Value))
                {
                    meshEntity = child.Value;
                    break;
                }
            }

            if (meshEntity == null)
            {
                Debug.LogWarning($"⚠ [SkinSystem] Rig entity {entity.Index} has no mesh child with SkinMatrix buffer!");
                continue;
            }

            // On travaille désormais sur le mesh
            var mesh = meshEntity.Value;

            var skinMatrices = em.GetBuffer<SkinMatrix>(mesh);
            var boneEntities = em.GetBuffer<BoneEntity>(entity);
            var bindPoses = em.GetBuffer<BindPose>(entity);
            var rootEntity = root.ValueRO.Value;

            if (!em.HasComponent<LocalToWorld>(rootEntity))
                continue;

            var rootL2W = em.GetComponentData<LocalToWorld>(rootEntity).Value;
            var invRoot = math.inverse(rootL2W);

            for (int i = 0; i < skinMatrices.Length && i < boneEntities.Length; i++)
            {
                var bone = boneEntities[i].Value;
                if (!em.HasComponent<LocalToWorld>(bone))
                    continue;

                var boneL2W = em.GetComponentData<LocalToWorld>(bone).Value;
                var bindPose = bindPoses[i].Value;
                var finalMatrix = math.mul(invRoot, math.mul(boneL2W, bindPose));

                skinMatrices[i] = new SkinMatrix
                {
                    Value = new float3x4(
                        finalMatrix.c0.xyz,
                        finalMatrix.c1.xyz,
                        finalMatrix.c2.xyz,
                        finalMatrix.c3.xyz
                    )
                };
            }

            Debug.Log($"✅ [SkinSystem] Updated {skinMatrices.Length} bones for mesh entity {mesh.Index} (rig {entity.Index})");
        }
    }
}
