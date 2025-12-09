using Unity.Netcode;
using UnityEngine;

namespace Collectibles
{
    public class PlayerCollector : NetworkBehaviour
    {
        private static readonly Collider[] buffer = new Collider[64];

        [Header("Collect Settings")]
        [SerializeField] private float collectRange = 3f;
        [SerializeField] private LayerMask collectibleMask;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            if (!IsServer) return;

            int hits = Physics.OverlapSphereNonAlloc(
                _transform.position,
                collectRange,
                buffer,
                collectibleMask
            );

            for (int i = 0; i < hits; i++)
            {
                Collider col = buffer[i];
                if (!col) continue;

                if (col.TryGetComponent(out MagneticCollectible collectible))
                {
                    collectible.StartMagnet(_transform);
                }
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
            Gizmos.DrawSphere(transform.position, collectRange);
        }
#endif
    }
}