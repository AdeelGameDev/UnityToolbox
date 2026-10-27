using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Flips <see cref="EditorSettings.enterPlayModeOptionsEnabled"/> on and off.
    /// </summary>
    /// <remarks>
    /// Disabling domain reload makes entering Play mode much faster at the cost of
    /// keeping static state between runs — handy while iterating, but turn it back
    /// on before trusting a build.
    /// </remarks>
    public static class EnterPlayModeToggle
    {
        [MenuItem("Tools/Unity Toolbox/Toggle Enter Play Mode Options")]
        private static void ToggleEnterPlayModeOptions()
        {
            EditorSettings.enterPlayModeOptionsEnabled = !EditorSettings.enterPlayModeOptionsEnabled;

            Debug.Log(EditorSettings.enterPlayModeOptionsEnabled
                ? "Enter Play Mode Options: enabled (domain reload will be skipped)"
                : "Enter Play Mode Options: disabled (full domain reload)");
        }

        [MenuItem("Tools/Unity Toolbox/Toggle Enter Play Mode Options", true)]
        private static bool ValidateToggleEnterPlayModeOptions()
        {
            Menu.SetChecked("Tools/Unity Toolbox/Toggle Enter Play Mode Options",
                EditorSettings.enterPlayModeOptionsEnabled);
            return true;
        }
    }
}
