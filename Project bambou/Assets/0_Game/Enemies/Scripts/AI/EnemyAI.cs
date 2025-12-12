using UnityEngine;
using UnityEngine.AI;
using Enemies.Tick;
using Enemies.Lod;
using Enemies.Visual;
using Entity;
using Stats.Data;

namespace Enemies.AI
{
    public class EnemyAI : EntityComponent<EnemyBehaviour>, ITickable, ILODComponent
    {
        [Header("References")]
        [SerializeField] private NavMeshAgent nav;
        private EnemyMovement _movement;
        private Transform _target;

        [Header("LOD")]
        public ILODComponent.LodLevel lodLevel = ILODComponent.LodLevel.High;

        private float _attackRange = 2.2f;
        
        private int _frameCount;
        private int _randomOffset;

        private float _attackSpeed = 3;

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

            var pl = GameObject.FindGameObjectWithTag("Player");
            if (pl != null)
                _target = pl.transform;

            if (_target != null)
            {
                nav.SetDestination(_target.position);
            }
        }

        new void OnDestroy()
        {
            if (EnemyLODSystem.Instance != null) EnemyLODSystem.Instance.Unregister(this);
            if (EnemyTickSystem.Instance != null) EnemyTickSystem.Instance.Unregister(this);
        }

        public override void LateInit()
        {
            base.LateInit();

            Owner.Stats.OnStatsChanged += UpdateSpeed;
        }

        // ----------------------------------------------------------
        // TICK
        // ----------------------------------------------------------
        public void Tick(float dt)
        {
            _frameCount++;

            if (_target == null)
                return;

            if (Vector3.Distance(_target.position, Position) < _attackRange)
            {
                //anim.SetAttack(_attackSpeed, dt);
                return;
            }
            
            ApplyMovement(dt);

            if (ShouldRepath())
                TryRepath();
        }

        // ----------------------------------------------------------
        // NAVIGATION
        // ----------------------------------------------------------
        void TryRepath()
        {
            if (_target == null)
                return;

            if (nav.isOnNavMesh) 
                nav.SetDestination(_target.position);
        }

        bool ShouldRepath()
        {
            var fc = _frameCount + _randomOffset;

            switch (lodLevel)
            {
                case ILODComponent.LodLevel.High: 
                    return true;

                case ILODComponent.LodLevel.Medium:
                    return (fc % 10 == 0);

                case ILODComponent.LodLevel.Low:
                    return (fc % 40 == 0);
            }

            return true;
        }

        // ----------------------------------------------------------
        // MOVEMENT
        // ----------------------------------------------------------
        void ApplyMovement(float dt)
        {
            nav.nextPosition = transform.position;
            
            var desired = nav.desiredVelocity;

            _movement.Move(desired, dt);

            if (desired.sqrMagnitude > 0.001f)
                _movement.Rotate(desired, dt);
        }

        // ----------------------------------------------------------
        // LOD SYSTEM
        // ----------------------------------------------------------
        public void SetLOD(ILODComponent.LodLevel level)
        {
            lodLevel = level;
        }

        public Vector3 Position => transform.position;

        private void UpdateSpeed()
        {
            nav.speed = Owner.Stats.GetStat(StatType.MoveSpeed);
        }
    }
}
