using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Modules")]
    public CharacterMovementController movement;
    public CharacterAnimationController animationController;
    public CharacterSkills skills;
    private Animator animator;

    [Header("Roll Settings")]
    public float rollDuration = 0.5f;
    public float rollSpeedMultiplier = 2f;

    private bool isRolling = false;
    private float rollTimer = 0f;

    private void Awake()
    {
        if(animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }    
    }
    
    public void Move(Vector2 input)
    {
        if (!isRolling && movement != null)
        {
            movement.Move(input);
        }
    }

    public void StartRoll()
    {
        if (isRolling) return; 

        isRolling = true;
        rollTimer = rollDuration;

        animator?.SetTrigger("roll");

        if (movement != null)
        {
            movement.moveSpeed *= rollSpeedMultiplier;
        }
    }

    private void Update()
    {
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                EndRoll();
            }
        }
    }

    private void EndRoll()
    {
        isRolling = false;

        if (movement != null)
        {
            movement.moveSpeed /= rollSpeedMultiplier;
        }
    }

    public void UseSkill1() => skills?.UseSkill(1);
    public void UseSkill2() => skills?.UseSkill(2);
    public void UseSkill3() => skills?.UseSkill(3);
}