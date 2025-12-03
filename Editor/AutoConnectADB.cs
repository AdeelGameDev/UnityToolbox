using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

[InitializeOnLoad]
public static class AutoConnectADB
{
    static AutoConnectADB()
    {
        // Runs automatically when Unity opens
        ConnectADB();
    }

    [MenuItem("Tools/Connect ADB Now")]
    public static void ConnectADB()
    {
        // Replace these with your own values in your local project
        string adbPath = @"PATH_TO_YOUR_ADB_EXE";
        string ipPort = "DEVICE_IP:5555";

        if (!File.Exists(adbPath))
        {
            UnityEngine.Debug.LogError("ADB not found at " + adbPath);
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = adbPath,
            Arguments = $"connect {ipPort}",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process proc = Process.Start(psi);
        proc.WaitForExit();

        string output = proc.StandardOutput.ReadToEnd();
        string error = proc.StandardError.ReadToEnd();

        UnityEngine.Debug.Log("ADB Output: " + output);
        if (!string.IsNullOrEmpty(error))
            UnityEngine.Debug.LogError("ADB Error: " + error);
    }
}
