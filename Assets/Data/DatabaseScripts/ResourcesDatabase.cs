using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Resources")]
public class ResourcesDatabase : ScriptableObject
{
    public List<ResourceDefinition> resources = new List<ResourceDefinition>();

    public bool TryGetById(string resourceId, out ResourceDefinition definition)
    {
        definition = null;
        if (resources == null)
            return false;

        string target = Normalize(resourceId);
        if (string.IsNullOrEmpty(target))
            return false;

        for (int i = 0; i < resources.Count; i++)
        {
            ResourceDefinition entry = resources[i];
            if (entry == null)
                continue;

            if (Normalize(entry.id) == target || Normalize(entry.displayName) == target)
            {
                definition = entry;
                return true;
            }
        }

        return false;
    }

    public bool IsCategory(ResourceDefinition definition, ResourceCategory category)
    {
        return definition != null && definition.category == category;
    }

    public bool IsCategoryById(string resourceId, ResourceCategory category)
    {
        if (!TryGetById(resourceId, out ResourceDefinition definition))
            return false;

        return IsCategory(definition, category);
    }


    static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Replace("_", string.Empty)
                    .Replace("-", string.Empty)
                    .Replace(" ", string.Empty)
                    .Trim()
                    .ToLowerInvariant();
    }
}
