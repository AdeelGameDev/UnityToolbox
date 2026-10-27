using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Configuration window for <see cref="AutoConnectADB"/>.
    /// </summary>
    internal class AdbSettingsWindow : EditorWindow
    {
        private string _adbPath;
        private string _device;

        internal static void ShowWindow()
        {
            AdbSettingsWindow window = GetWindow<AdbSettingsWindow>(true, "ADB Settings", true);
            window.minSize = new Vector2(440f, 130f);
            window.Show();
        }

        private void OnEnable()
        {
            _adbPath = AutoConnectADB.AdbPath;
            _device = AutoConnectADB.Device;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Wireless ADB", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Enable wireless debugging on the device (Android 11+: Developer options > Wireless debugging), " +
                "or run 'adb tcpip 5555' once over USB first.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                _adbPath = EditorGUILayout.TextField("adb Path", _adbPath);

                if (GUILayout.Button("Browse", GUILayout.Width(70f)))
                {
                    string selected = EditorUtility.OpenFilePanel("Select adb", string.Empty, string.Empty);

                    if (!string.IsNullOrEmpty(selected))
                    {
                        _adbPath = selected;
                    }
                }
            }

            _device = EditorGUILayout.TextField("Device (ip:port)", _device);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save"))
                {
                    Save();
                }

                if (GUILayout.Button("Save and Connect"))
                {
                    Save();
                    AutoConnectADB.Connect();
                }
            }
        }

        private void Save()
        {
            AutoConnectADB.AdbPath = _adbPath;
            AutoConnectADB.Device = _device;
        }
    }
}
