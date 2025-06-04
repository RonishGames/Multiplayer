using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MultiplayeHostAndClintLogin : MonoBehaviour
{
    [Header("maniPannel")]
    public GameObject ButtonsPanel;
    [Header("Buttons")]
    public Button hostButton;
    public Button clientButton;
    public TextMeshProUGUI localIPText;

    [Header("Clint Pannel")]
    public GameObject ClintPannel;
    public TMP_InputField ClintIPInputField;
    public Button ClintEnterPlayGameButton;
    public Button ClintExitGameButton;

    static string targetIp;

    void Start()
    {
        hostButton?.onClick.AddListener(() =>
        {
            StartHost();
            localIPText.text = "Local IP: " + GetLocalIPAddress();
            ButtonsPanel.SetActive(false);
        });

        clientButton?.onClick.AddListener(getClintInput);

        ClintEnterPlayGameButton?.onClick.AddListener(() =>
        {
            StartClient();
            // Only hide the panels if StartClient actually succeeds.
            // (We’ll hide inside StartClient once we know IP is valid.)
        });

        ClintExitGameButton?.onClick.AddListener(() =>
        {
            ClintPannel.SetActive(false);
            ButtonsPanel.SetActive(true);
            ResetIpInputFieldVisual();
        });
    }

    #region Host 
    public void StartHost()
    {
        SetIpInGame(GetLocalIPAddress());
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");
    }
    #endregion

    #region Client
    private void getClintInput()
    {
        ClintPannel.SetActive(true);
    }

    public void StartClient()
    {
        // Read the text from the input field
        targetIp = ClintIPInputField.text.Trim();

        // Validate before configuring transport/starting client
        if (!IsValidIp(targetIp))
        {
            // Invalid: turn the field red and show a placeholder message
            ClintIPInputField.text = "";
            ClintIPInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter Correct IP";
            ClintIPInputField.placeholder.GetComponent<TextMeshProUGUI>().color = Color.red;
            var colors = ClintIPInputField.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.pressedColor = Color.white;
            ClintIPInputField.colors = colors;
            return;
        }

        // If valid, reset visual in case it was red before
        ResetIpInputFieldVisual();

        try
        {
            SetIpInGame(targetIp);
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
                ClintPannel.SetActive(false);
                ButtonsPanel.SetActive(false);
            }
            else
            {
                Debug.LogError("NetworkManager.StartClient() returned false.");
                ShowConnectionError();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to start client: " + e.Message);
            ShowConnectionError();
        }
    }
    #endregion

    void SetIpInGame(string ip)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport != null)
        {
            transport.ConnectionData.Address = ip;
            transport.ConnectionData.Port = 7777; // Default port, can be changed
            Debug.Log("Transport configured with IP: " + ip);
        }
        else
        {
            Debug.LogError("UnityTransport component not found on NetworkManager.");
        }
    }

    string GetLocalIPAddress()
    {
        string localIP = "Not Available";
        foreach (var host in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (host.AddressFamily == AddressFamily.InterNetwork)
            {
                localIP = host.ToString();
                break;
            }
        }
        return localIP;
    }

    // Helper: simple IP address validation (IPv4). 
    // If you also want to allow hostnames, replace with Uri.CheckHostName(...) != Unknown.
    bool IsValidIp(string ipString)
    {
        return IPAddress.TryParse(ipString, out _);
    }

    // Reset input-field colors and placeholder to defaults
    void ResetIpInputFieldVisual()
    {
        // Reset placeholder text color & content
        var placeholderText = ClintIPInputField.placeholder.GetComponent<TextMeshProUGUI>();
        placeholderText.text = "Enter IP";
        placeholderText.color = Color.gray;

        // Reset color block to Unity’s default values
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
        ClintIPInputField.colors = colors;
    }

    // If StartClient fails, let the player know
    void ShowConnectionError()
    {
        ClintIPInputField.text = "";
        ClintIPInputField.placeholder.GetComponent<TextMeshProUGUI>().text = "Connection failed";
        ClintIPInputField.placeholder.GetComponent<TextMeshProUGUI>().color = Color.red;
        var colors = ClintIPInputField.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.pressedColor = Color.white;
        ClintIPInputField.colors = colors;
    }
}
