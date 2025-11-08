using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Enemies.Visual
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class EnemyAnimDriverSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (state, driverRef, boneNames, entity) in
                     SystemAPI.Query<RefRW<EnemyAnimationState>, EnemyAnimDriverRef, DynamicBuffer<BoneName>>()
                              .WithEntityAccess())
            {
                if (driverRef.Instance != null)
                    continue;

                var go = Object.Instantiate(driverRef.RigPrefab);
                go.hideFlags = HideFlags.HideAndDontSave;

                foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;

                var animator = go.GetComponentInChildren<Animator>(true);
                Animation animation = null;

                if (animator != null)
                {
                    var overrideCtrl = new AnimatorOverrideController(animator.runtimeAnimatorController);
                    if (overrideCtrl.animationClips.Length > 0)
                        overrideCtrl[overrideCtrl.animationClips[0].name] = driverRef.Clip;

                    animator.runtimeAnimatorController = overrideCtrl;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animator.updateMode = AnimatorUpdateMode.Normal;
                }
                else
                {
                    animation = go.GetComponentInChildren<Animation>(true) ?? go.AddComponent<Animation>();
                    animation.playAutomatically = false;
                    animation.wrapMode = WrapMode.Loop;

                    if (animation.GetClip(driverRef.Clip.name) == null)
                        animation.AddClip(driverRef.Clip, driverRef.Clip.name);

                    animation.clip = driverRef.Clip;
                    animation.Play();
                }

                var driverBones = new Transform[boneNames.Length];
                var allTransforms = go.GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < boneNames.Length; i++)
                {
                    var name = boneNames[i].Value.ToString();
                    driverBones[i] = allTransforms.FirstOrDefault(t => t.name == name);
                    if (driverBones[i] == null)
                        Debug.LogWarning($"[ECS Anim] Os introuvable dans le driver: {name}");
                }

                driverRef.Instance = go;
                driverRef.Animator = animator;
                driverRef.DriverBones = driverBones;
            }

            ecb.Playback(EntityManager);
        }
    }
}