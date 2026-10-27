using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Plays the first scene in Build Settings (the "bootstrap" scene) instead of
    /// whatever scene is currently open, then restores the original scene when
    /// Play mode ends.
    /// </summary>
    /// <remarks>
    /// Adapted from Unity Technologies'
    /// <a href="https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop/blob/main/Assets/Scripts/Editor/SceneBootstrapper.cs">
    /// com.unity.multiplayer.samples.coop</a> (Unity Companion License).
    /// </remarks>
    [InitializeOnLoad]
    public static class SceneBootstrapper
    {
        // Prefixed so these keys never collide with Unity's own or another asset's prefs.
        private const string PreviousSceneKey = "Adeel.UnityToolbox.SceneBootstrapper.PreviousScene";
        private const string EnabledKey = "Adeel.UnityToolbox.SceneBootstrapper.Enabled";

        private const string MenuRoot = "Tools/Unity Toolbox/";
        private const string LoadBootstrapMenu = MenuRoot + "Load Bootstrap Scene On Play";
        private const string DontLoadBootstrapMenu = MenuRoot + "Don't Load Bootstrap Scene On Play";

        static SceneBootstrapper()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // The bootstrap scene is the first entry in Build Settings.
        private static string BootstrapScenePath
        {
            get
            {
                EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
                return scenes.Length > 0 ? scenes[0].path : null;
            }
        }

        private static string PreviousScene
        {
            get => EditorPrefs.GetString(PreviousSceneKey, string.Empty);
            set => EditorPrefs.SetString(PreviousSceneKey, value);
        }

        private static bool ShouldLoadBootstrapScene
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!ShouldLoadBootstrapScene)
            {
                return;
            }

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    RememberAndLoadBootstrapScene();
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    RestorePreviousScene();
                    break;
            }
        }

        private static void RememberAndLoadBootstrapScene()
        {
            string bootstrapScene = BootstrapScenePath;

            if (string.IsNullOrEmpty(bootstrapScene))
            {
                Debug.LogWarning("Scene Bootstrapper: Build Settings has no scenes — playing the open scene instead.");
                return;
            }

            string activeScene = EditorSceneManager.GetActiveScene().path;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            // Never treat the bootstrap scene itself as "the scene to return to".
            PreviousScene = activeScene != bootstrapScene ? activeScene : string.Empty;

            if (EditorSceneManager.GetActiveScene().path != bootstrapScene)
            {
                EditorSceneManager.OpenScene(bootstrapScene);
            }
        }

        private static void RestorePreviousScene()
        {
            string previousScene = PreviousScene;

            if (string.IsNullOrEmpty(previousScene) || !System.IO.File.Exists(previousScene))
            {
                return;
            }

            EditorSceneManager.OpenScene(previousScene);
        }

        [MenuItem(LoadBootstrapMenu)]
        private static void EnableBootstrapper()
        {
            ShouldLoadBootstrapScene = true;
        }

        [MenuItem(LoadBootstrapMenu, true)]
        private static bool ValidateEnableBootstrapper()
        {
            return !ShouldLoadBootstrapScene;
        }

        [MenuItem(DontLoadBootstrapMenu)]
        private static void DisableBootstrapper()
        {
            ShouldLoadBootstrapScene = false;
        }

        [MenuItem(DontLoadBootstrapMenu, true)]
        private static bool ValidateDisableBootstrapper()
        {
            return ShouldLoadBootstrapScene;
        }
    }
}
