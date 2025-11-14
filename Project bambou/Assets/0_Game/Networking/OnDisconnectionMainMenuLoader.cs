using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnDisconnectionMainMenuLoader : MonoBehaviour
{
    [SerializeField]
    private NetworkManager m_networkManager;

    private void Start()
    {
        m_networkManager.OnClientStopped += HandleClientStopped;
    }

    private void HandleClientStopped(bool a_isHost)
    {
        Debug.Log($"[DISCONNECTION] Reason: {m_networkManager.DisconnectReason}"); 
        SceneManager.LoadSceneAsync("MainMenuScene");
    }
}
