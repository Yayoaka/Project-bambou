using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public static PlayerInputController Instance { get; private set; }
    
    private Vector2 moveInput;
    private Character character;
    
    private PlayerControls controls;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        
        controls = new PlayerControls();
    }
    
    public void SetChampionCharacter(Character player)
    {
        character = player;
    }

    private void Start()
    {
        controls.Enable();
        
        controls.Player.Skill1.started += HandleSkill1Started;
    }

    private void HandleSkill1Started(InputAction.CallbackContext obj)
    {
        TriggerSkill(1);
    }

    private void OnDestroy()
    {
        controls.Dispose();
    }

    private void Update()
    {
        
        if (!CanControl()) return;
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        character.Move(moveInput);    
    }

    public void OnMove(InputValue value)
    {
        
        if (!CanControl()) return;
        moveInput = value.Get<Vector2>();
    }

    private void OnRoll()
    {
        if (!CanControl()) return;
        character.StartRoll();
    }

    private void OnSkill1()
    {
        TriggerSkill(1);
    }

    private void OnSkill2()
    { 
        TriggerSkill(2);
    }

    private void OnSkill3()
    {
        TriggerSkill(3);
    }

    private void TriggerSkill(int index)
    {
        if (!CanControl()) return;
        character.UseSkill(index);
    }

    private bool CanControl()
    {
        return character != null;
    }
}