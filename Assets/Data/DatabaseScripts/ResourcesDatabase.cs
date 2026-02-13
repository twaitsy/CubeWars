using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Food,
    Gold,
    Wood,
    Stone,
    IronOre,
    Coal,
    Copper,
    Silicon,
    Lithium,

    IronIngot,
    Steel,
    Lumber,
    Flour,
    Bread,
    Tools,
    Fuel
}

[CreateAssetMenu(menuName = "CubeWars/Database/Resources")]
public class ResourcesDatabase : ScriptableObject
{
    public List<ResourceDefinition> resources = new List<ResourceDefinition>();

    public bool TryGet(ResourceType type, out ResourceDefinition definition)
    {
        return TryGet(resources, type, out definition);
    }

    public bool IsCategory(ResourceType type, ResourceCategory category)
    {
        return IsCategory(resources, type, category);
    }

    public static bool IsCategory(ResourcesDatabase database, ResourceType type, ResourceCategory category)
    {
        if (database == null)
            return false;

        return IsCategory(database.resources, type, category);
    }

    static bool IsCategory(List<ResourceDefinition> defs, ResourceType type, ResourceCategory category)
    {
        if (!TryGet(defs, type, out ResourceDefinition definition))
            return false;

        return definition != null && definition.category == category;
    }

    static bool TryGet(List<ResourceDefinition> defs, ResourceType type, out ResourceDefinition definition)
    {
        definition = null;
        if (defs == null)
            return false;

        string target = Normalize(type.ToString());

        for (int i = 0; i < defs.Count; i++)
        {
            ResourceDefinition entry = defs[i];
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
