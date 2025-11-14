using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    private NetworkManager m_networkManagerPrefab;
    
    private NetworkManager m_networkManager;
    private UnityTransport m_transport;
    
    [Header("UIs")]
    [SerializeField]
    private TMP_InputField m_addressField;
    [SerializeField]
    private TMP_InputField m_portField;
    [SerializeField]
    private Button m_hostButton;
    [SerializeField]
    private Button m_clientButton;

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
        
        m_transport = m_networkManager.GetComponent<UnityTransport>();

        m_addressField.text = m_transport.ConnectionData.Address;
        m_portField.text = m_transport.ConnectionData.Port.ToString();

        
        m_hostButton.onClick.AddListener(HandleHostButtonClicked);
        m_clientButton.onClick.AddListener(HandleClientButtonClicked);

        m_networkManager.OnServerStarted += HandleServerStarted;
    }
    
    private void OnDestroy()
    {
        m_networkManager.OnServerStarted -= HandleServerStarted;
        
        m_hostButton.onClick.RemoveListener(HandleHostButtonClicked);
        m_clientButton.onClick.RemoveListener(HandleClientButtonClicked);
    }

    private void HandleHostButtonClicked()
    {
        Debug.Log("Host Button Clicked");
        
        m_transport.ConnectionData.Address = m_addressField.text;
        m_transport.ConnectionData.ServerListenAddress = m_addressField.text;
        m_transport.ConnectionData.Port = ushort.Parse(m_portField.text);

        m_networkManager.StartHost();
    }
    
    private void HandleClientButtonClicked()
    {
        Debug.Log("Client Button Clicked");

        m_transport.ConnectionData.Address = m_addressField.text;
        m_transport.ConnectionData.Port = ushort.Parse(m_portField.text);
        
        m_networkManager.StartClient();
    }
    
    private void HandleServerStarted()
    {
        if (!m_networkManager.IsServer)
            return;

        m_networkManager.SceneManager.LoadScene("CharacterScene", LoadSceneMode.Single);
    }
}
