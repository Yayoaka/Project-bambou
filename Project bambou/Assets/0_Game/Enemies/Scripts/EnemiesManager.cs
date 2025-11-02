using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;

namespace Enemies
{
    public sealed class EnemiesManager : MonoBehaviour
    {
        #region Global Variables
        
        [Header("Ranges")]
        [SerializeField] private float visibleRange = 25f;
        [SerializeField] private float closeRange = 50f;

        [Header("Strides (frames)")]
        [SerializeField] private int visibleStride = 4;
        [SerializeField] private int closeStride = 10;
        [SerializeField] private int farStride = 20;

        #endregion

        #region Private Variables
        
        private readonly List<Enemy> _entities = new();
        private readonly List<Enemy> _visibleEntities = new();
        private readonly List<Enemy> _closeEntities = new();
        private readonly List<Enemy> _farEntities = new();

        private readonly List<PlayerEntity> _players = new();
        private int _frameIndex;

        #endregion

        #region Getters and Setters

        public List<PlayerEntity> GetPlayers => _players;

        #endregion

        #region Events

        public static event Action<PlayerEntity> OnPlayerSpawned;

        #endregion

        #region Singleton

        public static EnemiesManager Instance;

        private void Awake()
        {
            if(Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        #endregion

        #region List Registration
        
        public void Register(Enemy e)
        {
            _entities.Add(e);
            _visibleEntities.Add(e);
        }
        
        public void Unregister(Enemy e)
        {
            _entities.Remove(e);
            _visibleEntities.Remove(e);
            _closeEntities.Remove(e);
            _farEntities.Remove(e);
        }

        public void RegisterPlayer(PlayerEntity player)
        {
            _players.Add(player);
            OnPlayerSpawned?.Invoke(player);
        }
        
        #endregion

        private void Update()
        {
            var dt = Time.deltaTime;

            // Tick all visible entities
            foreach (var enemy in _visibleEntities)
                enemy.TickAll(dt);

            // Less often Tick all close but not visible entities
            if (_frameIndex % visibleStride == 0)
            {
                var scaledDt = dt * visibleStride;
                foreach (var enemy in _closeEntities)
                    enemy.TickAll(scaledDt);
            }

            // Rare Tick all far entities
            if (_frameIndex % farStride == 0)
            {
                var scaledDt = dt * farStride;
                foreach (var enemy in _farEntities)
                    enemy.TickAll(scaledDt);
            }

            // Progressive visibility re-eval (don’t scan everything every frame)
            for (var i = _frameIndex % closeStride; i < _entities.Count; i += closeStride)
                UpdateEnemyVisibility(_entities[i]);

            _frameIndex++;
        }

        public void UpdateEnemyVisibility(Enemy e)
        {
            var pos = e.transform.position;
            var sqr = GetClosestPlayerSqrDistance(pos);

            // Move enemy to the appropriate bucket
            if (sqr <= visibleRange * visibleRange) MoveToList(e, _visibleEntities);
            else if (sqr <= closeRange * closeRange) MoveToList(e, _closeEntities);
            else MoveToList(e, _farEntities);
        }
        
        private float GetClosestPlayerSqrDistance(Vector3 p)
        {
            return _players.Select(t => (t.transform.position - p)).Select(d => d.sqrMagnitude).Prepend(float.MaxValue).Min();
        }

        private void MoveToList(Enemy e, List<Enemy> list)
        {
            _visibleEntities.Remove(e);
            _closeEntities.Remove(e);
            _farEntities.Remove(e);
            list.Add(e);
        }

        #if UNITY_EDITOR
        
        private void OnDrawGizmos()
        {
            foreach (var player in _players)
            {
                DrawCircleGizmo(player.transform.position, visibleRange, color: Color.green);
                DrawCircleGizmo(player.transform.position, closeRange, color: Color.red);
            }
        }
        
        private static void DrawCircleGizmo(Vector3 center, float radius, int segments = 32, Color color = default)
        {
            var step = Mathf.PI * 2f / segments;
            Vector3 prevPoint = center + new Vector3(Mathf.Cos(0), 0f, Mathf.Sin(0)) * radius;
            for (int i = 1; i <= segments; i++)
            {
                var angle = step * i;
                var nextPoint = center + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Gizmos.color = color;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
        
        #endif
    }
}
