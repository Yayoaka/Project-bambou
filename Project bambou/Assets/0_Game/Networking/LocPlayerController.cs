using Character.Input;
using UnityEngine;

public class LocPlayerController : MonoBehaviour
{
    public static LocPlayerController Instance { get; private set; }

    private CharacterInputController _localCharacter;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetChampionCharacter(CharacterInputController character)
    {
        _localCharacter = character;
    }
}