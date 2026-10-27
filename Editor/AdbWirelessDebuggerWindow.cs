using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// One window for the whole Android 11+ wireless-debugging flow: discover the
    /// device, pair it with the six-digit code, and connect — no terminal, no
    /// copying IP addresses.
    /// </summary>
    /// <remarks>
    /// While the phone's "Pair device with pairing code" dialog is open the device
    /// advertises a temporary pairing port over mDNS. This window reads that port
    /// and the connect port automatically, so the only thing the user types is the
    /// code shown on the phone.
    /// </remarks>
    internal class AdbWirelessDebuggerWindow : EditorWindow
    {
        /// <summary>EditorPrefs keys shared with <see cref="AdbCommandRunner"/> and <see cref="AdbAutoConnect"/>.</summary>
        internal static class Prefs
        {
            internal const string AdbPath = "Adeel.UnityToolbox.Adb.Path";
            internal const string Ip = "Adeel.UnityToolbox.Adb.Ip";
            internal const string PairingPort = "Adeel.UnityToolbox.Adb.PairingPort";
            internal const string ConnectPort = "Adeel.UnityToolbox.Adb.ConnectPort";
            internal const string AutoConnect = "Adeel.UnityToolbox.Adb.AutoConnect";
        }

        private const string ScannedKey = "Adeel.UnityToolbox.Adb.ScannedThisSession";
        private const int MaxLogLength = 12000;

        private static readonly StringBuilder LogBuilder = new StringBuilder();

        private string _ip = string.Empty;
        private string _pairingPort = string.Empty;
        private string _connectPort = string.Empty;
        private string _pairingCode = string.Empty;
        private string _adbPath = string.Empty;
        private string _discoverySummary = string.Empty;
        private bool _autoConnect;
        private Vector2 _logScroll;

        [MenuItem("Tools/Unity Toolbox/ADB/Wireless Debugging…", priority = 1)]
        internal static void ShowWindow()
        {
            AdbWirelessDebuggerWindow window = GetWindow<AdbWirelessDebuggerWindow>("Wireless ADB");
            window.minSize = new Vector2(500f, 460f);
            window.Show();
        }

        [MenuItem("Tools/Unity Toolbox/ADB/Connect Now", priority = 2)]
        private static void ConnectNow()
        {
            AdbMdnsServices services = AdbCommandRunner.DiscoverServices();
            string ip = !string.IsNullOrEmpty(services.IpAddress)
                ? services.IpAddress
                : EditorPrefs.GetString(Prefs.Ip, string.Empty);
            int port = services.HasConnect
                ? services.ConnectPort
                : ParsePort(EditorPrefs.GetString(Prefs.ConnectPort, string.Empty));

            if (string.IsNullOrEmpty(ip) || port <= 0)
            {
                Debug.LogWarning(
                    "Wireless ADB: no device found. Open Tools > Unity Toolbox > ADB > Wireless Debugging to pair one.");
                return;
            }

            AdbResult result = AdbCommandRunner.Run($"connect {ip}:{port}");

            if (AdbCommandRunner.OutputIndicatesSuccess(result, "connect"))
            {
                Debug.Log($"Wireless ADB: connected to {ip}:{port}");
            }
            else
            {
                Debug.LogWarning($"Wireless ADB: could not connect to {ip}:{port}.{Environment.NewLine}{result.Combined}");
            }
        }

        [MenuItem("Tools/Unity Toolbox/ADB/Disconnect All", priority = 3)]
        private static void DisconnectAll()
        {
            AdbResult result = AdbCommandRunner.Run("disconnect");
            Debug.Log($"Wireless ADB: disconnect.{Environment.NewLine}{result.Combined}");
        }

        [MenuItem("Tools/Unity Toolbox/ADB/Restart ADB Server", priority = 4)]
        private static void RestartServer()
        {
            AdbCommandRunner.Run("kill-server");
            AdbResult result = AdbCommandRunner.Run("start-server");
            Debug.Log($"Wireless ADB: server restarted.{Environment.NewLine}{result.Combined}");
        }

        /// <summary>
        /// Connects to the last known device using mDNS to refresh the port. Used by
        /// <see cref="AdbAutoConnect"/> when the editor starts.
        /// </summary>
        internal static bool TryConnectOnStartup()
        {
            AdbMdnsServices services = AdbCommandRunner.DiscoverServices();

            string ip = !string.IsNullOrEmpty(services.IpAddress)
                ? services.IpAddress
                : EditorPrefs.GetString(Prefs.Ip, string.Empty);

            int port = services.HasConnect
                ? services.ConnectPort
                : ParsePort(EditorPrefs.GetString(Prefs.ConnectPort, string.Empty));

            if (string.IsNullOrEmpty(ip) || port <= 0)
            {
                return false;
            }

            AdbResult result = AdbCommandRunner.Run($"connect {ip}:{port}");

            if (!AdbCommandRunner.OutputIndicatesSuccess(result, "connect"))
            {
                return false;
            }

            // Remember the freshly discovered port so the next session is instant.
            EditorPrefs.SetString(Prefs.Ip, ip);
            EditorPrefs.SetString(Prefs.ConnectPort, port.ToString());
            return true;
        }

        private void OnEnable()
        {
            _ip = EditorPrefs.GetString(Prefs.Ip, string.Empty);
            _pairingPort = EditorPrefs.GetString(Prefs.PairingPort, string.Empty);
            _connectPort = EditorPrefs.GetString(Prefs.ConnectPort, string.Empty);
            _autoConnect = EditorPrefs.GetBool(Prefs.AutoConnect, false);
            _adbPath = AdbCommandRunner.AdbPath;

            // Only scan once per editor session; a scan is cheap but not free.
            if (!SessionState.GetBool(ScannedKey, false))
            {
                SessionState.SetBool(ScannedKey, true);
                EditorApplication.delayCall += RefreshDiscovery;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "1.  Phone: Developer options → Wireless debugging → On\n" +
                "2.  Phone: tap \"Pair device with pairing code\" and leave that dialog open\n" +
                "3.  Type the six-digit code here and press Pair & Connect",
                MessageType.Info);

            DrawDeviceFields();
            EditorGUILayout.Space();
            DrawActions();
            EditorGUILayout.Space();
            DrawAutoConnect();
            EditorGUILayout.Space();
            DrawLog();
            EditorGUILayout.Space();
            DrawAdbPath();
        }

        private void DrawDeviceFields()
        {
            EditorGUILayout.LabelField("Device", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                _ip = EditorGUILayout.TextField("IP address", _ip);

                if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
                {
                    RefreshDiscovery();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _pairingPort = EditorGUILayout.TextField("Pairing port", _pairingPort);
                _pairingCode = EditorGUILayout.TextField("Code", _pairingCode, GUILayout.Width(150f));
            }

            _connectPort = EditorGUILayout.TextField("Connect port", _connectPort);

            if (!string.IsNullOrEmpty(_discoverySummary))
            {
                EditorGUILayout.LabelField("Discovered via mDNS", _discoverySummary, EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Nothing discovered yet — press Refresh with the pairing dialog open.",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                bool canPair = !string.IsNullOrEmpty(_ip)
                               && !string.IsNullOrEmpty(_pairingPort)
                               && CleanDigits(_pairingCode).Length == 6;

                using (new EditorGUI.DisabledScope(!canPair))
                {
                    if (GUILayout.Button("Pair & Connect", GUILayout.Height(26f)))
                    {
                        PairAndConnect();
                    }
                }

                if (GUILayout.Button("Connect Only", GUILayout.Height(26f), GUILayout.Width(120f)))
                {
                    ConnectInternal();
                }
            }

            EditorGUILayout.LabelField(
                "Pairing is needed once per device. After that, \"Connect Only\" is enough — and the code is never needed again.",
                EditorStyles.miniLabel);
        }

        private void DrawAutoConnect()
        {
            bool autoConnect = EditorGUILayout.Toggle(
                new GUIContent(
                    "Connect automatically when Unity starts",
                    "Finds the device over mDNS and connects on editor load. Needs no code once the device is paired."),
                _autoConnect);

            if (autoConnect != _autoConnect)
            {
                _autoConnect = autoConnect;
                EditorPrefs.SetBool(Prefs.AutoConnect, _autoConnect);
            }
        }

        private void DrawLog()
        {
            EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Show Devices", GUILayout.Width(110f)))
                {
                    ShowDevices();
                }

                if (GUILayout.Button("Clear", GUILayout.Width(60f)))
                {
                    LogBuilder.Clear();
                }
            }

            _logScroll = EditorGUILayout.BeginScrollView(_logScroll, GUILayout.Height(140f));
            EditorGUILayout.SelectableLabel(
                LogBuilder.Length == 0 ? "(nothing yet)" : LogBuilder.ToString(),
                EditorStyles.label,
                GUILayout.ExpandHeight(true),
                GUILayout.ExpandWidth(true));
            EditorGUILayout.EndScrollView();
        }

        private void DrawAdbPath()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _adbPath = EditorGUILayout.TextField("adb path", _adbPath);

                if (GUILayout.Button("Browse", GUILayout.Width(70f)))
                {
                    string selected = EditorUtility.OpenFilePanel("Select adb", string.Empty, string.Empty);

                    if (!string.IsNullOrEmpty(selected))
                    {
                        _adbPath = selected;
                        AdbCommandRunner.AdbPath = selected;
                        AdbCommandRunner.ClearCache();
                        Append($"Using adb: {selected}");
                    }
                }

                if (GUILayout.Button("Auto-detect", GUILayout.Width(90f)))
                {
                    AdbCommandRunner.AdbPath = string.Empty;
                    AdbCommandRunner.ClearCache();
                    _adbPath = AdbCommandRunner.AdbPath;
                    Append(string.IsNullOrEmpty(_adbPath)
                        ? "Could not find adb automatically. Install Android SDK platform-tools or browse for it."
                        : $"Detected adb: {_adbPath}");
                }
            }
        }

        private void RefreshDiscovery()
        {
            AdbMdnsServices services = AdbCommandRunner.DiscoverServices();

            if (!string.IsNullOrEmpty(services.IpAddress))
            {
                _ip = services.IpAddress;
            }

            if (services.HasPairing)
            {
                _pairingPort = services.PairingPort.ToString();
            }

            if (services.HasConnect)
            {
                _connectPort = services.ConnectPort.ToString();
            }

            if (services.HasPairing || services.HasConnect)
            {
                _discoverySummary = services.ToString();
                Append($"Discovered {services}");
            }
            else
            {
                _discoverySummary = string.Empty;
                Append(
                    "No mDNS services found. Make sure wireless debugging is on and the pairing dialog is open, " +
                    "then press Refresh. If your network blocks mDNS, type the ports from the phone by hand.");

                // Surface why the call itself failed (usually a missing adb).
                string hint = AdbCommandRunner.LastHint;

                if (!string.IsNullOrEmpty(hint))
                {
                    Append(hint);
                }
            }

            SaveDeviceFields();
            Repaint();
        }

        private void PairAndConnect()
        {
            string code = CleanDigits(_pairingCode);

            if (code.Length != 6)
            {
                Append("Enter the six-digit pairing code shown on the phone.");
                return;
            }

            // The pairing port only lives while the dialog is open, so re-read it now.
            AdbMdnsServices services = AdbCommandRunner.DiscoverServices();

            if (services.HasPairing)
            {
                _pairingPort = services.PairingPort.ToString();
            }

            if (!string.IsNullOrEmpty(services.IpAddress))
            {
                _ip = services.IpAddress;
            }

            if (services.HasConnect && string.IsNullOrEmpty(_connectPort))
            {
                _connectPort = services.ConnectPort.ToString();
            }

            SaveDeviceFields();

            if (string.IsNullOrEmpty(_ip) || string.IsNullOrEmpty(_pairingPort))
            {
                Append("Pairing port unknown. Open \"Pair device with pairing code\" on the phone and press Refresh.");
                return;
            }

            // Keep the code out of the log.
            AdbResult pair = AdbCommandRunner.Run($"pair {_ip}:{_pairingPort} {code}");
            Append(AdbCommandRunner.Describe($"pair {_ip}:{_pairingPort} ******", pair));

            if (!AdbCommandRunner.OutputIndicatesSuccess(pair, "pair"))
            {
                Append(
                    "Pairing failed. The port and code must come from the dialog that is open right now — " +
                    "both change every time it is opened.");
                return;
            }

            _pairingCode = string.Empty;

            // A successful pair usually makes the device appear for connect, but the
            // connect port may still need a moment; discover again before connecting.
            AdbMdnsServices afterPair = AdbCommandRunner.DiscoverServices();

            if (afterPair.HasConnect)
            {
                _connectPort = afterPair.ConnectPort.ToString();
            }

            ConnectInternal();
        }

        private void ConnectInternal()
        {
            if (string.IsNullOrEmpty(_connectPort))
            {
                AdbMdnsServices services = AdbCommandRunner.DiscoverServices();

                if (services.HasConnect)
                {
                    _connectPort = services.ConnectPort.ToString();
                }

                if (string.IsNullOrEmpty(_ip) && !string.IsNullOrEmpty(services.IpAddress))
                {
                    _ip = services.IpAddress;
                }
            }

            if (string.IsNullOrEmpty(_ip) || string.IsNullOrEmpty(_connectPort))
            {
                Append("No connect port known. Toggle wireless debugging on the phone and press Refresh.");
                return;
            }

            SaveDeviceFields();

            AdbResult connect = AdbCommandRunner.Run($"connect {_ip}:{_connectPort}");
            Append(AdbCommandRunner.Describe($"connect {_ip}:{_connectPort}", connect));

            if (AdbCommandRunner.OutputIndicatesSuccess(connect, "connect"))
            {
                ShowDevices();
            }
        }

        private void ShowDevices()
        {
            AdbResult result = AdbCommandRunner.Run("devices -l");
            Append(AdbCommandRunner.Describe("devices -l", result));
        }

        private void SaveDeviceFields()
        {
            EditorPrefs.SetString(Prefs.Ip, _ip ?? string.Empty);
            EditorPrefs.SetString(Prefs.PairingPort, _pairingPort ?? string.Empty);
            EditorPrefs.SetString(Prefs.ConnectPort, _connectPort ?? string.Empty);
        }

        private void Append(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            LogBuilder.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

            if (LogBuilder.Length > MaxLogLength)
            {
                LogBuilder.Length = MaxLogLength;
            }

            Repaint();
        }

        private static string CleanDigits(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);

            foreach (char character in value)
            {
                if (char.IsDigit(character))
                {
                    builder.Append(character);
                }
            }

            return builder.ToString();
        }

        private static int ParsePort(string value)
        {
            return int.TryParse((value ?? string.Empty).Trim(), out int port) && port > 0 ? port : 0;
        }
    }
}
