using UnityEngine;
using UnityEditor;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Finds renderers whose material slot is empty or points at a missing
    /// material, in both the open scenes and every prefab in the project.
    /// </summary>
    /// <remarks>
    /// Results are written to the Console and clicking a log line pings the
    /// offending object. Handy right before a build or a store submission.
    /// </remarks>
    public static class AssetMaterialAudit
    {
        [MenuItem("Tools/Unity Toolbox/Asset Audit/Find Missing Materials")]
        public static void RunAudit()
        {
            Debug.Log("=== Asset material audit started ===");

            int issues = ScanScene() + ScanPrefabs();

            Debug.Log(issues == 0
                ? "=== Audit complete: no missing materials ==="
                : $"=== Audit complete: {issues} issue(s) found ===");
        }

        private static int ScanScene()
        {
            Debug.Log("--- Scene scan ---");

            int issues = 0;

            // FindObjectsOfType keeps this compatible with Unity 2021.3; the newer
            // FindObjectsByType overloads only exist from 2022.2 onwards.
#pragma warning disable CS0618
            Renderer[] sceneRenderers = Object.FindObjectsOfType<Renderer>(true);
#pragma warning restore CS0618

            foreach (Renderer renderer in sceneRenderers)
            {
                if (!HasMissingMaterial(renderer))
                {
                    continue;
                }

                Debug.Log($"Scene issue: {GetPath(renderer.gameObject)}", renderer.gameObject);
                issues++;
            }

            return issues;
        }

        private static int ScanPrefabs()
        {
            Debug.Log("--- Prefab scan ---");

            int issues = 0;
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
                {
                    if (!HasMissingMaterial(renderer))
                    {
                        continue;
                    }

                    Debug.Log($"Prefab issue: {path} -> {GetPath(renderer.gameObject)}", prefab);
                    issues++;
                }
            }

            return issues;
        }

        private static bool HasMissingMaterial(Renderer renderer)
        {
            if (renderer == null)
            {
                return false;
            }

            Material[] materials = renderer.sharedMaterials;

            if (materials == null || materials.Length == 0)
            {
                return true;
            }

            foreach (Material material in materials)
            {
                if (material == null)
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetPath(GameObject gameObject)
        {
            string path = gameObject.name;
            Transform current = gameObject.transform;

            while (current.parent != null)
            {
                current = current.parent;
                path = current.name + "/" + path;
            }

            return path;
        }
    }
}
