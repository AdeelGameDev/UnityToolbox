using UnityEditor;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// When enabled, collapses every component on the object you select in the
    /// Hierarchy so the Inspector shows only the headers.
    /// </summary>
    /// <remarks>
    /// The toggle state is persisted in <see cref="EditorPrefs"/>, so it survives
    /// editor restarts. Useful when working with objects that carry many
    /// components.
    /// </remarks>
    [InitializeOnLoad]
    public static class ComponentCollapser
    {
        private const string MenuPath = "Tools/Unity Toolbox/Auto Collapse Components";
        private const string PreferenceKey = "Adeel.UnityToolbox.ComponentCollapser.Enabled";

        private static bool _enabled;

        static ComponentCollapser()
        {
            _enabled = EditorPrefs.GetBool(PreferenceKey, false);
            ApplyHandler(_enabled);
            EditorApplication.delayCall += () => Menu.SetChecked(MenuPath, _enabled);
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            _enabled = !_enabled;
            EditorPrefs.SetBool(PreferenceKey, _enabled);
            Menu.SetChecked(MenuPath, _enabled);
            ApplyHandler(_enabled);
        }

        private static void ApplyHandler(bool enable)
        {
            Selection.selectionChanged -= CollapseComponents;

            if (enable)
            {
                Selection.selectionChanged += CollapseComponents;
            }
        }

        private static void CollapseComponents()
        {
            if (Selection.activeGameObject == null || Selection.count > 1)
            {
                return;
            }

            ActiveEditorTracker tracker = ActiveEditorTracker.sharedTracker;
            tracker.ForceRebuild();

            // Index 0 is the GameObject's own header — leave it expanded.
            for (int i = 1; i < tracker.activeEditors.Length; i++)
            {
                tracker.SetVisible(i, 0);
            }
        }
    }
}
