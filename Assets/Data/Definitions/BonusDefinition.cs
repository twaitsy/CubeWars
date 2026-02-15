using System;
using UnityEngine;

public enum BonusTargetType
{
    Resource,
    Category
}

[Serializable]
public class BonusDefinition
{
    public string id; // optional, for debugging or lookup

    public BonusTargetType targetType;

    // Only one of these is used depending on targetType.
    // For Resource targets this stores a shallow data copy selected from ResourcesDatabase.
    public ResourceDefinition resource;
    public ResourceCategory category;

    // Multiplier applied to speed/efficiency/etc.
    public float multiplier = 1f;

    public bool Matches(ResourceDefinition target)
    {
        if (target == null)
            return false;

        switch (targetType)
        {
            case BonusTargetType.Resource:
                if (resource == null)
                    return false;

                if (ReferenceEquals(resource, target))
                    return true;

                if (!string.IsNullOrWhiteSpace(resource.id) &&
                    !string.IsNullOrWhiteSpace(target.id) &&
                    string.Equals(resource.id, target.id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;

            case BonusTargetType.Category:
                return category == target.category;

            default:
                return false;
        }
    }
}
