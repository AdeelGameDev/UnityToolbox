using UnityEditor;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Snaps the selected UI elements to the nearest of the nine anchor presets
    /// (top-left … bottom-right) without moving them on screen.
    /// </summary>
    /// <remarks>
    /// Select one or more objects with a <see cref="RectTransform"/> and press
    /// <c>Ctrl/Cmd + Shift + A</c>, or use the Tools menu. The anchored position
    /// is recomputed against the parent's pivot so elements stay put even when
    /// the parent is not centred.
    /// </remarks>
    public static class AnchorSmartFixer
    {
        private const string MenuPath = "Tools/Unity Toolbox/Fix UI Anchors (Smart) %#a";

        [MenuItem(MenuPath)]
        private static void FixAnchors()
        {
            int changed = 0;

            foreach (GameObject selected in Selection.gameObjects)
            {
                if (!selected.TryGetComponent(out RectTransform rectTransform) || rectTransform.parent == null)
                {
                    continue;
                }

                if (!(rectTransform.parent is RectTransform parentRect))
                {
                    continue;
                }

                Undo.RecordObject(rectTransform, "Fix UI Anchors");

                if (Apply(rectTransform, parentRect))
                {
                    EditorUtility.SetDirty(rectTransform);
                    changed++;
                }
            }

            if (changed == 0)
            {
                Debug.LogWarning("Fix UI Anchors: no valid UI elements in the selection.");
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateFixAnchors()
        {
            return Selection.gameObjects.Length > 0;
        }

        private static bool Apply(RectTransform rectTransform, RectTransform parentRect)
        {
            // Keep the element visually still while its anchors change.
            Vector3 worldPosition = rectTransform.position;

            // Convert to a normalised position inside the parent, accounting for
            // the parent's own pivot rather than assuming a centred parent.
            Vector2 localPoint = parentRect.InverseTransformPoint(worldPosition);
            Vector2 parentSize = parentRect.rect.size;

            if (parentSize.x <= 0f || parentSize.y <= 0f)
            {
                return false;
            }

            Vector2 normalized = new Vector2(
                localPoint.x / parentSize.x + parentRect.pivot.x,
                localPoint.y / parentSize.y + parentRect.pivot.y);

            Vector2 anchor = GetAnchorPreset(normalized);

            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.position = worldPosition;

            return true;
        }

        private static Vector2 GetAnchorPreset(Vector2 normalized)
        {
            float x = normalized.x < 0.33f ? 0f : normalized.x < 0.66f ? 0.5f : 1f;
            float y = normalized.y < 0.33f ? 0f : normalized.y < 0.66f ? 0.5f : 1f;
            return new Vector2(x, y);
        }
    }
}
