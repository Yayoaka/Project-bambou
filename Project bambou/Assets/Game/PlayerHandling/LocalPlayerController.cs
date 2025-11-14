using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalPlayerController : MonoBehaviour
{
    #region Singleton
    public static LocalPlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    #endregion
    
    
    private InputSystem_Actions m_actions;
    
    
    // Setter pour le champion
    private PlayerInputController m_Character;

    public void SetChampionCharacter(PlayerInputController a_Character)
    {
        m_Character = a_Character;
    }
    
    
    void Start()
    {
        m_actions = new InputSystem_Actions();
        m_actions.Enable();
        
        m_actions.Player.Interact.started += HandleInteractStarted;
    }

    private void Update()
    {
        if (!m_Character)
            return;
        
        // On récupère et envoie l'input du movement au character.
        var moveInput = m_actions.Player.Move.ReadValue<Vector2>();
        //m_Character.OnMove(moveInput);
    }

    private void HandleInteractStarted(InputAction.CallbackContext a_obj)
    {
        Debug.Log("Interact started");
    }
}
