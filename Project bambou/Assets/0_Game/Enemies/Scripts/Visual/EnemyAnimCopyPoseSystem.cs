using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Enemies.Visual;
using _2_Core.ECS.Animation;
using Unity.Burst;

namespace Enemies.Visual
{
    // Tourne dans la présentation (main thread, accès GO)
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial class EnemyAnimCopyPoseSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var em = EntityManager;

            // On reste côté main thread (GO) => WithoutBurst + Run implicite avec SystemBase
            Entities
                .ForEach((Entity e, in EnemyVisualLink link) =>
                {
                    // 1) Récupère le rig Unity instancié pour CET ennemi
                    if (!em.HasComponent<EnemyRigInstanceRef>(e)) return;
                    var rigRef = em.GetComponentObject<EnemyRigInstanceRef>(e);
                    if (rigRef == null || rigRef.Rig == null) return;

                    // 2) Construit une map nom->Transform du rig (simple et robuste; à optimiser plus tard si besoin)
                    var map = new Dictionary<string, Transform>(128);
                    var trs = rigRef.Rig.GetComponentsInChildren<Transform>(true);
                    for (int i = 0; i < trs.Length; i++)
                        map[trs[i].name] = trs[i];

                    // 3) Récupère les buffers côté mesh ECS
                    if (!em.HasBuffer<BoneEntity>(link.SkinEntity)) return;
                    if (!em.HasBuffer<BoneName>(link.SkinEntity)) return;

                    var boneEntities = em.GetBuffer<BoneEntity>(link.SkinEntity);
                    var boneNames    = em.GetBuffer<BoneName>(link.SkinEntity);

                    var count = math.min(boneEntities.Length, boneNames.Length);
                    for (int i = 0; i < count; i++)
                    {
                        var name = boneNames[i].Value.ToString();
                        if (!map.TryGetValue(name, out var tr)) continue;

                        em.SetComponentData(boneEntities[i].Value, new LocalToWorld
                        {
                            Value = tr.localToWorldMatrix
                        });
                    }
                })
                .WithoutBurst()
                .Run();
        }
    }
}
