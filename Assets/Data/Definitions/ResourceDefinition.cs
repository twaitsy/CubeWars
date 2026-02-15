using System;
using UnityEngine;

[Serializable]
public class ResourceDefinition
{
    public string id;              // "wood", "iron_ore", "bread"
    public string displayName;     // "Wood", "Iron Ore", "Bread"
    public Sprite icon;

    public ResourceCategory category;

    public float weight = 1f;      // optional gameplay stat
    public int baseValue = 1;      // optional economy stat

    public override string ToString()
    {
        if (!string.IsNullOrWhiteSpace(displayName))
            return displayName;

        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return nameof(ResourceDefinition);
    }
}
