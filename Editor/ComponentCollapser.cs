using UnityEditor;

[InitializeOnLoad]
public class ComponentCollapser
{
    private const string MENU_PATH = "Tools/Auto Collapse Components";
    private static bool _enabled;

    static ComponentCollapser()
    {
        _enabled = EditorPrefs.GetBool(MENU_PATH, false);
        SetHandler(_enabled);
        EditorApplication.delayCall += () => Menu.SetChecked(MENU_PATH, _enabled);
    }

    [MenuItem(MENU_PATH)]
    private static void Toggle()
    {
        _enabled = !_enabled;
        Menu.SetChecked(MENU_PATH, _enabled);
        EditorPrefs.SetBool(MENU_PATH, _enabled);
        SetHandler(_enabled);
    }

    private static void SetHandler(bool enable)
    {
        if (enable)
        {
            Selection.selectionChanged += CollapseComponents;
        }
        else
        {
            Selection.selectionChanged -= CollapseComponents;
        }
    }

    private static void CollapseComponents()
    {
        if (Selection.activeGameObject == null || Selection.count > 1) return;

        // Get the ActiveEditorTracker for the current selection
        var tracker = ActiveEditorTracker.sharedTracker;

        // Force the tracker to rebuild
        tracker.ForceRebuild();

        // Iterate through all the editors and set their expanded state to false
        for (int i = 0; i < tracker.activeEditors.Length; i++)
        {
            // Skip the first component (GameObject component)
            if (i == 0) continue;

            tracker.SetVisible(i, 0); // Collapse the component
        }
    }
}