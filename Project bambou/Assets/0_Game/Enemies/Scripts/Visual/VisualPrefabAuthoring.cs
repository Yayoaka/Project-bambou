using Unity.Entities;
using UnityEngine;

namespace Enemies.Visual
{
    public class VisualPrefabAuthoring : MonoBehaviour
    {
        class Baker : Baker<VisualPrefabAuthoring>
        {
            public override void Bake(VisualPrefabAuthoring authoring)
            {
                GetEntity(TransformUsageFlags.Renderable | TransformUsageFlags.Dynamic);
            }
        }
    }
}