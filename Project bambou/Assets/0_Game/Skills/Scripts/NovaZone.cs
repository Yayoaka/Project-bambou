using UnityEngine;
using Unity.Netcode;
using System.Collections;
using Character;

public class NovaZone : NetworkBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int damage = 25;

    private ulong ownerId;

    public void Initialize(ulong ownerClientId)
    {
        ownerId = ownerClientId;
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(duration);
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent<CharacterBehaviour>(out var character))
        {
            if (character.NetworkObject.OwnerClientId == ownerId) return;
            
            Debug.Log($"Nova hit {character.name} for {damage} damage");
        }
    }
}