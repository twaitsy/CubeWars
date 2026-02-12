using System.Collections.Generic;
using UnityEngine;

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
        var missing = new List<string>();
        var conflicts = new List<string>();

        bool hasCrafting = TryGetComponent<CraftingBuilding>(out _);
        bool hasStorageContainer = TryGetComponent<ResourceStorageContainer>(out _);
        bool hasStorageProvider = TryGetComponent<ResourceStorageProvider>(out _);
        bool hasDropoff = TryGetComponent<ResourceDropoff>(out _);
        int localStorageContainerCount = GetComponentsInChildren<ResourceStorageContainer>(true).Length;

        if (!TryGetComponent<Building>(out _))
            missing.Add("Building (required)");

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
