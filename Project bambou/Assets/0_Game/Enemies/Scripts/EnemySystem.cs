using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Enemies.Data;
using Enemies.Visual;

namespace Enemies
{
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EnemySystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Chaque entité avec EnemyPrefabData (créée par le spawner)
            foreach (var (prefabData, enemyEntity) in SystemAPI.Query<RefRO<EnemyPrefabData>>().WithEntityAccess())
            {
                // 1️⃣ Récupère la configuration du prefab
                var prefabEntity = prefabData.ValueRO.Value;

                // 2️⃣ Copie la config du Scriptable (baked dans EnemyConfigData)
                if (state.EntityManager.HasComponent<EnemyConfigData>(prefabEntity))
                {
                    var cfg = state.EntityManager.GetComponentData<EnemyConfigData>(prefabEntity);
                    ecb.AddComponent(enemyEntity, cfg);
                }

                // 3️⃣ Copie la référence du visuel (pour instantiation plus tard)
                if (state.EntityManager.HasComponent<EnemyVisualPrefab>(prefabEntity))
                {
                    var visual = state.EntityManager.GetComponentData<EnemyVisualPrefab>(prefabEntity);
                    ecb.AddComponent(enemyEntity, visual);
                }

                // 4️⃣ Copie les autres data (health, movement, etc.)
                if (state.EntityManager.HasComponent<EnemyStateData>(prefabEntity))
                {
                    var stateData = state.EntityManager.GetComponentData<EnemyStateData>(prefabEntity);
                    ecb.AddComponent(enemyEntity, stateData);
                }

                // 5️⃣ Nettoie le tag pour ne pas rebaker
                ecb.RemoveComponent<EnemyPrefabData>(enemyEntity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}
