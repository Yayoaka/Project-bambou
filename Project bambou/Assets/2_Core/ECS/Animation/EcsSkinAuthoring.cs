using Enemies.Visual;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace _2_Core.ECS.Animation
{
    [DisallowMultipleComponent]
    public class EcsSkinAuthoring : MonoBehaviour
    {
        public SkinnedMeshRenderer skinned;
    }

    public class EcsSkinBaker : Baker<EcsSkinAuthoring>
    {
        public override void Bake(EcsSkinAuthoring a)
        {
            if (a.skinned == null)
            {
                Debug.LogError($"[{nameof(EcsSkinBaker)}] Missing SkinnedMeshRenderer on {a.name}");
                return;
            }

            // --- Déclare une dépendance sur le mesh et les bones ---
            DependsOn(a.skinned);
            DependsOn(a.skinned.sharedMesh);
            foreach (var bone in a.skinned.bones)
                DependsOn(bone);

            // --- Crée l’entité du rig ---
            var rigEntity = GetEntity(TransformUsageFlags.Dynamic);

            // --- Crée l’entité du mesh ---
            var meshEntity = GetEntity(a.skinned.gameObject, TransformUsageFlags.Renderable);

            // --- Bones ---
            var bones = a.skinned.bones;
            var boneBuf = AddBuffer<BoneEntity>(rigEntity);
            var bindBuf = AddBuffer<BindPose>(rigEntity);
            var nameBuf = AddBuffer<BoneName>(rigEntity);
            boneBuf.ResizeUninitialized(bones.Length);
            bindBuf.ResizeUninitialized(bones.Length);
            nameBuf.ResizeUninitialized(bones.Length);

            for (int i = 0; i < bones.Length; i++)
            {
                boneBuf[i] = new BoneEntity { Value = GetEntity(bones[i], TransformUsageFlags.Dynamic) };
                bindBuf[i] = new BindPose { Value = a.skinned.sharedMesh.bindposes[i] };
                nameBuf[i] = new BoneName { Value = bones[i].name };
            }

            // --- Root ---
            AddComponent(rigEntity, new RootEntity { Value = GetEntity(a.skinned.rootBone, TransformUsageFlags.Dynamic) });

            // --- Skin matrices sur le mesh ---
            var skinBuf = AddBuffer<SkinMatrix>(meshEntity);
            skinBuf.ResizeUninitialized(bones.Length);
            for (int i = 0; i < bones.Length; i++)
                skinBuf[i] = new SkinMatrix { Value = float3x4.zero };

            // --- LinkedEntityGroup ---
            var linked = AddBuffer<LinkedEntityGroup>(rigEntity);
            linked.Add(new LinkedEntityGroup { Value = rigEntity });
            linked.Add(new LinkedEntityGroup { Value = meshEntity });

            // --- Rendu ECS ---
            var material = a.skinned.sharedMaterial;
            var mesh = a.skinned.sharedMesh;
            if (material == null || mesh == null)
            {
                Debug.LogError($"[{nameof(EcsSkinBaker)}] Missing material or mesh on {a.name}");
                return;
            }

            var renderMeshArray = new RenderMeshArray(new[] { material }, new[] { mesh });
            AddSharedComponentManaged(meshEntity, renderMeshArray);
            AddComponent(meshEntity, new MaterialMeshInfo { Material = 0 });
            AddComponent(meshEntity, new RenderBounds { Value = a.skinned.localBounds.ToAABB() });

            // --- Tag pour le calcul du skin ---
            AddComponent<CalculateSkinMatrixTag>(rigEntity);

            Debug.Log($"✅ [EcsSkinBaker] Linked rig {rigEntity.Index} ↔ mesh {meshEntity.Index}");
        }
    }
}
