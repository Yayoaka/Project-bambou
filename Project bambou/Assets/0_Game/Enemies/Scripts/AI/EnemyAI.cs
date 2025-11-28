using UnityEngine;
using UnityEngine.AI;
using Enemies.Tick;
using Enemies.Lod;

namespace Enemies.AI
{
    public class EnemyAI : MonoBehaviour, ITickable, ILODComponent
    {
        [Header("References")]
        [SerializeField] private NavMeshAgent nav;
        private EnemyMovement _movement;
        private Transform _target;

        [Header("LOD")]
        public ILODComponent.LodLevel lodLevel = ILODComponent.LodLevel.High;

        private Vector3 _lastTargetPos;
        private int _frameCounter;
        private int _randomOffset;

        private const float RepathDistance = 0.5f; // Minimum movement before recalculating path

        void Awake()
        {
            nav.updateRotation = false;
            nav.updatePosition = false;

            _randomOffset = Random.Range(0, 5);
        }

        void Start()
        {
            EnemyTickSystem.Instance.Register(this);

            _movement = GetComponent<EnemyMovement>();

            var obj = GameObject.FindWithTag("Player");
            if (obj != null)
                _target = obj.transform;

            if (_target != null)
                _lastTargetPos = Vector3.positiveInfinity;
        }

        void OnDestroy()
        {
            if (EnemyTickSystem.Instance != null)
                EnemyTickSystem.Instance.Unregister(this);
        }

        // ------------------------------------------------------------
        // TICK
        // ------------------------------------------------------------
        public void Tick()
        {
            //DEBUG
            if(Vector3.Distance(_lastTargetPos, transform.position) < 2)
                Destroy(this.gameObject);
            
            _frameCounter++;

            if (!ShouldTick()) 
                return;

            if (_target == null) 
                return;

            TryRepath();
            ApplyMovement();
        }

        // ------------------------------------------------------------
        // NAVIGATION
        // ------------------------------------------------------------
        void TryRepath()
        {
            // Avoid too frequent SetDestination
            var dist = (_target.position - _lastTargetPos).sqrMagnitude;
            if (dist < RepathDistance * RepathDistance)
                return;

            // LOD throttling + random offset for desync
            if (!LODAllowsRepath())
                return;

            nav.SetDestination(_target.position);
            _lastTargetPos = _target.position;
        }

        bool LODAllowsRepath()
        {
            var mod = _randomOffset;

            switch (lodLevel)
            {
                case ILODComponent.LodLevel.High:
                    return true;

                case ILODComponent.LodLevel.Medium:
                    return (_frameCounter + mod) % 10 == 0;

                case ILODComponent.LodLevel.Low:
                    return (_frameCounter + mod) % 50 == 0;

                default:
                    return true;
            }
        }

        // ------------------------------------------------------------
        // PHYSICS DRIVEN MOVEMENT
        // ------------------------------------------------------------
        void ApplyMovement()
        {
            var desired = nav.desiredVelocity;
            _movement.Move(desired);

            if (desired.sqrMagnitude > 0.01f)
                _movement.Rotate(desired);
            
            nav.nextPosition = transform.position;
        }

        // ------------------------------------------------------------
        // LOD
        // ------------------------------------------------------------
        public void SetLOD(ILODComponent.LodLevel level)
        {
            lodLevel = level;
            _frameCounter = 0;
        }

        bool ShouldTick()
        {
            switch (lodLevel)
            {
                case ILODComponent.LodLevel.High:
                    return true;

                case ILODComponent.LodLevel.Medium:
                    return (_frameCounter % 2 == 0);  // Half frequency
                case ILODComponent.LodLevel.Low:
                    return (_frameCounter % 5 == 0);  // Low frequency
            }

            return true;
        }
    }
}
