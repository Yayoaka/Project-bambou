using Unity.Collections;
using Unity.Entities;

namespace Enemies.Visual
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct EnemyVisualCollectSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Crée un singleton pour stocker les entités à instancier
            var e = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PendingVisualEntity>(e);
        }

        public void OnUpdate(ref SystemState state)
        {
            var buffer = SystemAPI.GetSingletonBuffer<PendingVisualEntity>();
            buffer.Clear();

            foreach (var (visual, rigCfg, entity)
                     in SystemAPI.Query<EnemyVisualPrefab, EnemyRigPrefabRef>()
                         .WithNone<EnemyVisualLink>()
                         .WithEntityAccess())
            {
                buffer.Add(new PendingVisualEntity { Value = entity });
            }
        }
    }
}