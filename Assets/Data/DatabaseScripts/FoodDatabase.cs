using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Food", fileName = "FoodDatabase")]
public class FoodDatabase : ScriptableObject
{
    public List<FoodDefinition> foods = new();

    public bool TryGet(ResourceDefinition resource, out FoodDefinition definition)
    {
        definition = null;
        string key = ResourceIdUtility.GetKey(resource);
        if (string.IsNullOrEmpty(key) || foods == null) return false;
        for (int i = 0; i < foods.Count; i++)
        {
            FoodDefinition entry = foods[i];
            if (entry == null || entry.resource == null) continue;
            if (string.Equals(ResourceIdUtility.GetKey(entry.resource), key, StringComparison.OrdinalIgnoreCase))
            {
                definition = entry;
                return true;
            }
        }
        return false;
    }

    public bool IsEdible(ResourceDefinition resource) => TryGet(resource, out _);

    public IEnumerable<FoodDefinition> EnumerateFoods()
    {
        if (foods == null) yield break;
        for (int i = 0; i < foods.Count; i++)
            if (foods[i] != null && foods[i].resource != null)
                yield return foods[i];
    }
}
