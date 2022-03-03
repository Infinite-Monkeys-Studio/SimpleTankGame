using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Nat;
using System;
using System.Net;
using System.Linq;

public class UPNPControl : MonoBehaviour
{
    [SerializeField] private int port = 7777;
    [SerializeField] private Protocol transportProtocol = Protocol.Udp;

    private static IPAddress ExternalIPAddress;
    private INatDevice device;
    private string textPort;
    private bool show = true;

    // Start is called before the first frame update
    void Start()
    {
        textPort = port.ToString();

        if (checkIfConnected())
        {
            Debug.Log("Connected.  Setting up UPnP");
            NatUtility.DeviceFound += DeviceFound;
            NatUtility.DeviceLost += DeviceLost;
            NatUtility.StartDiscovery();
        }
        else
        {
            Debug.LogError("Not connected to the internet");
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 210, 300, 300));
        if(show)
        {
            if (GUILayout.Button("Hide")) show = false;
            textPort = GUILayout.TextField(textPort);
            if (GUILayout.Button("Open UPnP")) TryOpenUPNP();
            if (GUILayout.Button("Close UPnP")) TryCloseUPNP();
            if (GUILayout.Button("Netork Info")) LogNetworkInfo();
        } else
        {
            if (GUILayout.Button("Show UPnP")) show = true;
        }        
        GUILayout.EndArea();
    }

    private void DeviceFound(object sender, DeviceEventArgs args)
    {
        device = args.Device;
        Debug.Log("UPnP Device found");        
    }

    private void TryCloseUPNP()
    {
        if (device == null)
        {
            Debug.LogError("No UPNP device found!");
            return;
        }

        if (!int.TryParse(textPort, out port))
        {
            Debug.LogWarning("Could not parse port.  Using default port");
        }

        if(device.GetSpecificMapping(transportProtocol, port).PublicPort == port)
        {
            Debug.Log("Port mapping found.  Removing...");
            device.DeletePortMap(new Mapping(transportProtocol, port, port));
        }
        
        Debug.Log("Could not find port mapping.");
    }

    private void TryOpenUPNP()
    {
        if(device == null)
        {
            Debug.LogError("No UPNP device found!");
            return;
        }

        if(!int.TryParse(textPort, out port))
        {
            Debug.LogWarning("Could not parse port.  Using default port");
        }

        if (device.GetSpecificMapping(transportProtocol, port).PublicPort == -1) //check that no map exists
        {
            Debug.Log($"created map for port {port}");
        }
        else if (device.GetSpecificMapping(transportProtocol, port).PublicPort == port)
        {
            Debug.Log($"port {port} already mapped, removing and replacing");
            device.DeletePortMap(new Mapping(transportProtocol, port, port));
        }

        device.CreatePortMap(new Mapping(transportProtocol, port, port));

        if (device.GetSpecificMapping(transportProtocol, port).PublicPort == port)
        {
            Debug.Log($"Port {port} open");
        }
        else
        {
            Debug.LogError($"Port not open correctly");
        }
    }

    private void LogNetworkInfo()
    {
        ExternalIPAddress = device.GetExternalIP();
        Debug.Log($"Public IP is: {device.GetExternalIP()}");
        //Debug.Log($"Coded public ip is: {codeIPAddress(device.GetExternalIP())}");

        var localIP = GetLocalIPAddress();
        Debug.Log($"Local IP is: {localIP}");
        //var codedIP = codeIPAddress(localIP);
        //Debug.Log($"Coded local ip is: {codedIP}");
        //var decodedIP = decodeIPAddress(codedIP);
        //Debug.Log($"decoded local ip is: {decodedIP}");
    }

    private void DeviceLost(object sender, DeviceEventArgs args)
    {
        device = null;
    }

    private void OnApplicationQuit()
    {
        TryCloseUPNP();
    }

    public static string GetPublicIPAddress()
    {
        if (ExternalIPAddress == null) return "No IP Available.";
        return ExternalIPAddress.ToString();
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    private bool checkIfConnected()
    {
        return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
    }
}
