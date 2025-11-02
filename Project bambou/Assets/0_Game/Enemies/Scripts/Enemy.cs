using UnityEngine;

namespace Enemies
{
    public sealed class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyStats baseStats;

        private EnemyContext _ctx;
        private IEnemyModule[] _modules;
        private IEnemyTick[] _tickables;
        
        public EnemyHealth Health { get; private set; }

        private void Awake()
        {
            _ctx = new EnemyContext(transform, baseStats, this);
            _modules = GetComponents<IEnemyModule>();
            _tickables = GetComponents<IEnemyTick>();
            foreach (var m in _modules) m.Setup(_ctx);
        }

        private void OnEnable()
        {
            EnemiesManager.Instance.Register(this);
            foreach (var m in _modules) m.OnActivated();
            // First classification on spawn/enable
            EnemiesManager.Instance.UpdateEnemyVisibility(this);
        }

        private void OnDisable()
        {
            foreach (var m in _modules) m.OnDeactivated();
            EnemiesManager.Instance.Unregister(this);
        }

        public void ApplyStats(EnemyStats stats) => _ctx.SetStats(stats);

        /// <summary>
        /// Tick all modules
        /// </summary>
        /// <param name="dt"></param>
        public void TickAll(float dt)
        {
            // Tick only modules that asked for ticking
            if (_tickables == null) return;
            foreach (var t in _tickables)
            {
                t?.Tick(dt);
            }
        }
    }
}