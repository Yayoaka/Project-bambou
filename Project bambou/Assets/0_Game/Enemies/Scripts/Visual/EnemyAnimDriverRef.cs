using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Enemies.Visual
{
    // Pure ECS: which ECS skin prefab to instantiate for this enemy
    public struct EnemyVisualPrefab : IComponentData
    {
        public Entity EcsSkinPrefab; // baked from a GameObject via GetEntity(...)
    }

    // Managed config baked from authoring (rig prefab + anim)
    public class EnemyRigPrefabRef : IComponentData
    {
        public GameObject RigPrefab;
        public AnimationClip Clip;
        public float Speed;
    }

    // Managed instance stored at runtime (the instantiated rig GO)
    public class EnemyRigInstanceRef : IComponentData
    {
        public GameObject Rig; // null until instantiated
    }

    // Link to the instantiated ECS skin entity
    public struct EnemyVisualLink : IComponentData
    {
        public Entity SkinEntity;
    }
    
    public struct BoneName : IBufferElementData
    {
        public FixedString64Bytes Value;
    }

    public struct EnemyAnimationState : IComponentData
    {
        public float Time;
        public float Speed;
    }
}