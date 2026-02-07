using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles AI building placement for a single team.
/// 
/// DEPENDENCIES:
/// - BuildGridCell
/// - BuildPlacementManager
/// - BuildCatalog
/// - TeamResources
///
/// RESPONSIBILITIES:
/// - Periodically attempt to place buildings
/// - Choose affordable items
/// - Respect category priorities
///
/// IMPORTANT:
/// - Does NOT delete teams
/// - Does NOT modify team objects
/// </summary>
public class TeamAIBuild : MonoBehaviour
{
    [Header("Team")]
    public int teamID = 1;
    public int playerTeamID = 0;

    [Header("Build Settings")]
    public BuildCatalog catalog;
    public float buildInterval = 4f;

    [Tooltip("Categories the AI will try in order.")]
    public List<string> categoryPriority = new List<string> { "Economy", "Industry", "Housing", "Tech" };

    private float timer;

    void Update()
    {
        if (teamID == playerTeamID) return;
        if (catalog == null) return;

        timer += Time.deltaTime;
        if (timer < buildInterval) return;
        timer = 0f;

        TryBuildSomething();
    }

    void TryBuildSomething()
    {
        BuildGridCell cell = FindFreeCellForTeam(teamID);
        if (cell == null) return;

        BuildItemDefinition chosen = ChooseAffordableItem(teamID);
        if (chosen == null) return;

        if (BuildPlacementManager.Instance != null)
            BuildPlacementManager.Instance.TryPlace(cell, chosen);
    }

    BuildGridCell FindFreeCellForTeam(int t)
    {
        var cells = FindObjectsOfType<BuildGridCell>();
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].teamID == t && !cells[i].isOccupied)
                return cells[i];
        }
        return null;
    }

    BuildItemDefinition ChooseAffordableItem(int t)
    {
        foreach (var cat in categoryPriority)
        {
            foreach (var item in catalog.items)
            {
                if (item == null) continue;
                if (!CategoryMatch(item.category, cat)) continue;

                if (TeamResources.Instance == null || TeamResources.Instance.CanAfford(t, item.costs))
                    return item;
            }
        }

        foreach (var item in catalog.items)
        {
            if (item == null) continue;
            if (TeamResources.Instance == null || TeamResources.Instance.CanAfford(t, item.costs))
                return item;
        }

        return null;
    }

    bool CategoryMatch(string a, string b)
    {
        string na = string.IsNullOrWhiteSpace(a) ? "" : a.Trim().ToLowerInvariant();
        string nb = string.IsNullOrWhiteSpace(b) ? "" : b.Trim().ToLowerInvariant();
        return na == nb;
    }
}