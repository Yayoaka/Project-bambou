using UnityEngine;
using Unity.Netcode;

public class CharacterSkills : NetworkBehaviour
{
    // [SerializeField] private CharacterAnimationController animationController;

    public void UseSkill(int index)
    {
        if (!IsOwner) return;

        switch (index)
        {
            case 1: Skill1(); break;
            case 2: Skill2(); break;
            case 3: Skill3(); break;
            default: Debug.LogWarning($"Unknown Skill: {index}"); break;
        }
    }

    private void Skill1()
    {
        Debug.Log("Skill1 used");
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