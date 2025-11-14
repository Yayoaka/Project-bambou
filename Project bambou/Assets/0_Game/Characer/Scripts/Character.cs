using Unity.Netcode;
using UnityEngine;

public class Character : NetworkBehaviour
{
    [Header("Modules")]
    [SerializeField] private CharacterMovementController movement;
    [SerializeField] private CharacterAnimationController animationController;
    [SerializeField] private CharacterSkills skills;

    private Animator animator;

    [Header("Roll Settings")]
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollSpeedMultiplier = 2f;

    private bool isRolling;
    private float rollTimer;
    private float baseMoveSpeed;

    public override void OnNetworkSpawn()
    {
        if (IsOwner && movement != null)
        {
            baseMoveSpeed = movement.MoveSpeed;
        }
    }

    protected override void OnNetworkPostSpawn()
    {
        if (!IsOwner) return;
        base.OnNetworkPostSpawn();
        PlayerInputController.Instance.SetChampionCharacter(this);
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        if (movement != null && baseMoveSpeed == 0)
            baseMoveSpeed = movement.MoveSpeed;
    }

    private void Update()
    {
        if (!IsOwner || !isRolling) return;

        rollTimer -= Time.deltaTime;
        if (rollTimer <= 0f) EndRoll();
    }

    public void Move(Vector2 input)
    {
        if (!CanControl()) return;
        Debug.Log("Character :" + input.ToString());
        movement.Move(input);
    }

    public void StartRoll()
    {
        if (!CanControl()) return;

        isRolling = true;
        rollTimer = rollDuration;

        animator?.SetTrigger("roll");
        movement.MoveSpeed = baseMoveSpeed * rollSpeedMultiplier;
    }

    private void EndRoll()
    {
        isRolling = false;
        movement.MoveSpeed = baseMoveSpeed;
    }

    public void UseSkill(int skillIndex)
    {
        if (!IsOwner) return;
        skills?.UseSkill(skillIndex);
    }

    private bool CanControl()
    {
        return IsOwner && !isRolling && movement != null;
    }
}