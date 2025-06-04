using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MultiplayerHostAndClientLogin : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject mainPanel;

    [Header("Buttons")]
    public Button hostButton;
    public Button clientButton;
    public TextMeshProUGUI localIPText;

    [Header("Client Panel")]
    public GameObject clientPanel;
    public TMP_InputField clientIPInputField;
    public Button clientEnterGameButton;
    public Button clientExitButton;

    [Header("Connection Settings")]
    public bool autoConnectToLocalHost; // True = use local IP directly, False = show input field
    private static string targetIp;

    const ushort DefaultPort = 7777;

    void Start()
    {
        ResetIpInputFieldVisual();

        hostButton?.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsListening) return;

            StartHost();
            if (localIPText != null)
                localIPText.text = "Local IP: " + GetLocalIPAddress();

            mainPanel.SetActive(false);
        });

        clientButton?.onClick.AddListener(() =>
        {
            if (autoConnectToLocalHost)
            {
                StartClientWithIp(GetLocalIPAddress());
            }
            else
            {
                ShowClientIpInput();
            }
        });

        clientEnterGameButton?.onClick.AddListener(() =>
        {
            StartClient();
        });

        clientExitButton?.onClick.AddListener(() =>
        {
            clientPanel.SetActive(false);
            mainPanel.SetActive(true);
            ResetIpInputFieldVisual();
        });

        // Handle connection failures
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.LogError("Client failed to connect or was disconnected.");
                ShowConnectionError();
                clientPanel.SetActive(true);
                mainPanel.SetActive(false);
            }
        };
    }

    #region Host
    public void StartHost()
    {
        SetTransportIpAndPort(GetLocalIPAddress(), DefaultPort);
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started on IP: " + GetLocalIPAddress());
    }
    #endregion

    #region Client
    private void ShowClientIpInput()
    {
        clientPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void StartClient()
    {
        targetIp = clientIPInputField?.text.Trim();

        if (!IsValidIp(targetIp))
        {
            ShowInvalidIpError();
            return;
        }

        StartClientWithIp(targetIp);
    }

    private void StartClientWithIp(string ip)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Already connected.");
            return;
        }

        SetTransportIpAndPort(ip, DefaultPort);

        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client started with IP: " + ip);
            clientPanel.SetActive(false);
            mainPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Failed to start client.");
            ShowConnectionError();
        }
    }
    #endregion

    #region Networking Helpers
    void SetTransportIpAndPort(string ip, ushort port)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.ConnectionData.Address = ip;
            transport.ConnectionData.Port = port;
            Debug.Log($"Transport configured with IP: {ip}, Port: {port}");
        }
        else
        {
            Debug.LogError("UnityTransport component not found on NetworkManager.");
        }
    }

    string GetLocalIPAddress()
    {
        foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (host.AddressFamily == AddressFamily.InterNetwork)
                return host.ToString();
        }
        return "Unavailable";
    }

    bool IsValidIp(string ipString)
    {
        return IPAddress.TryParse(ipString, out _);
    }
    #endregion

    #region UI Helpers
    void ResetIpInputFieldVisual()
    {
        var placeholderText = clientIPInputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter IP";
        placeholderText.color = Color.gray;

        var colors = new ColorBlock
        {
            normalColor = Color.white,
            highlightedColor = new Color(0.78f, 0.78f, 0.78f),
            pressedColor = new Color(0.60f, 0.60f, 0.60f),
            selectedColor = Color.white,
            disabledColor = new Color(0.50f, 0.50f, 0.50f),
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };
        clientIPInputField.colors = colors;
    }

    void ShowInvalidIpError()
    {
        clientIPInputField.text = "";
        var placeholder = clientIPInputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholder.text = "Enter Correct IP";
        placeholder.color = Color.red;
    }

    void ShowConnectionError()
    {
        clientIPInputField.text = "";
        var placeholder = clientIPInputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholder.text = "Connection failed";
        placeholder.color = Color.red;
    }
    #endregion
}
