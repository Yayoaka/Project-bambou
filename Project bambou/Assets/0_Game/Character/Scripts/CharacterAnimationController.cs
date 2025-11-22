using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int _hashSpeed;
    private int _hashSkillTrigger;
    private Vector2 _lastMoveInput = Vector2.zero;

    void Awake()
    {
        _hashSpeed = Animator.StringToHash("Speed");
        _hashSkillTrigger = Animator.StringToHash("Skill");
    }

    public void SetMoveInput(Vector2 move)
    {
        _lastMoveInput = move;
        float speed = move.magnitude;
        if (animator != null) animator.SetFloat(_hashSpeed, speed, 0.05f, Time.deltaTime);
    }
    
    public void TriggerRoll()
    {
        animator.SetTrigger("roll");
    }

    public void TriggerSkill(int index)
    {
        if (animator != null) animator.SetInteger(_hashSkillTrigger, index);
    }
}