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
        _isEnabled = EditorPrefs.GetBool(MenuName, true);
        Menu.SetChecked(MenuName, _isEnabled);
        EditorApplication.delayCall += () => ToggleAction(_isEnabled);
    }

    [MenuItem(MenuName)]
    private static void Toggle()
    {
        _isEnabled = !_isEnabled;
        EditorPrefs.SetBool(MenuName, _isEnabled);
        Menu.SetChecked(MenuName, _isEnabled);
        ToggleAction(_isEnabled);
    }

    private static void ToggleAction(bool enabled)
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
                Debug.Log($"Auto-incremented Bundle Version Code to: {newVersionCode} (AAB build)");
            }
            else
            {
                Debug.Log("Skipping Bundle Version Code increment: APK build detected");
            }
        }

        BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
    }
}

#endif
