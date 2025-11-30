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
            Instance = this;
        }

        void Update()
        {
            var dt = Time.deltaTime;

            var count = _tickables.Count;
            for (var i = 0; i < count; i++)
                _tickables[i].Tick(dt);
        }

        public void Register(ITickable t) => _tickables.Add(t);
        public void Unregister(ITickable t) => _tickables.Remove(t);
    }
}