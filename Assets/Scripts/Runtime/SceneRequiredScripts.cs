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
        typeof(HousingRegistry),
        typeof(JobManager),
        typeof(WorkerTaskDispatcher),
        typeof(WorkerTaskGenerationSystem),
        typeof(CraftingJobManager),
        typeof(ProductionNotificationManager),
        typeof(ProjectilePool),
        typeof(SelectionManager),
        typeof(UnitInspectorUI),
        typeof(BuildMenuUI),
        typeof(DiagnosticUI),
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

            // Use the new API: FindFirstObjectByType(Type)
            if (FindFirstObjectByType(requiredType) == null)
                missingRequiredTypes.Add(requiredType.Name);
        }

        // Check bootstrap requirement
        if (FindObjectsByType<TeamBootstrap>(FindObjectsSortMode.None) == null)
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
            Debug.LogWarning($"[SceneRequiredScripts] Resource diagnostics: {nodesMissingResource} node(s) missing a resource definition, {nodesResourceNotInDatabase} node(s) reference resources not present in database. Node-name auto-assignment only works when names match resource IDs/display names.", this);

        if (loaded == null)
        {
            Debug.LogWarning("[SceneRequiredScripts] Recipes diagnostics: GameDatabase is not loaded. Ensure one active GameDatabaseLoader has GameDatabase assigned.", this);
            return;
        }

        if (loaded.recipes == null || loaded.recipes.recipes == null || loaded.recipes.recipes.Count == 0)
            Debug.LogWarning("[SceneRequiredScripts] Recipes diagnostics: no RecipesDatabase entries found. Crafting station inspector recipe selection will be empty.", this);

        ValidateDatabaseMinimums(loaded);
        ValidateCraftingRoleCoverage();
        ValidateCraftingAssignmentRegistration();
        ValidateCraftingHaulerCaps();
        ValidateCivilianControllerSetup();
    }

    void ValidateCraftingRoleCoverage()
    {
        var buildings = CraftingRegistry.Instance != null ? CraftingRegistry.Instance.AllBuildings : Array.Empty<CraftingBuilding>();
        var civilians = CivilianRegistry.GetAll();

        foreach (CraftingBuilding building in buildings)
        {
            if (building == null || building.recipe == null)
                continue;

            CivilianJobType required = building.recipe.requiredJobType;
            if (required == CivilianJobType.Generalist || required == CivilianJobType.Idle)
                continue;

            bool hasMatch = false;
            for (int c = 0; c < civilians.Count; c++)
            {
                Civilian civ = civilians[c];
                if (civ == null || civ.teamID != building.teamID)
                    continue;

                if (civ.JobType == required)
                {
                    hasMatch = true;
                    break;
                }
            }

            if (!hasMatch)
                Debug.LogWarning($"[SceneRequiredScripts] Crafting diagnostics: '{building.name}' requires {required} but no same-team civilian currently has that job type.", building);
        }
    }

    void ValidateCraftingAssignmentRegistration()
    {
        var buildings = CraftingRegistry.Instance != null ? CraftingRegistry.Instance.AllBuildings : Array.Empty<CraftingBuilding>();
        var civilians = CivilianRegistry.GetAll();

        foreach (CraftingBuilding building in buildings)
        {
            if (building == null)
                continue;

            int linkedWorkers = 0;

            for (int c = 0; c < civilians.Count; c++)
            {
                Civilian civ = civilians[c];
                if (civ == null || civ.teamID != building.teamID)
                    continue;

                if (civ.AssignedCraftingBuilding == building)
                    linkedWorkers++;
            }

            if (linkedWorkers > building.AssignedWorkers.Count)
                Debug.LogWarning($"[SceneRequiredScripts] Crafting diagnostics: '{building.name}' has {linkedWorkers} civilian link(s) but only {building.AssignedWorkers.Count} registered worker(s). Reassign affected civilians to refresh assignment registration.", building);
        }
    }


    void ValidateCraftingHaulerCaps()
    {
        var buildings = CraftingRegistry.Instance != null ? CraftingRegistry.Instance.AllBuildings : Array.Empty<CraftingBuilding>();

        foreach (CraftingBuilding building in buildings)
        {
            if (building == null || !building.requireHaulerLogistics)
                continue;

            if (building.maxAssignedHaulers < 1)
                Debug.LogWarning($"[SceneRequiredScripts] Crafting diagnostics: '{building.name}' has requireHaulerLogistics enabled but maxAssignedHaulers is less than 1. Runtime clamps this to 1.", building);

            int assignedHaulers = building.GetAssignedHaulerCount();
            int cap = Mathf.Max(1, building.maxAssignedHaulers);
            if (assignedHaulers > cap)
                Debug.LogWarning($"[SceneRequiredScripts] Crafting diagnostics: '{building.name}' currently has {assignedHaulers} hauler(s) assigned but cap is {cap}.", building);
        }
    }

    void ValidateDatabaseMinimums(GameDatabase loaded)
    {
        if (loaded == null)
            return;

        int resourceCount = loaded.resources != null && loaded.resources.resources != null ? loaded.resources.resources.Count : 0;
        int jobCount = loaded.jobs != null && loaded.jobs.jobs != null ? loaded.jobs.jobs.Count : 0;
        int foodCount = loaded.foods != null && loaded.foods.foods != null ? loaded.foods.foods.Count : 0;
        int recipeCount = loaded.recipes != null && loaded.recipes.recipes != null ? loaded.recipes.recipes.Count : 0;
        int toolCount = loaded.tools != null && loaded.tools.tools != null ? loaded.tools.tools.Count : 0;

        if (resourceCount < 50)
            Debug.LogWarning($"[SceneRequiredScripts] Database diagnostics: resources count is {resourceCount}; expected at least 50 for RTS economy coverage.", this);

        if (jobCount < 10)
            Debug.LogWarning($"[SceneRequiredScripts] Database diagnostics: jobs count is {jobCount}; expected at least 10 civilian job definitions.", this);

        if (foodCount < 20)
            Debug.LogWarning($"[SceneRequiredScripts] Database diagnostics: foods count is {foodCount}; expected at least 20 food entries.", this);

        if (recipeCount < 40)
            Debug.LogWarning($"[SceneRequiredScripts] Database diagnostics: recipes count is {recipeCount}; expected at least 40 crafting recipes.", this);

        if (toolCount < 20)
            Debug.LogWarning($"[SceneRequiredScripts] Database diagnostics: tools count is {toolCount}; expected at least 20 tool definitions.", this);
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
    void ValidateCivilianControllerSetup()
    {
        var civilians = CivilianRegistry.GetAll();
        for (int i = 0; i < civilians.Count; i++)
        {
            Civilian civ = civilians[i];
            if (civ == null)
                continue;

            if (civ.GetComponent<HealthComponent>() == null ||
                civ.GetComponent<MovementController>() == null ||
                civ.GetComponent<CarryingController>() == null ||
                civ.GetComponent<GatheringController>() == null ||
                civ.GetComponent<ConstructionWorkerControl>() == null ||
                civ.GetComponent<NeedsController>() == null ||
                civ.GetComponent<CivilianStateMachineController>() == null)
            {
                Debug.LogWarning($"[SceneRequiredScripts] Civilian diagnostics: '{civ.name}' is missing one or more required controller components (Health/Movement/Carrying/Gathering/Construction/Needs/StateMachine).", civ);
            }

            if (civ.foodDatabase == null && civ.resourcesDatabase == null)
            {
                Debug.LogWarning($"[SceneRequiredScripts] Civilian diagnostics: '{civ.name}' has neither FoodDatabase nor ResourcesDatabase assigned. Hunger food fallback will depend on global ResourcesDatabase.Instance.", civ);
            }
        }
    }

}
