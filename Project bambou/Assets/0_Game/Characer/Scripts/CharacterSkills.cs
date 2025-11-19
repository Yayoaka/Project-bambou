using UnityEngine;
using Unity.Netcode;

public class CharacterSkills : NetworkBehaviour
{
    // [SerializeField] private CharacterAnimationController animationController;
    [SerializeField] private GameObject fireballPrefab;
    
    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void UseSkill(int index)
    {
        if (!IsOwner) return;

        switch (index)
        {
            case 1: Skill1_Fireball(); break;
            case 2: Skill2(); break;
            case 3: Skill3(); break;
            default: Debug.LogWarning($"Unknown Skill: {index}"); break;
        }
    }

    private void Skill1_Fireball()
    {
        Debug.Log("Fireball Cast!");

        SpawnFireballRpc(transform.position, transform.forward, OwnerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnFireballRpc(Vector3 spawnPos, Vector3 direction, ulong clientId)
    {
        Debug.Log("Fireball Spawn!");
        GameObject obj = Instantiate(fireballPrefab, spawnPos, Quaternion.LookRotation(direction));

        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        
        netObj.SpawnAsPlayerObject(clientId);

        obj.GetComponent<FireballProjectile>().Initialize(direction);
    }
    
    private void Skill2()
    {
        Debug.Log("Skill2 used");
    }
    private void Skill3()
    {
        Debug.Log("Skill3 used");
    }
}