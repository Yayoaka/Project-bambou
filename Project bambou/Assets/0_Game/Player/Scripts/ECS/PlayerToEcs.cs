using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player.ECS
{
    public class PlayerToEcs : MonoBehaviour
    {
        public int playerId;
        public float speed;

        private EntityManager _entityManager;
        private Entity _playerEntity;

        void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _playerEntity = _entityManager.CreateEntity(typeof(PlayerData));
        }

        void Update()
        {
            var pos = transform.position;

            _entityManager.SetComponentData(_playerEntity, new PlayerData
            {
                Id = playerId,
                Position = new float3(pos.x, pos.y, pos.z)
            });

            transform.position = new Vector3(pos.x + Random.Range(-1,1) * Time.deltaTime * speed, pos.y, pos.z + Random.Range(-1,1) * Time.deltaTime * speed);
        }
    }
}