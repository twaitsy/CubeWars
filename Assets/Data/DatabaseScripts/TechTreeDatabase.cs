using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Tech Tree")]
public class TechTreeDatabase : ScriptableObject
{
    public List<TechNodeDefinition> techNodes = new List<TechNodeDefinition>();

    public bool TryGetById(string id, out TechNodeDefinition node)
    {
        node = null;
        if (string.IsNullOrWhiteSpace(id)) return false;

        string target = id.Trim().ToLowerInvariant();

        foreach (var t in techNodes)
        {
            if (t == null) continue;
            if (t.id.Trim().ToLowerInvariant() == target)
            {
                node = t;
                return true;
            }
        }

        return false;
    }
}