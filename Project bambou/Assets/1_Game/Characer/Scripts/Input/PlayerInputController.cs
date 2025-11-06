using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private Vector2 currentInput;
    private Character character;
    
    private void Awake()
    {
        character = GetComponent<Character>();
    }
    
    private void Update()
    {
        character.Move(currentInput);
    }

    private void OnMove(InputValue value)
    {
        currentInput = value.Get<Vector2>();
        //Debug.Log("OnMove: " + currentInput);
    }

    private void OnRoll()
    {
        character.StartRoll();
    }

    private void OnSkill1()
    {
        character.UseSkill1();
    }

    private void OnSkill2()
    {
        character.UseSkill2();
    }

    private void OnSkill3()
    {
        character.UseSkill3();
    }
}