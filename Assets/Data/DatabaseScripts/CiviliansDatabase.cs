using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Civilians")]
public class CiviliansDatabase : ScriptableObject
{
    public List<CivilianDefinition> civilians = new();

    public bool TryGetById(string id, out CivilianDefinition def)
    {
        def = null;
        if (string.IsNullOrWhiteSpace(id))
            return false;

        string target = id.Trim().ToLowerInvariant();
        for (int i = 0; i < civilians.Count; i++)
        {
            CivilianDefinition item = civilians[i];
            if (item == null || string.IsNullOrWhiteSpace(item.id))
                continue;

            if (item.id.Trim().ToLowerInvariant() == target)
            {
                def = item;
                return true;
            }
        }

        return false;
    }
}
