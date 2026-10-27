using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Bumps <c>PlayerSettings.Android.bundleVersionCode</c> before an Android
    /// build so every App Bundle uploaded to the Play Console is unique.
    /// </summary>
    /// <remarks>
    /// Only App Bundle (.aab) builds are incremented — APKs used for local
    /// testing keep whatever version code you set by hand. The version code is
    /// also persisted to <see cref="PlayerSettings"/> so it survives a restart.
    /// </remarks>
#if UNITY_ANDROID
    [InitializeOnLoad]
    public static class VersionCodeAutoIncrement
#else
    public static class VersionCodeAutoIncrement
#endif
    {
        private const string MenuPath = "Tools/Unity Toolbox/Auto-Increment Android Version Code";
        private const string PreferenceKey = "Adeel.UnityToolbox.VersionCodeAutoIncrement.Enabled";

#if UNITY_ANDROID
        private static bool _isEnabled;

        static VersionCodeAutoIncrement()
        {
            _isEnabled = EditorPrefs.GetBool(PreferenceKey, true);
            EditorApplication.delayCall += () =>
            {
                Menu.SetChecked(MenuPath, _isEnabled);
                ApplyHandler(_isEnabled);
            };
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            _isEnabled = !_isEnabled;
            EditorPrefs.SetBool(PreferenceKey, _isEnabled);
            Menu.SetChecked(MenuPath, _isEnabled);
            ApplyHandler(_isEnabled);
        }

        private static void ApplyHandler(bool enabled)
        {
            if (enabled)
            {
                BuildPlayerWindow.RegisterBuildPlayerHandler(IncrementVersionCodeAndBuild);
            }
            else
            {
                BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerWindow.DefaultBuildMethods.BuildPlayer);
            }
        }

        private static void IncrementVersionCodeAndBuild(BuildPlayerOptions options)
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                if (EditorUserBuildSettings.buildAppBundle)
                {
                    int currentVersionCode = PlayerSettings.Android.bundleVersionCode;
                    int newVersionCode = currentVersionCode + 1;
                    PlayerSettings.Android.bundleVersionCode = newVersionCode;
                    Debug.Log($"Auto-incremented Android bundle version code to {newVersionCode} (AAB build).");
                }
                else
                {
                    Debug.Log("Skipping bundle version increment: APK build detected.");
                }
            }

            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        }
#else
        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            Debug.LogWarning("Version code auto-increment only applies when the Android build target is active.");
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateToggle()
        {
            return false;
        }
#endif
    }
}
