#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PrefabIntegrityValidator
{
    [MenuItem("CubeWars/Validation/Validate Prefab Integrity")]
    public static void ValidatePrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        var issues = new List<string>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            ValidatePrefab(path, prefab, issues);
        }

        if (issues.Count == 0)
        {
            Debug.Log($"[PrefabValidation] PASS. Checked {guids.Length} prefabs in Assets/Prefabs.");
            return;
        }

        Debug.LogWarning($"[PrefabValidation] Found {issues.Count} issue(s):\n - {string.Join("\n - ", issues)}");
    }

    static void ValidatePrefab(string path, GameObject prefab, List<string> issues)
    {
        if (prefab.GetComponent<Headquarters>() != null)
        {
            if (prefab.GetComponentInChildren<ResourceStorageContainer>(true) == null)
                issues.Add($"{path}: HQ prefab missing ResourceStorageContainer.");
        }

        var civ = prefab.GetComponent<Civilian>();
        if (civ != null)
        {
            if (civ.carryCapacity <= 0)
                issues.Add($"{path}: Civilian carryCapacity should be > 0.");
            if (civ.gatherTickSeconds <= 0f)
                issues.Add($"{path}: Civilian gatherTickSeconds should be > 0.");
            if (civ.searchRetrySeconds <= 0f)
                issues.Add($"{path}: Civilian searchRetrySeconds should be > 0.");
        }

        var allBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < allBehaviours.Length; i++)
        {
            if (allBehaviours[i] == null)
                issues.Add($"{path}: Missing script reference on child object.");
        }

        var storageProviders = prefab.GetComponentsInChildren<ResourceStorageProvider>(true);
        for (int i = 0; i < storageProviders.Length; i++)
        {
            var provider = storageProviders[i];
            if (provider == null) continue;
            if (provider.capacities == null || provider.capacities.Length == 0)
                issues.Add($"{path}: ResourceStorageProvider has no capacities configured ({provider.name}).");
        }
    }
}
#endif
