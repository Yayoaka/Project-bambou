using System.Collections.Generic;
using UnityEngine;

namespace Enemies.Tick
{
    public class EnemyTickSystem : MonoBehaviour
    {
        public static EnemyTickSystem Instance { get; private set; }

        private readonly List<ITickable> _tickables = new();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Update()
        {
            var count = _tickables.Count;
            for (var i = 0; i < count; i++)
            {
                _tickables[i].Tick();
            }
        }

        public void Register(ITickable tickable)
        {
            _tickables.Add(tickable);
        }

        public void Unregister(ITickable tickable)
        {
            _tickables.Remove(tickable);
        }
    }
}
