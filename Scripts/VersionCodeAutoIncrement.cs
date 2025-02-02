using UnityEditor;
using UnityEngine;

#if UNITY_ANDROID

[InitializeOnLoad]
public static class VersionCodeAutoIncrement
{
    private const string MenuName = "Build/Auto-Increment Version Code";
    private static bool _isEnabled;

    static VersionCodeAutoIncrement()
    {
        // Load the saved toggle state
        _isEnabled = EditorPrefs.GetBool(MenuName, true);
        UnityEditor.Menu.SetChecked(MenuName, _isEnabled);
        EditorApplication.delayCall += () => ToggleAction(_isEnabled);
    }

    [MenuItem(MenuName)]
    private static void Toggle()
    {
        _isEnabled = !_isEnabled;
        EditorPrefs.SetBool(MenuName, _isEnabled);
        UnityEditor.Menu.SetChecked(MenuName, _isEnabled);
        ToggleAction(_isEnabled);
    }

    private static void ToggleAction(bool enabled)
    {
        if (enabled)
        {
            // Hook into the build process
            BuildPlayerWindow.RegisterBuildPlayerHandler(IncrementVersionCodeAndBuild);
        }
        else
        {
            // Restore default build behavior
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerWindow.DefaultBuildMethods.BuildPlayer);
        }
    }

    private static void IncrementVersionCodeAndBuild(BuildPlayerOptions options)
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            // Get current version code
            int currentVersionCode = PlayerSettings.Android.bundleVersionCode;

            // Increment by 1 (or any logic you prefer)
            int newVersionCode = currentVersionCode + 1;
            PlayerSettings.Android.bundleVersionCode = newVersionCode;
            Debug.Log($"Auto-incremented Bundle Version Code to: {newVersionCode}");
        }

        // Proceed with the build
        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
    }
}

#endif