using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private Button m_startGameButton;
    [SerializeField] private Button m_readyButton;
    [SerializeField] private Button m_returnToMenuButton;

    [Header("Text")]
    [SerializeField] private TMP_Text m_readyButtonText;
    [SerializeField] private TMP_Text m_statusText;
    [SerializeField] private TMP_Text m_debugNumberPlayerText;

    private int m_currentCharacterIndex = 0;
    private bool m_isReady = false;
    private bool m_isHost = false;

    private void Start()
    {
        m_startGameButton?.onClick.AddListener(OnStartGameClicked);
        m_readyButton?.onClick.AddListener(OnReadyClicked);
        m_returnToMenuButton?.onClick.AddListener(OnReturnToMenuClicked);
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            InitializeUI();
        }
        else
        {
            Invoke(nameof(InitializeUI), 0.5f);
        }
    }

    private void InitializeUI()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            return;
        
        m_isHost = NetworkManager.Singleton.IsHost;
        
        UpdateUIForRole();
        UpdatePlayerList();
    }

    private void UpdateUIForRole()
    {
        if (m_startGameButton != null)
            m_startGameButton.gameObject.SetActive(m_isHost);

        if (m_readyButton != null)
            m_readyButton.gameObject.SetActive(!m_isHost);

        if (m_statusText != null)
        {
            m_statusText.text = m_isHost ? "Vous êtes l'hôte" : "En attente de l'hôte";
        }
    }

    public void UpdatePlayerList()
    {
        if (LobbyManager.Instance == null)
            return;

        var players = LobbyManager.Instance.GetAllPlayers();
        m_debugNumberPlayerText.text = $"Joueurs dans le lobby: {players.Count} / 4";
    }

    private void OnStartGameClicked()
    {
        if (!m_isHost || LobbyManager.Instance == null)
            return;

        if (LobbyManager.Instance.AreAllPlayersReady())
        {
            LobbyManager.Instance.StartGameServerRpc();
        }
        else
        {
            Debug.Log("Tous les joueurs ne sont pas prêts!");
            if (m_statusText != null)
            {
                m_statusText.text = "Tous les joueurs ne sont pas prêts!";
                Invoke(nameof(ResetStatusText), 2f);
            }
        }
    }
    
    private void OnReadyClicked()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
        {
            Debug.Log("LobbyManager found");
            var lobbyManager = FindFirstObjectByType<LobbyManager>();
            if (lobbyManager != null && lobbyManager.IsSpawned)
            {
                Debug.Log("LobbyManager spawned");
                lobbyManager.SetPlayerReadyServerRpc(
                    NetworkManager.Singleton.LocalClientId, 
                    true
                );
                UpdateReadyButtonText();
            }
        }
    }

    private void UpdateReadyButtonText()
    {
        if (m_readyButtonText != null)
        {
            m_readyButtonText.text = m_isReady ? "Annuler" : "Prêt";
        }
    }

    private void OnReturnToMenuClicked()
    {
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.ReturnToMainMenu();
        }
    }

    private void ResetStatusText()
    {
        if (m_statusText != null)
        {
            m_statusText.text = m_isHost ? "Vous êtes l'hôte" : "En attente de l'hôte";
        }
    }

    private void OnDestroy()
    {
        m_startGameButton?.onClick.RemoveListener(OnStartGameClicked);
        m_readyButton?.onClick.RemoveListener(OnReadyClicked);
        m_returnToMenuButton?.onClick.RemoveListener(OnReturnToMenuClicked);
    }
}