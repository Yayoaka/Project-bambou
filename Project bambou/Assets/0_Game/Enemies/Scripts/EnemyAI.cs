using UnityEngine;

namespace Enemies
{
    public sealed class EnemyAI : MonoBehaviour, IEnemyModule, IEnemyTick
    {
        private EnemyContext _ctx;
        private EnemyMover _mover;

        private enum State { Idle, Chase }
        private State _state;

        public void Setup(EnemyContext ctx)
        {
            _ctx = ctx;
            _mover = GetComponent<EnemyMover>();
            _state = State.Idle;
        }

        public void OnActivated()
        {
            _state = State.Idle;
        }

        public void OnDeactivated() { }

        public void Tick(float dt)
        {
            // Minimal placeholder: keep still
        }
    }
}

