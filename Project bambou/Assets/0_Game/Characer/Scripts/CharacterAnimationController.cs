using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    Animator animator;
    int hashSpeed;
    int hashIsDashing;
    int hashSkillTrigger;
    Vector2 lastMoveInput = Vector2.zero;

    void Awake()
    {
        animator = GetComponent<Animator>();
        hashSpeed = Animator.StringToHash("Speed");
        hashIsDashing = Animator.StringToHash("IsDashing");
        hashSkillTrigger = Animator.StringToHash("Skill");
    }

    public void SetMoveInput(Vector2 move)
    {
        lastMoveInput = move;
        float speed = move.magnitude;
        if (animator != null) animator.SetFloat(hashSpeed, speed, 0.05f, Time.deltaTime);
    }
    
    public void TriggerRoll()
    {
        animator.SetTrigger("roll");
    }

    public void TriggerSkill(int index)
    {
        if (animator != null) animator.SetInteger(hashSkillTrigger, index);
    }
}