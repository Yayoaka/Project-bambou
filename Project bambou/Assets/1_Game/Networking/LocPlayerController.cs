using UnityEngine;

public class LocPlayerController : MonoBehaviour
{
    public static LocPlayerController Instance { get; private set; }

    private PlayerInputController localPlayer;

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

    public void SetChampionCharacter(PlayerInputController player)
    {
        localPlayer = player;
    }
}