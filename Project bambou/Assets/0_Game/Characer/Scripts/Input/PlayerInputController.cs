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

        controls.Player.Skill1.started += (InputAction.CallbackContext ctx) =>
        {
            TriggerSkill(1);
        };
        
        controls.Player.Skill2.started += (InputAction.CallbackContext ctx) =>
        {
            TriggerSkill(2);
        };
        
        controls.Player.Skill3.started += (InputAction.CallbackContext ctx) =>
        {
            TriggerSkill(3);
        };
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