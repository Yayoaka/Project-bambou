using UnityEngine;

public class CharacterSkills : MonoBehaviour
{
    //private CharacterAnimationController anim;

    private void Awake()
    {
        //anim = GetComponent<CharacterAnimationController>();
    }

    public void UseSkill(int index)
    {
        switch (index)
        {
            case 1:
                Debug.Log($"{name} used Skill 1");
                break;
            case 2:
                Debug.Log($"{name} used Skill 2");
                break;
            case 3:
                Debug.Log($"{name} used Skill 3");
                break;
            default:
                Debug.Log($"{name} used Unknown skill {index}");
                break;
        }

        //anim?.TriggerSkill(index);
    }
}