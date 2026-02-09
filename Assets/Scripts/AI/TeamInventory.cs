// =============================================================
// TeamInventory.cs
//
// PURPOSE:
// - Stores per-team tool counts (e.g., equipment, crafting tools).
//
// DEPENDENCIES:
// - ToolItem:
//      * Represents a tool type.
// - Team.cs:
//      * teamID determines which inventory bucket to use.
//
// NOTES FOR FUTURE MAINTENANCE:
// - This script uses a global singleton pattern.
//   DO NOT attach this script to multiple objects.
// - Consider replacing the singleton with a per-team component.
// - Inventory is stored in dictionaries for fast lookup.
//
// IMPORTANT:
// - This script does NOT delete teams.
// =============================================================

using System.Collections.Generic;
using UnityEngine;

public class TeamInventory : MonoBehaviour
{
    public static TeamInventory Instance;

    [Header("Teams")]
    public int teamCount = 6;

    private Dictionary<int, Dictionary<ToolItem, int>> tools =
        new Dictionary<int, Dictionary<ToolItem, int>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate TeamInventory found on team object. Keeping existing singleton and removing duplicate component.");
            Destroy(this);
            return;
        }

        Instance = this;

        for (int t = 0; t < teamCount; t++)
            tools[t] = new Dictionary<ToolItem, int>();
    }

    public int GetToolCount(int teamID, ToolItem item)
    {
        if (item == null) return 0;
        if (!tools.TryGetValue(teamID, out var dict)) return 0;
        dict.TryGetValue(item, out int c);
        return c;
    }

    public void AddTool(int teamID, ToolItem item, int amount)
    {
        if (item == null || amount <= 0) return;
        if (!tools.TryGetValue(teamID, out var dict))
        {
            dict = new Dictionary<ToolItem, int>();
            tools[teamID] = dict;
        }

        dict.TryGetValue(item, out int c);
        dict[item] = c + amount;
    }

    public bool RemoveTool(int teamID, ToolItem item, int amount)
    {
        if (item == null || amount <= 0) return false;
        if (!tools.TryGetValue(teamID, out var dict)) return false;
        dict.TryGetValue(item, out int c);
        if (c < amount) return false;
        c -= amount;
        if (c <= 0) dict.Remove(item);
        else dict[item] = c;
        return true;
    }
}