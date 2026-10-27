using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Connects the editor to a device over wireless ADB (<c>adb connect host:port</c>)
    /// without leaving Unity.
    /// </summary>
    /// <remarks>
    /// The ADB executable is auto-detected from <c>ANDROID_HOME</c>,
    /// <c>ANDROID_SDK_ROOT</c> and the standard SDK install locations. Both the
    /// path and the target device are stored in <see cref="EditorPrefs"/>, so they
    /// only need to be entered once.
    /// </remarks>
    public static class AutoConnectADB
    {
        private const string AdbPathKey = "Adeel.UnityToolbox.AutoConnectADB.AdbPath";
        private const string DeviceKey = "Adeel.UnityToolbox.AutoConnectADB.Device";

        internal const string MenuRoot = "Tools/Unity Toolbox/ADB/";

        /// <summary>Configured path to adb.exe, or the auto-detected one.</summary>
        internal static string AdbPath
        {
            get
            {
                string configured = EditorPrefs.GetString(AdbPathKey, string.Empty);

                if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
                {
                    return configured;
                }

                return DetectAdbPath();
            }
            set => EditorPrefs.SetString(AdbPathKey, value);
        }

        /// <summary>Configured <c>host:port</c> target, for example <c>192.168.1.42:5555</c>.</summary>
        internal static string Device
        {
            get => EditorPrefs.GetString(DeviceKey, string.Empty);
            set => EditorPrefs.SetString(DeviceKey, value);
        }

        [MenuItem(MenuRoot + "Connect Wireless Device", priority = 1)]
        internal static void Connect()
        {
            if (string.IsNullOrEmpty(Device))
            {
                EditorUtility.DisplayDialog(
                    "Unity Toolbox",
                    "No device configured yet. Open Tools > Unity Toolbox > ADB > Settings and enter the device IP and port.",
                    "OK");
                return;
            }

            Run($"connect {Device}");
        }

        [MenuItem(MenuRoot + "Disconnect All", priority = 2)]
        private static void DisconnectAll()
        {
            Run("disconnect");
        }

        [MenuItem(MenuRoot + "Restart ADB Server", priority = 3)]
        private static void RestartServer()
        {
            Run("kill-server");
            Run("start-server");
        }

        [MenuItem(MenuRoot + "Settings…", priority = 20)]
        private static void OpenSettings()
        {
            AdbSettingsWindow.ShowWindow();
        }

        private static void Run(string arguments)
        {
            string adbPath = AdbPath;

            if (string.IsNullOrEmpty(adbPath) || !File.Exists(adbPath))
            {
                Debug.LogError(
                    "AutoConnectADB: adb was not found. Set the path in Tools > Unity Toolbox > ADB > Settings.");
                return;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        Debug.LogError("AutoConnectADB: could not start the adb process.");
                        return;
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    // Never block the editor indefinitely if adb hangs.
                    if (!process.WaitForExit(5000))
                    {
                        Debug.LogWarning("AutoConnectADB: adb did not exit within 5 seconds.");
                        return;
                    }

                    if (!string.IsNullOrEmpty(output))
                    {
                        Debug.Log($"adb {arguments}\n{output.Trim()}");
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"adb {arguments}\n{error.Trim()}");
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"AutoConnectADB: failed to run adb — {exception.Message}");
            }
        }

        private static string DetectAdbPath()
        {
            string executable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";

            string[] roots =
            {
                Environment.GetEnvironmentVariable("ANDROID_HOME"),
                Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Android", "Sdk"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library", "Android", "sdk")
            };

            foreach (string root in roots)
            {
                if (string.IsNullOrEmpty(root))
                {
                    continue;
                }

                string candidate = Path.Combine(root, "platform-tools", executable);

                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }
    }
}
