using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Units")]
public class UnitsDatabase : ScriptableObject
{
    public List<UnitDefinition> units = new();

    public bool TryGetById(string id, out UnitDefinition def)
    {
        def = null;
        if (string.IsNullOrWhiteSpace(id)) return false;

        string target = id.Trim().ToLowerInvariant();

        foreach (var u in units)
        {
            if (u == null) continue;
            if (u.id.Trim().ToLowerInvariant() == target)
            {
                def = u;
                return true;
            }
        }

        return false;
    }
}