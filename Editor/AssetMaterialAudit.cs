using UnityEngine;
using UnityEditor;

public class AssetMaterialAudit : EditorWindow
{
    [MenuItem("Tools/Asset Audit/Missing Materials Scan")]
    public static void RunAudit()
    {
        Debug.Log("=== STARTING ASSET MATERIAL AUDIT ===");

        ScanScene();
        ScanPrefabs();

        Debug.Log("=== AUDIT COMPLETE ===");
    }

    static void ScanScene()
    {
        Debug.Log("--- SCENE SCAN ---");

        Renderer[] sceneRenderers = GameObject.FindObjectsOfType<Renderer>(true);

        foreach (Renderer r in sceneRenderers)
        {
            if (HasMissingMaterial(r))
            {
                Debug.Log("Scene issue: " + GetPath(r.gameObject), r.gameObject);
            }
        }
    }

    static void ScanPrefabs()
    {
        Debug.Log("--- PREFAB SCAN ---");

        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);

            foreach (Renderer r in renderers)
            {
                if (HasMissingMaterial(r))
                {
                    Debug.Log("Prefab issue: " + path + " -> " + GetPath(r.gameObject), prefab);
                }
            }
        }
    }

    static bool HasMissingMaterial(Renderer r)
    {
        if (r == null) return false;

        if (r.sharedMaterials == null || r.sharedMaterials.Length == 0)
            return true;

        foreach (Material m in r.sharedMaterials)
        {
            if (m == null)
                return true;
        }

        return false;
    }

    static string GetPath(GameObject obj)
    {
        string path = obj.name;

        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }

        return path;
    }
}