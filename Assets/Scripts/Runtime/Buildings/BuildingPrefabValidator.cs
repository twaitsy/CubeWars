using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingPrefabValidator : MonoBehaviour
{
    [Header("Validation")]
    public bool validateOnAwake = true;
    public bool includeOptionalWarnings = true;

    void Awake()
    {
        if (validateOnAwake)
            ValidateAndLog();
    }

    [ContextMenu("Validate Building Components")]
    public void ValidateAndLog()
    {
        bool hasBuilding = TryGetComponent<Building>(out _);
        bool hasCivilian = TryGetComponent<Civilian>(out _);
        bool hasResourceNode = TryGetComponent<ResourceNode>(out _);

        if (hasBuilding)
            ValidateBuildingPrefab();

        if (hasCivilian)
            ValidateCivilianPrefab();

        if (hasResourceNode)
            ValidateResourceNodePrefab();

        if (!hasBuilding && !hasCivilian && !hasResourceNode)
            Debug.Log($"[BuildingPrefabValidator] {name}: no supported root component found (Building/Civilian/ResourceNode).", this);
    }

    void ValidateBuildingPrefab()
    {
        var missing = new List<string>();
        var conflicts = new List<string>();

        bool hasCrafting = TryGetComponent<CraftingBuilding>(out _);
        bool hasStorageContainer = TryGetComponent<ResourceStorageContainer>(out _);
        bool hasStorageProvider = TryGetComponent<ResourceStorageProvider>(out _);
        bool hasDropoff = TryGetComponent<ResourceDropoff>(out _);
        int localStorageContainerCount = GetComponentsInChildren<ResourceStorageContainer>(true).Length;

        if (!TryGetComponent<Collider>(out _))
            missing.Add("Collider (required)");

        if (!TryGetComponent<BuildGridOccupant>(out _))
            missing.Add("BuildGridOccupant (required)");

        if (!TryGetComponent<BuildingFootprint>(out _))
            missing.Add("BuildingFootprint (required)");

        if (!TryGetComponent<TeamVisual>(out _) && includeOptionalWarnings)
            missing.Add("TeamVisual (recommended)");

        if (!hasStorageContainer && includeOptionalWarnings)
            missing.Add("ResourceStorageContainer (optional, needed for storage/logistics buildings)");

        if (!hasStorageProvider && includeOptionalWarnings)
            missing.Add("ResourceStorageProvider (optional, needed for adding storage capacity)");

        if (hasCrafting && !hasStorageContainer)
            missing.Add("ResourceStorageContainer (required on CraftingBuilding so inspector storage and hauler logistics target the same container)");

        if (missing.Count == 0)
        {
            Debug.Log($"[BuildingPrefabValidator] {name}: all checked components are present.", this);

        ValidateConflictingScripts(conflicts, hasCrafting, hasStorageContainer, hasStorageProvider, hasDropoff, localStorageContainerCount);

        if (conflicts.Count > 0)
            Debug.LogWarning($"[BuildingPrefabValidator] {name}: conflicting/not-required components => {string.Join(", ", conflicts)}", this);

            return;
        }

        Debug.Log($"[BuildingPrefabValidator] {name}: missing components => {string.Join(", ", missing)}", this);

        ValidateConflictingScripts(conflicts, hasCrafting, hasStorageContainer, hasStorageProvider, hasDropoff, localStorageContainerCount);

        if (conflicts.Count > 0)
            Debug.LogWarning($"[BuildingPrefabValidator] {name}: conflicting/not-required components => {string.Join(", ", conflicts)}", this);

    }

    void ValidateCivilianPrefab()
    {
        var missing = new List<string>();
        var warnings = new List<string>();

        Civilian civilian = GetComponent<Civilian>();

        if (!TryGetComponent<NavMeshAgent>(out _))
            missing.Add("NavMeshAgent (required)");

        if (!TryGetComponent<Selectable>(out _) && includeOptionalWarnings)
            warnings.Add("Selectable (recommended for player interaction)");

        if (civilian != null)
        {
            if (string.IsNullOrWhiteSpace(civilian.unitDefinitionId))
                missing.Add("unitDefinitionId (required for database lookup)");
            else if (!IsUnitInDatabase(civilian.unitDefinitionId))
                warnings.Add($"unitDefinitionId '{civilian.unitDefinitionId}' not found in loaded GameDatabase units");

            if (civilian.carryCapacity <= 0)
                warnings.Add("carryCapacity <= 0 (civilian will effectively carry 1)");

            if (civilian.harvestPerTick <= 0)
                warnings.Add("harvestPerTick <= 0 (civilian will effectively gather 1 per tick)");

            if (civilian.resourcesDatabase == null && includeOptionalWarnings)
                warnings.Add("resourcesDatabase missing (food fallback category checks will be limited)");
        }

        if (TryGetComponent<ResourceStorageContainer>(out _))
            warnings.Add("ResourceStorageContainer on Civilian is not required for gathering/hauling");

        if (TryGetComponent<ResourceStorageProvider>(out _))
            warnings.Add("ResourceStorageProvider on Civilian is not required for gathering/hauling");

        EmitValidationLog("civilian", missing, warnings);
    }

    void ValidateResourceNodePrefab()
    {
        var missing = new List<string>();
        var warnings = new List<string>();

        ResourceNode node = GetComponent<ResourceNode>();

        if (!TryGetComponent<Collider>(out _))
            missing.Add("Collider (required for selection/interaction)");

        if (node != null)
        {
            if (node.resource == null)
                missing.Add("resource (ResourceDefinition reference)");
            else if (!IsResourceInDatabase(node.resource))
                warnings.Add($"resource '{node.resource.id}' is not present in loaded resource databases");

            if (node.remaining <= 0)
                warnings.Add("remaining <= 0 (node starts depleted)");

            if (node.maxGatherers <= 0)
                warnings.Add("maxGatherers <= 0 (will be clamped to 1 at runtime)");
        }

        if (ResourceRegistry.Instance == null && includeOptionalWarnings)
            warnings.Add("ResourceRegistry missing in scene (node discovery/jobs will fail)");

        EmitValidationLog("resource node", missing, warnings);
    }

    void EmitValidationLog(string label, List<string> missing, List<string> warnings)
    {
        if (missing.Count == 0)
            Debug.Log($"[BuildingPrefabValidator] {name} ({label}): required checks passed.", this);
        else
            Debug.Log($"[BuildingPrefabValidator] {name} ({label}): missing => {string.Join(", ", missing)}", this);

        if (warnings.Count > 0)
            Debug.LogWarning($"[BuildingPrefabValidator] {name} ({label}): warnings => {string.Join(", ", warnings)}", this);
    }

    static bool IsUnitInDatabase(string unitId)
    {
        if (string.IsNullOrWhiteSpace(unitId))
            return false;

        GameDatabase loaded = GameDatabaseLoader.Loaded;
        if (loaded != null && loaded.TryGetUnitById(unitId, out var loadedDef) && loadedDef != null)
            return true;

        return false;
    }

    static bool IsResourceInDatabase(ResourceDefinition resource)
    {
        string key = ResourceIdUtility.GetKey(resource);
        if (string.IsNullOrEmpty(key))
            return false;

        GameDatabase loaded = GameDatabaseLoader.Loaded;
        if (loaded != null && loaded.resources != null && loaded.resources.resources != null)
        {
            List<ResourceDefinition> entries = loaded.resources.resources;
            for (int i = 0; i < entries.Count; i++)
                if (ResourceIdUtility.GetKey(entries[i]) == key)
                    return true;
        }

        if (ResourcesDatabase.Instance != null && ResourcesDatabase.Instance.resources != null)
        {
            List<ResourceDefinition> entries = ResourcesDatabase.Instance.resources;
            for (int i = 0; i < entries.Count; i++)
                if (ResourceIdUtility.GetKey(entries[i]) == key)
                    return true;
        }

        return false;
    }

    void ValidateConflictingScripts(List<string> conflicts, bool hasCrafting, bool hasStorageContainer, bool hasStorageProvider, bool hasDropoff, int localStorageContainerCount)
    {
        int productionScriptCount = 0;
        if (hasCrafting) productionScriptCount++;
        if (TryGetComponent<Farm>(out _)) productionScriptCount++;
        if (TryGetComponent<Barracks>(out _)) productionScriptCount++;
        if (TryGetComponent<WeaponsFactory>(out _)) productionScriptCount++;

        if (productionScriptCount > 1)
            conflicts.Add("Multiple production scripts detected (CraftingBuilding/Farm/Barracks/WeaponsFactory)");

        if (TryGetComponent<ProductionStatusVisualizer>(out _) && !hasCrafting)
            conflicts.Add("ProductionStatusVisualizer present without CraftingBuilding");

        if (hasStorageProvider && !hasStorageContainer)
            conflicts.Add("ResourceStorageProvider present without ResourceStorageContainer");

        if (hasDropoff && hasCrafting)
            conflicts.Add("ResourceDropoff on CraftingBuilding may conflict with dedicated crafting input/output logistics");

        if (hasCrafting && localStorageContainerCount > 1)
            conflicts.Add($"CraftingBuilding has {localStorageContainerCount} ResourceStorageContainer components in hierarchy (use exactly one to avoid split logistics/inspector storage)");
    }
}
