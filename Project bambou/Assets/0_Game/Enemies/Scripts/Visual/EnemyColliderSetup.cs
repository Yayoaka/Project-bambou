using Entity;
using UnityEngine;

namespace Enemies.Visual
{
    public class EnemyColliderSetup : EntityComponent<EnemyBehaviour>
    {
        [SerializeField] private bool destroyAuthoringCollider = true;

        public void SetupColliderFromVisual()
        {
            var authoring = GetComponentInChildren<EnemyColliderAuthoring>();
            if (authoring == null)
            {
                Debug.LogWarning($"[EnemyColliderSetup] No ColliderAuthoring found on {name}");
                return;
            }

            var source = authoring.GetComponent<Collider>();
            if (source == null)
            {
                Debug.LogError($"[EnemyColliderSetup] ColliderAuthoring has no Collider on {name}");
                return;
            }

            CopyCollider(source);

            if (destroyAuthoringCollider)
                Destroy(source);
            else
                source.enabled = false;
        }

        private void CopyCollider(Collider source)
        {
            switch (source)
            {
                case CapsuleCollider capsule:
                    CopyCapsule(capsule);
                    break;

                case BoxCollider box:
                    CopyBox(box);
                    break;

                case SphereCollider sphere:
                    CopySphere(sphere);
                    break;

                default:
                    Debug.LogError($"[EnemyColliderSetup] Unsupported collider type {source.GetType()}");
                    break;
            }
        }

        private void CopyCapsule(CapsuleCollider src)
        {
            var dst = gameObject.GetComponent<CapsuleCollider>();

            var scale = src.transform.lossyScale;

            dst.center = Vector3.Scale(src.center, scale);
            dst.radius = src.radius * Mathf.Max(scale.x, scale.z);
            dst.height = src.height * scale.y;
            dst.direction = src.direction;
            dst.isTrigger = src.isTrigger;
        }

        private void CopyBox(BoxCollider src)
        {
            var dst = gameObject.GetComponent<BoxCollider>();
            dst.center = src.center;
            dst.size = src.size;
            dst.isTrigger = src.isTrigger;
        }

        private void CopySphere(SphereCollider src)
        {
            var dst = gameObject.GetComponent<SphereCollider>();
            dst.center = src.center;
            dst.radius = src.radius;
            dst.isTrigger = src.isTrigger;
        }
    }
}