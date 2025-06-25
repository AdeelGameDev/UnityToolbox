using UnityEditor;
using UnityEngine;

public class EnterPlayModeToggle : EditorWindow
{
    [MenuItem("Tools/Toggle Enter Play Mode Options _F1")] // Change _F1 to any key you prefer
    private static void ToggleEnterPlayModeOptions()
    {
        EditorSettings.enterPlayModeOptionsEnabled = !EditorSettings.enterPlayModeOptionsEnabled;
        Debug.Log($"Enter Play Mode Options: {(EditorSettings.enterPlayModeOptionsEnabled ? "Enabled" : "Disabled")}");
    }
}
