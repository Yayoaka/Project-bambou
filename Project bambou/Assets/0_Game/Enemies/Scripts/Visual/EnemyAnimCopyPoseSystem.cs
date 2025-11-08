using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Enemies.Visual
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    public partial class EnemyAnimCopyPoseSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = SystemAPI.Time.DeltaTime;
            var entityManager = EntityManager;

            foreach (var (animState, driverRef, boneEntities) in
                     SystemAPI.Query<RefRW<EnemyAnimationState>, EnemyAnimDriverRef, DynamicBuffer<BoneEntity>>())
            {
                if (driverRef.Instance == null ||
                    driverRef.Clip == null ||
                    driverRef.DriverBones == null ||
                    driverRef.DriverBones.Length == 0)
                    continue;

                animState.ValueRW.Time += dt * driverRef.Speed;

                var clipLen = driverRef.Clip.length;
                var t = clipLen > 0f ? animState.ValueRW.Time % clipLen : animState.ValueRW.Time;

                driverRef.Clip.SampleAnimation(driverRef.Instance, t);

                var driverBones = driverRef.DriverBones;
                var count = math.min(boneEntities.Length, driverBones.Length);

                for (int i = 0; i < count; i++)
                {
                    var boneEntity = boneEntities[i].Value;
                    var tr = driverBones[i];
                    if (tr == null || !entityManager.HasComponent<LocalTransform>(boneEntity))
                        continue;

                    var lt = entityManager.GetComponentData<LocalTransform>(boneEntity);
                    lt.Position = tr.localPosition;
                    lt.Rotation = tr.localRotation;
                    lt.Scale = tr.localScale.x;
                    entityManager.SetComponentData(boneEntity, lt);
                }
            }
        }
    }
}