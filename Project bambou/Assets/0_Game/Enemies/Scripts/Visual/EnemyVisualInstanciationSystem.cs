using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Enemies.Visual
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class EnemyVisualInstantiationSystem : SystemBase
    {
        private readonly List<(Entity entity, GameObject rig)> _pending = new();

        protected override void OnUpdate()
        {
            var em = EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var buffer = SystemAPI.GetSingletonBuffer<PendingVisualEntity>(true);
            if (buffer.Length == 0)
                return;

            foreach (var entry in buffer)
            {
                var entity = entry.Value;
                if (!em.HasComponent<EnemyRigPrefabRef>(entity) ||
                    !em.HasComponent<EnemyVisualPrefab>(entity))
                    continue;

                var xform = em.GetComponentData<LocalTransform>(entity);
                var visual = em.GetComponentData<EnemyVisualPrefab>(entity);
                var rigCfg = em.GetComponentObject<EnemyRigPrefabRef>(entity);

                var skinEntity = em.Instantiate(visual.EcsSkinPrefab);
                em.SetComponentData(skinEntity, xform);
                em.AddComponentData(skinEntity, new Parent { Value = entity });
                
                ecb.SetComponent(skinEntity, LocalTransform.FromPositionRotationScale(
                    float3.zero, quaternion.identity, 1f));

                GameObject rig = null;
                if (rigCfg.RigPrefab != null)
                {
                    rig = Object.Instantiate(rigCfg.RigPrefab);
                    rig.name = $"EnemyRig_{entity.Index}";
                    rig.transform.position = xform.Position;

                    var animator = rig.GetComponent<Animator>();
                    if (animator != null && rigCfg.Clip != null)
                    {
                        animator.speed = rigCfg.Speed <= 0f ? 1f : rigCfg.Speed;
                        animator.Play(rigCfg.Clip.name, 0, 0f);
                    }

                    var driver = rig.GetComponent<EnemyAnimDriverHybrid>();
                    if (driver != null)
                        driver.LinkEcsMesh(skinEntity, em);

                    _pending.Add((entity, rig));
                }

                ecb.AddComponent(entity, new EnemyVisualLink { SkinEntity = skinEntity });
            }

            ecb.Playback(em);
            ecb.Dispose();

            foreach (var (entity, rig) in _pending)
                em.AddComponentObject(entity, new EnemyRigInstanceRef { Rig = rig });

            _pending.Clear();
        }
    }
}
