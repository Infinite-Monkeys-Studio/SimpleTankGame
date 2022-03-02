using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Nat;
using System;
using System.Net;
using System.Linq;

public class UPNPControl : MonoBehaviour
{
    [SerializeField] private int defaultPort = 7777;
    [SerializeField] private Protocol transportProtocol = Protocol.Udp;
    [SerializeField] private TextAsset textJSON;
    private INatDevice device;

    [System.Serializable]
    private class WordList
    {
        public string[] codeWords;
    }

    private WordList wordList;

    // Start is called before the first frame update
    void Start()
    {
        if (checkIfConnected())
        {
            wordList = JsonUtility.FromJson<WordList>(textJSON.text);
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

    private void DeviceFound(object sender, DeviceEventArgs args)
    {
        device = args.Device;

        if(device.GetSpecificMapping(transportProtocol, defaultPort).PublicPort == -1) //check that no map exists
        {
            Debug.Log($"created map for port {defaultPort}");
        } else if(device.GetSpecificMapping(transportProtocol, defaultPort).PublicPort == defaultPort)
        {
            Debug.Log($"port {defaultPort} already mapped, removing and replacing");
            device.DeletePortMap(new Mapping(transportProtocol, defaultPort, defaultPort));
        }

        device.CreatePortMap(new Mapping(transportProtocol, defaultPort, defaultPort));


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
        if(device != null)
            device.DeletePortMap(new Mapping(transportProtocol, defaultPort, defaultPort));
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

    private string codeIPAddress(string address)
    {
        return codeIPAddress(IPAddress.Parse(address));
    }

    private string codeIPAddress(IPAddress address)
    {
        string output = "";
        foreach (var item in address.GetAddressBytes())
        {
            output += wordList.codeWords[item] + " ";
        }

        return output.Trim();
    }

    private string decodeIPAddress(string words)
    {
        string address = "";
        words = words.ToLower();
        foreach (var item in words.Split(' '))
        {
            int index = wordList.codeWords.ToList().IndexOf(item);
            address += index + ".";
        }

        return address.Remove(address.Length - 1, 1);
    }
}
