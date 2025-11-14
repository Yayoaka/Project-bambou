using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;
using _2_Core.ECS.Animation;

namespace Enemies.Visual
{
    public class EnemyAnimDriverHybrid : MonoBehaviour
    {
        private Entity _ecsMeshEntity;
        private EntityManager _em;
        private Dictionary<string, Transform> _rigBones;

        public void LinkEcsMesh(Entity ecsEntity, EntityManager em)
        {
            _ecsMeshEntity = ecsEntity;
            _em = em;

            // Build bone lookup
            _rigBones = new Dictionary<string, Transform>(128);
            foreach (var tr in GetComponentsInChildren<Transform>())
                _rigBones[tr.name] = tr;

            Debug.Log($"✅ Linked rig {name} to ECS entity {_ecsMeshEntity.Index}");
        }

        private void LateUpdate()
        {
            if (_ecsMeshEntity == Entity.Null || !_em.Exists(_ecsMeshEntity))
                return;

            if (!_em.HasBuffer<BoneEntity>(_ecsMeshEntity) || !_em.HasBuffer<BoneName>(_ecsMeshEntity))
                return;

            var bones = _em.GetBuffer<BoneEntity>(_ecsMeshEntity);
            var names = _em.GetBuffer<BoneName>(_ecsMeshEntity);

            for (int i = 0; i < bones.Length; i++)
            {
                var boneEntity = bones[i].Value;
                if (!_em.Exists(boneEntity)) continue;

                var boneName = names[i].Value.ToString();
                if (!_rigBones.TryGetValue(boneName, out var tr)) continue;

                // Copy Unity transform to ECS
                _em.SetComponentData(boneEntity, new LocalToWorld
                {
                    Value = tr.localToWorldMatrix
                });
            }
        }
    }
}