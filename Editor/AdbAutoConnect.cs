using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Reconnects to the last paired Android device when the editor starts, so
    /// wireless debugging needs no manual step at all once it has been set up.
    /// </summary>
    /// <remarks>
    /// Only runs when "Connect automatically when Unity starts" is enabled in the
    /// Wireless ADB window, and at most once per editor session. Failures stay
    /// quiet — a phone that is off or on another network is not an error worth a
    /// red console message.
    /// </remarks>
    [InitializeOnLoad]
    internal static class AdbAutoConnect
    {
        private const string AttemptedKey = "Adeel.UnityToolbox.Adb.AutoConnectAttempted";

        static AdbAutoConnect()
        {
            EditorApplication.delayCall += TryConnect;
        }

        private static void TryConnect()
        {
            if (!EditorPrefs.GetBool(AdbWirelessDebuggerWindow.Prefs.AutoConnect, false))
            {
                return;
            }

            // A domain reload would otherwise retry on every recompile.
            if (SessionState.GetBool(AttemptedKey, false))
            {
                return;
            }

            SessionState.SetBool(AttemptedKey, true);

            if (AdbWirelessDebuggerWindow.TryConnectOnStartup())
            {
                Debug.Log("Wireless ADB: reconnected automatically.");
            }
        }
    }
}
