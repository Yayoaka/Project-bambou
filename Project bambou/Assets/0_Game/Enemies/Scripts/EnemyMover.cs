using UnityEngine;

namespace Enemies
{
    public sealed class EnemyMover : MonoBehaviour, IEnemyModule, IEnemyTick
    {
        private EnemyContext _ctx;
        private Vector3 _dir;

        public void Setup(EnemyContext ctx)
        {
            _ctx = ctx;
            _dir = Vector3.zero;
        }

        public void OnActivated() { }
        public void OnDeactivated() { }

        public void SetDirection(Vector3 dir)
        {
            _dir = dir;
        }

        public void Tick(float dt)
        {
            var t = _ctx.Transform;
            var delta = _dir * (_ctx.Stats.moveSpeed * dt);
            t.position += delta;
        }
    }
}