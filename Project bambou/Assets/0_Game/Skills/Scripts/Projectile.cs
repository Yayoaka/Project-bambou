using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 3f;

    private Vector3 direction;
    private float timer;

    public void Initialize(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        if (!IsSpawned) return;

        if (IsServer)
        {
            transform.position += direction * speed * Time.deltaTime;

            timer += Time.deltaTime;
            if (timer >= lifetime)
            {
                GetComponent<NetworkObject>().Despawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if(!other.CompareTag("Enemy")) return;
        
        if (other.TryGetComponent(out NetworkObject netObj))
        {
            if (netObj.OwnerClientId == OwnerClientId)
                return;
        }

        GetComponent<NetworkObject>().Despawn();
    }
}