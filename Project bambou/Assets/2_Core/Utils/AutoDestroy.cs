using UnityEngine;

namespace Utils
{
    public class AutoDestroy : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1.5f;

        void OnEnable()
        {
            Invoke(nameof(DestroySelf), lifetime);
        }

        void DestroySelf()
        {
            Destroy(gameObject);
        }

        void OnDisable()
        {
            CancelInvoke();
        }
    }
}