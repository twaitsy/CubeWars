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

        if (!TryGetComponent<ResourceStorageContainer>(out _) && includeOptionalWarnings)
            missing.Add("ResourceStorageContainer (optional, needed for storage/logistics buildings)");

        if (!TryGetComponent<ResourceStorageProvider>(out _) && includeOptionalWarnings)
            missing.Add("ResourceStorageProvider (optional, needed for adding storage capacity)");

        if (missing.Count == 0)
        {
            Debug.Log($"[BuildingPrefabValidator] {name}: all checked components are present.", this);

        ValidateConflictingScripts(conflicts);

        if (conflicts.Count > 0)
            Debug.LogWarning($"[BuildingPrefabValidator] {name}: conflicting/not-required components => {string.Join(", ", conflicts)}", this);

            return;
        }

        Debug.Log($"[BuildingPrefabValidator] {name}: missing components => {string.Join(", ", missing)}", this);

        ValidateConflictingScripts(conflicts);

        if (conflicts.Count > 0)
            Debug.LogWarning($"[BuildingPrefabValidator] {name}: conflicting/not-required components => {string.Join(", ", conflicts)}", this);

    }

    void ValidateConflictingScripts(List<string> conflicts)
    {
        int productionScriptCount = 0;
        if (TryGetComponent<CraftingBuilding>(out _)) productionScriptCount++;
        if (TryGetComponent<Farm>(out _)) productionScriptCount++;
        if (TryGetComponent<Barracks>(out _)) productionScriptCount++;
        if (TryGetComponent<WeaponsFactory>(out _)) productionScriptCount++;

        if (productionScriptCount > 1)
            conflicts.Add("Multiple production scripts detected (CraftingBuilding/Farm/Barracks/WeaponsFactory)");

        if (TryGetComponent<ProductionStatusVisualizer>(out _) && !TryGetComponent<CraftingBuilding>(out _))
            conflicts.Add("ProductionStatusVisualizer present without CraftingBuilding");

        if (TryGetComponent<ResourceStorageProvider>(out _) && !TryGetComponent<ResourceStorageContainer>(out _))
            conflicts.Add("ResourceStorageProvider present without ResourceStorageContainer");

        if (TryGetComponent<ResourceDropoff>(out _) && TryGetComponent<CraftingBuilding>(out _))
            conflicts.Add("ResourceDropoff on CraftingBuilding may conflict with dedicated crafting input/output logistics");
    }
}
