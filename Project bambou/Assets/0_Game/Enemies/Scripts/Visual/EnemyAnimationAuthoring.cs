using _2_Core.ECS.Animation;
using Enemies.Visual;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Enemies
{
    [DisallowMultipleComponent]
    public class EnemyAnimationAuthoring : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public GameObject rigPrefab;
        public AnimationClip clip;
        public float speed = 1f;
    }

    public class EnemyAnimationBaker : Baker<EnemyAnimationAuthoring>
    {
        public override void Bake(EnemyAnimationAuthoring a)
        {
            if (a.skinnedMeshRenderer == null || a.rigPrefab == null || a.clip == null)
            {
                Debug.LogWarning($"[{nameof(EnemyAnimationBaker)}] Missing references on {a.name}");
                return;
            }
            
            var renderEntity = GetEntity(TransformUsageFlags.Renderable);
            
            AddComponent(renderEntity, new EnemyAnimationState { Time = 0f, Speed = a.speed });
            
            AddComponent<CalculateSkinMatrixTag>(renderEntity);

            var bones = a.skinnedMeshRenderer.bones;
            var bindPoses = a.skinnedMeshRenderer.sharedMesh.bindposes;

            var boneEntityBuf = AddBuffer<BoneEntity>(renderEntity);
            boneEntityBuf.ResizeUninitialized(bones.Length);
            for (int i = 0; i < bones.Length; i++)
                boneEntityBuf[i] = new BoneEntity { Value = GetEntity(bones[i], TransformUsageFlags.Dynamic) };

            var bindPoseBuf = AddBuffer<BindPose>(renderEntity);
            bindPoseBuf.ResizeUninitialized(bindPoses.Length);
            for (int i = 0; i < bindPoses.Length; i++)
                bindPoseBuf[i] = new BindPose { Value = bindPoses[i] };

            var boneNameBuf = AddBuffer<BoneName>(renderEntity);
            boneNameBuf.ResizeUninitialized(bones.Length);
            for (int i = 0; i < bones.Length; i++)
                boneNameBuf[i] = new BoneName { Value = bones[i].name };

            AddComponent(renderEntity, new RootEntity
            {
                Value = GetEntity(a.skinnedMeshRenderer.rootBone, TransformUsageFlags.Dynamic)
            });
            
            AddComponentObject(renderEntity, new EnemyAnimDriverRef
            {
                RigPrefab = a.rigPrefab,
                Clip = a.clip,
                Speed = a.speed
            });
           
            var mesh = a.skinnedMeshRenderer.sharedMesh;
            var material = a.skinnedMeshRenderer.sharedMaterial;

            if (mesh != null && material != null)
            {
                var renderArray = new RenderMeshArray(new[] { material }, new[] { mesh });

                AddComponent(renderEntity, new MaterialMeshInfo
                {
                    MaterialID = new BatchMaterialID(),
                    MeshID = new BatchMeshID()
                });

                AddSharedComponentManaged(renderEntity, renderArray);
            }
            else
            {
                Debug.LogWarning($"[{nameof(EnemyAnimationBaker)}] {a.name} missing mesh or material on SkinnedMeshRenderer.");
            }
        }
    }
}
