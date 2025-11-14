using Enemies.Visual;
using Unity.Entities;
using UnityEngine;
using _2_Core.ECS.Animation; // pour EnemyAnimationState si tu le gardes

namespace Enemies.Visual
{
    [DisallowMultipleComponent]
    public class EnemyAnimationAuthoring : MonoBehaviour
    {
        [Header("ECS Skin Prefab")]
        public GameObject ecsSkinPrefab;  // prefab avec EcsSkinAuthoring

        [Header("Rig/Animation")]
        public GameObject rigPrefab;      // GO rig (bones + Animator)
        public AnimationClip clip;
        public float speed = 1f;
    }

    public class EnemyAnimationBaker : Baker<EnemyAnimationAuthoring>
    {
        public override void Bake(EnemyAnimationAuthoring a)
        {
            var e = GetEntity(TransformUsageFlags.Renderable);

            // ECS entity reference for the skin prefab
            if (a.ecsSkinPrefab != null)
            {
                var ecsSkin = GetEntity(a.ecsSkinPrefab, TransformUsageFlags.Renderable);
                AddComponent(e, new EnemyVisualPrefab { EcsSkinPrefab = ecsSkin });
            }

            // Managed config: rig and animation settings
            AddComponentObject(e, new EnemyRigPrefabRef
            {
                RigPrefab = a.rigPrefab,
                Clip = a.clip,
                Speed = a.speed
            });

            // Optional simple state
            AddComponent(e, new EnemyAnimationState
            {
                Time = 0f,
                Speed = a.speed
            });
        }
    }
}