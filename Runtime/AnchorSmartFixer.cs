using UnityEngine;
using UnityEditor;

public class AnchorSmartFixer
{
    [MenuItem("Tools/Fix Anchors Smart %#a")]
    private static void FixAnchors()
    {
        foreach (var obj in Selection.gameObjects)
        {
            var rt = obj.GetComponent<RectTransform>();
            if (rt != null && rt.parent != null)
                Apply(rt);
        }
    }

    static void Apply(RectTransform rt)
    {
        RectTransform parent = rt.parent as RectTransform;
        if (parent == null) return;

        // STEP 1: store world position
        Vector3 worldPos = rt.position;

        Vector2 parentSize = parent.rect.size;

        Vector2 localPos = rt.anchoredPosition;

        Vector2 normalized = new Vector2(
            (localPos.x / parentSize.x) + 0.5f,
            (localPos.y / parentSize.y) + 0.5f
        );

        Vector2 anchor = GetAnchorPreset(normalized);

        Undo.RecordObject(rt, "Fix Anchors Smart");

        // STEP 2: apply new anchors
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;

        // STEP 3: restore position so UI does NOT move
        rt.position = worldPos;
    }

    static Vector2 GetAnchorPreset(Vector2 n)
    {
        float x =
            n.x < 0.33f ? 0f :
            n.x < 0.66f ? 0.5f : 1f;

        float y =
            n.y < 0.33f ? 0f :
            n.y < 0.66f ? 0.5f : 1f;

        return new Vector2(x, y);
    }
}