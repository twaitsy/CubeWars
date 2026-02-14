using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Food", fileName = "FoodDatabase")]
public class FoodDatabase : ScriptableObject
{
    public List<FoodDefinition> foods = new List<FoodDefinition>();

    public bool TryGet(ResourceDefinition resource, out FoodDefinition definition)
    {
        definition = null;
        if (resource == null || foods == null)
            return false;

        for (int i = 0; i < foods.Count; i++)
        {
            FoodDefinition candidate = foods[i];
            if (candidate == null || candidate.resource == null)
                continue;

            if (ReferenceEquals(candidate.resource, resource) || string.Equals(candidate.resource.id, resource.id, StringComparison.OrdinalIgnoreCase))
            {
                definition = candidate;
                return true;
            }
        }

        return false;
    }

    public IEnumerable<FoodDefinition> EnumerateFoods()
    {
        if (foods == null)
            yield break;

        for (int i = 0; i < foods.Count; i++)
        {
            FoodDefinition entry = foods[i];
            if (entry == null || entry.resource == null)
                continue;

            yield return entry;
        }
    }
}
