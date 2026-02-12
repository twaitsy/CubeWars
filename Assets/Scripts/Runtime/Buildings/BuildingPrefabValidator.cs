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
            return;
        }

        Debug.Log($"[BuildingPrefabValidator] {name}: missing components => {string.Join(", ", missing)}", this);
    }
}
