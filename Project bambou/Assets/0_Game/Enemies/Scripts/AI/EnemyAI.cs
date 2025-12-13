using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Enemies.Tick;
using Enemies.Lod;
using Enemies.Visual;
using Entity;
using Interfaces;
using Skills;
using Stats.Data;

namespace Enemies.AI
{
    public class EnemyAI : EntityComponent<EnemyBehaviour>, ITickable, ILODComponent
    {
        [Header("References")]
        [SerializeField] private NavMeshAgent nav;
        private EnemyMovement _movement;

        [Header("Targets")]
        [SerializeField] private List<Transform> targets = new();
        private Transform _currentTarget;

        [Header("LOD")]
        public ILODComponent.LodLevel lodLevel = ILODComponent.LodLevel.High;

        private float _attackRange = 2.2f;
        private float _attackSpeed = 3f;
        private float _attackDelay;

        private int _frameCount;
        private int _randomOffset;
        
        [Header("Target Override")]
        private Transform _forcedTarget;
        private float _forcedTargetEndTime;

        void Awake()
        {
            nav.avoidancePriority = 50;
            nav.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            nav.updateRotation = false;
            nav.updatePosition = false;

            _randomOffset = Random.Range(0, 8);
        }

        void Start()
        {
            EnemyLODSystem.Instance.Register(this);
            EnemyTickSystem.Instance.Register(this);

            _movement = GetComponent<EnemyMovement>();
        }

        new void OnDestroy()
        {
            if (EnemyLODSystem.Instance != null)
                EnemyLODSystem.Instance.Unregister(this);

            if (EnemyTickSystem.Instance != null)
                EnemyTickSystem.Instance.Unregister(this);
        }

        public override void LateInit()
        {
            base.LateInit();
            Owner.Stats.OnStatsChanged += UpdateStats;
        }

        private void OnEnable()
        {
            targets = PlayerCharacterManager.Characters.Select(x => x.transform).ToList();
            PlayerCharacterManager.OnPlayerSpawned += RegisterTarget;
            PlayerCharacterManager.OnPlayerUnspawned += UnregisterTarget;
        }

        private void OnDisable()
        {
            PlayerCharacterManager.OnPlayerSpawned -= RegisterTarget;
            PlayerCharacterManager.OnPlayerUnspawned -= UnregisterTarget;
        }
        
        private void RegisterTarget(GameObject target)
        {
            targets.Add(target.transform);
        }

        private void UnregisterTarget(GameObject target)
        {
            targets.Remove(target.transform);
        }

        // ----------------------------------------------------------
        // TICK
        // ----------------------------------------------------------
        public void Tick(float dt)
        {
            _frameCount++;

            ResolveClosestTarget();

            if (_currentTarget == null)
                return;

            if (IsInAttackRange())
            {
                TryAttack();
                return;
            }

            ApplyMovement(dt);

            if (ShouldRepath())
                TryRepath();
        }

        // ----------------------------------------------------------
        // TARGETING
        // ----------------------------------------------------------
        private void ResolveClosestTarget()
        {
            // Forced target has absolute priority
            if (_forcedTarget != null)
            {
                if (Time.time <= _forcedTargetEndTime && _forcedTarget != null)
                {
                    _currentTarget = _forcedTarget;
                    return;
                }

                // Taunt expired
                _forcedTarget = null;
            }

            if (targets == null || targets.Count == 0)
            {
                _currentTarget = null;
                return;
            }

            var bestSqrDist = float.MaxValue;
            Transform best = null;
            var pos = Position;

            for (var i = 0; i < targets.Count; i++)
            {
                var t = targets[i];
                if (t == null)
                    continue;

                var sqrDist = (t.position - pos).sqrMagnitude;
                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    best = t;
                }
            }

            _currentTarget = best;
        }

        private bool IsInAttackRange()
        {
            return (_currentTarget.position - Position).sqrMagnitude <= _attackRange * _attackRange;
        }
        
        public void ForceTarget(Transform target, float duration)
        {
            if (target == null)
                return;

            _forcedTarget = target;
            _forcedTargetEndTime = Time.time + duration;

            if (nav.isOnNavMesh)
                nav.SetDestination(target.position);
        }

        public void ClearForcedTarget()
        {
            _forcedTarget = null;
        }

        // ----------------------------------------------------------
        // ATTACK
        // ----------------------------------------------------------
        private void TryAttack()
        {
            if (_attackDelay > Time.time)
                return;

            _attackDelay = Time.time + _attackSpeed;

            var affectable = _currentTarget.GetComponent<IAffectable>();
            if (affectable == null)
                return;

            EffectExecutor.Execute(
                Owner.Data.effect,
                Owner.Stats,
                NetworkObjectId,
                affectable,
                transform.position
            );
        }

        // ----------------------------------------------------------
        // NAVIGATION
        // ----------------------------------------------------------
        private void TryRepath()
        {
            if (_currentTarget == null)
                return;

            if (!nav.isOnNavMesh)
                return;

            nav.SetDestination(_currentTarget.position);
        }

        private bool ShouldRepath()
        {
            var fc = _frameCount + _randomOffset;

            switch (lodLevel)
            {
                case ILODComponent.LodLevel.High:
                    return true;

                case ILODComponent.LodLevel.Medium:
                    return fc % 10 == 0;

                case ILODComponent.LodLevel.Low:
                    return fc % 40 == 0;
            }

            return true;
        }

        // ----------------------------------------------------------
        // MOVEMENT
        // ----------------------------------------------------------
        private void ApplyMovement(float dt)
        {
            nav.nextPosition = transform.position;

            var desired = nav.desiredVelocity;
            _movement.Move(desired, dt);

            if (desired.sqrMagnitude > 0.001f)
                _movement.Rotate(desired, dt);
        }

        // ----------------------------------------------------------
        // LOD
        // ----------------------------------------------------------
        public void SetLOD(ILODComponent.LodLevel level)
        {
            lodLevel = level;
        }

        public Vector3 Position => transform.position;

        // ----------------------------------------------------------
        // STATS
        // ----------------------------------------------------------
        private void UpdateStats()
        {
            nav.speed = Owner.Stats.GetStat(StatType.MoveSpeed);
            _attackSpeed = Owner.Stats.GetStat(StatType.AttackSpeed);
            _attackRange = Owner.Stats.GetStat(StatType.AttackRange);
        }
    }
}
