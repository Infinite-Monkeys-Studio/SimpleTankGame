using UnityEngine;
using System;

public class ConsoleToGUI : MonoBehaviour
{
    [SerializeField] int kChars = 700;
    [SerializeField] bool doShow = true;

    string myLog = "*begin log";
    string filename = "";

    void OnEnable() { Application.logMessageReceived += Log; }
    void OnDisable() { Application.logMessageReceived -= Log; }
    void Update() { if (Input.GetButtonDown("Console")) { doShow = !doShow; } }

    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        myLog = myLog + "\n" + logString;
        if (myLog.Length > kChars) { myLog = myLog.Substring(myLog.Length - kChars); }

        // for the file ...
        if (filename == "")
        {
            string d = Application.dataPath + "/Logs";
            System.IO.Directory.CreateDirectory(d);
            //string r = Random.Range(1000, 9999).ToString();
            string r = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss");
            filename = d + "/" + r + ".txt";
        }
        try { System.IO.File.AppendAllText(filename, logString + "\n"); }
        catch { }
    }

    void OnGUI()
    {
        if (!doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(new Vector3(0, Screen.height - 500, 0),
            Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), myLog);
    }
}
