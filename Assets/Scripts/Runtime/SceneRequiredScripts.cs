using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneRequiredScripts : MonoBehaviour
{
    [SerializeField] bool checkOnAwake = true;

    static readonly Type[] RequiredSceneScripts =
    {
        typeof(GameManager),
        typeof(TeamStorageManager),
        typeof(TeamResources),
        typeof(DiplomacyManager),
        typeof(BuildPlacementManager),
        typeof(BuildGridManager),
        typeof(ResourceRegistry),
        typeof(JobManager),
        typeof(WorkerTaskDispatcher),
        typeof(WorkerTaskGenerationSystem),
        typeof(CraftingJobManager),
        typeof(ProductionNotificationManager),
        typeof(ProjectilePool),
        typeof(SelectionManager),
        typeof(UnitInspectorUI),
        typeof(BuildMenuUI),
        typeof(Minimap),
        typeof(RTSCamera),
    };

    void Awake()
    {
        if (checkOnAwake)
            ValidateScene();
    }

    [ContextMenu("Validate Required Scene Scripts")]
    public void ValidateScene()
    {
        var missingRequiredTypes = new List<string>();

        // Check required scene scripts
        for (int i = 0; i < RequiredSceneScripts.Length; i++)
        {
            Type requiredType = RequiredSceneScripts[i];
            if (FindObjectOfType(requiredType) == null)
                missingRequiredTypes.Add(requiredType.Name);
        }

        // Check bootstrap requirement
        if (FindObjectOfType<TeamBootstrap>() == null &&
            FindObjectOfType<SixTeamBootstrap>() == null &&
            FindObjectOfType<HQSpawner>() == null)
        {
            missingRequiredTypes.Add("TeamBootstrap OR SixTeamBootstrap OR HQSpawner");
        }

        // Check for missing script components
        int missingScriptComponents = CountMissingScriptComponentsInActiveScene();

        if (missingRequiredTypes.Count == 0 && missingScriptComponents == 0)
        {
            Debug.Log("[SceneRequiredScripts] Validation passed: all required scene scripts are present and no missing script components were found.", this);
            ValidateWorkforceDataSetup();
            return;
        }

        if (missingRequiredTypes.Count > 0)
        {
            Debug.LogError("[SceneRequiredScripts] Missing required scene scripts: " + string.Join(", ", missingRequiredTypes), this);
        }

        if (missingScriptComponents > 0)
        {
            Debug.LogError("[SceneRequiredScripts] Found " + missingScriptComponents + " missing script component(s) in the active scene hierarchy.", this);
        }

        ValidateWorkforceDataSetup();
    }

    void ValidateWorkforceDataSetup()
    {
        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();

        // Civilians
        Civilian[] civilians = Array.FindAll(
    Resources.FindObjectsOfTypeAll<Civilian>(),
    c => c.gameObject.scene.IsValid()
);
        int civiliansMissingUnitDefinition = 0;

        for (int i = 0; i < civilians.Length; i++)
        {
            Civilian civ = civilians[i];
            if (civ == null)
                continue;

            bool missing = string.IsNullOrWhiteSpace(civ.unitDefinitionId)
                || loaded == null
                || !loaded.TryGetUnitById(civ.unitDefinitionId, out var def)
                || def == null;

            if (missing)
                civiliansMissingUnitDefinition++;
        }

        // Resource nodes
        ResourceNode[] nodes = Array.FindAll(
    Resources.FindObjectsOfTypeAll<ResourceNode>(),
    n => n.gameObject.scene.IsValid()
);
        int nodesMissingResource = 0;
        int nodesResourceNotInDatabase = 0;

        for (int i = 0; i < nodes.Length; i++)
        {
            ResourceNode node = nodes[i];
            if (node == null)
                continue;

            if (node.resource == null)
            {
                nodesMissingResource++;
                continue;
            }

            string resourceKey = ResourceIdUtility.GetKey(node.resource);

            bool inLoadedDatabase = false;
            if (loaded != null && loaded.resources != null && loaded.resources.resources != null)
            {
                for (int r = 0; r < loaded.resources.resources.Count; r++)
                {
                    if (ResourceIdUtility.GetKey(loaded.resources.resources[r]) == resourceKey)
                    {
                        inLoadedDatabase = true;
                        break;
                    }
                }
            }

            bool inGlobalResources = false;
            if (ResourcesDatabase.Instance != null && ResourcesDatabase.Instance.resources != null)
            {
                for (int r = 0; r < ResourcesDatabase.Instance.resources.Count; r++)
                {
                    if (ResourceIdUtility.GetKey(ResourcesDatabase.Instance.resources[r]) == resourceKey)
                    {
                        inGlobalResources = true;
                        break;
                    }
                }
            }

            if (!inLoadedDatabase && !inGlobalResources)
                nodesResourceNotInDatabase++;
        }

        if (civiliansMissingUnitDefinition > 0)
            Debug.LogWarning($"[SceneRequiredScripts] Workforce diagnostics: {civiliansMissingUnitDefinition} civilian(s) have missing/invalid unitDefinitionId database entries.", this);

        if (nodesMissingResource > 0 || nodesResourceNotInDatabase > 0)
            Debug.LogWarning($"[SceneRequiredScripts] Resource diagnostics: {nodesMissingResource} node(s) missing a resource definition, {nodesResourceNotInDatabase} node(s) reference resources not present in database.", this);

        if (loaded == null)
        {
            Debug.LogWarning("[SceneRequiredScripts] Recipes diagnostics: GameDatabase is not loaded. Ensure one active GameDatabaseLoader has GameDatabase assigned.", this);
            return;
        }

        if (loaded.recipes == null || loaded.recipes.recipes == null || loaded.recipes.recipes.Count == 0)
            Debug.LogWarning("[SceneRequiredScripts] Recipes diagnostics: no RecipesDatabase entries found. Crafting station inspector recipe selection will be empty.", this);
    }

    static int CountMissingScriptComponentsInActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
            return 0;

        int missingCount = 0;
        GameObject[] roots = scene.GetRootGameObjects();

        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] allTransforms = roots[i].GetComponentsInChildren<Transform>(true);

            for (int j = 0; j < allTransforms.Length; j++)
            {
                Component[] components = allTransforms[j].GetComponents<Component>();

                for (int k = 0; k < components.Length; k++)
                {
                    if (components[k] == null)
                        missingCount++;
                }
            }
        }

        return missingCount;
    }
}
