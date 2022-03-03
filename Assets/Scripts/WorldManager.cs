using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private int defaultPort = 7777;
    private string ipAddress = "127.0.0.1";
    private string port = "7777";

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        ipAddress = GUILayout.TextField(ipAddress);
        port = GUILayout.TextField(port);
        if (GUILayout.Button("Client"))
        {
            int intPort;

            if (!int.TryParse(port, out intPort))
            {
                intPort = defaultPort;
            }

            var transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            transport.ConnectAddress = ipAddress;
            transport.ConnectPort = intPort;
            transport.ServerListenPort = intPort;
            NetworkManager.Singleton.StartClient();
        }
        //if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    static void StatusLabels()
    {
        var mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        GUILayout.Label("Transport: " +
            NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
        GUILayout.Label("Mode: " + mode);
        if(GUILayout.Button("Disconnect"))
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
