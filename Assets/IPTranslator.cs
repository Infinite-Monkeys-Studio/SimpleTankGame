using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Linq;

public class IPTranslator : MonoBehaviour
{
    [SerializeField] private TextAsset textJSON;

    [System.Serializable]
    private class WordList
    {
        public string[] codeWords;
    }

    private static WordList wordList;

    void Start()
    {
        wordList = JsonUtility.FromJson<WordList>(textJSON.text);
    }

    private static string codeIPAddress(string address)
    {
        return codeIPAddress(IPAddress.Parse(address));
    }

    private static string codeIPAddress(IPAddress address)
    {
        string output = "";
        foreach (var item in address.GetAddressBytes())
        {
            output += wordList.codeWords[item] + " ";
        }

        return output.Trim();
    }

    private static string decodeIPAddress(string words)
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
