using GameState;
using GameState.States;
using SceneLoader;
using Steam;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private NetworkManager m_networkManagerPrefab;

    [SerializeField]
    private SteamLobbyManager m_steamLobbyManager;

    private NetworkManager m_networkManager;

    [Header("UIs")]
    [SerializeField]
    private Button m_hostButton;

    private void Start()
    {
        if (!NetworkManager.Singleton)
        {
            m_networkManager = Instantiate(m_networkManagerPrefab);
        }
        else
        {
            m_networkManager = NetworkManager.Singleton;
        }

        m_hostButton.onClick.AddListener(HandleHostButtonClicked);

        m_networkManager.OnClientStarted += OnClientStarted;
    }

    private void OnDestroy()
    {
        m_networkManager.OnClientStarted -= OnClientStarted;

        m_hostButton.onClick.RemoveListener(HandleHostButtonClicked);
    }

    private void HandleHostButtonClicked()
    {
        Debug.Log("Host Button Clicked (Steam)");

        // 1. Create Steam lobby
        m_steamLobbyManager.CreateLobby();
        // 2. StartHost() sera appelé dans OnLobbyCreated
    }

    private void OnClientStarted()
    {
        SceneLoaderManager.Instance.LoadSceneAsync(
            new LoadingContext(GameStateType.Lobby)
        );
    }
}