// =============================================================
// TaskBoardUI.cs
//
// DEPENDENCIES:
// - JobManager: GetRoleCounts(), GetActiveConstructionSiteCount()
// - CivilianRole: expects Gatherer, Builder, Hauler, Idle
// - TeamStorageManager: building-only stored/capacity/reserved totals
// - IMGUIInputBlocker: prevents clicks through the panel
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new roles, update the display line for civilians.
// - If storage logic changes, update the resource lines accordingly.
// =============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskBoardUI : MonoBehaviour
{
    public int playerTeamID = 0;

    [Header("Toggle")]
    public bool show = true;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Panel Layout (Bottom Left)")]
    public int panelWidth = 460;
    public int panelHeight = 320;
    public int marginLeft = 12;
    public int marginBottom = 12;

    [Header("Display")]
    public int maxResourceLines = 14;


    void Start()
    {
        AutoAssignPlayerTeam();
    }

    void AutoAssignPlayerTeam()
    {
        if (GameManager == null)
            return;

        if (GameManager.playerTeam != null)
            playerTeamID = GameManager.playerTeam.teamID;
    }

    GameManager GameManager
    {
        get
        {
            return FindObjectOfType<GameManager>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            show = !show;
    }

    void OnGUI()
    {
        if (!show) return;

        float left = marginLeft;
        float top = Screen.height - panelHeight - marginBottom;

        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);
        IMGUIInputBlocker.Register(panelRect);

        GUI.Box(panelRect, $"TASK BOARD (Team {playerTeamID})");

        float x = panelRect.x + 10;
        float y = panelRect.y + 24;

        if (JobManager.Instance != null)
        {
            Dictionary<CivilianRole, int> counts = JobManager.Instance.GetRoleCounts(playerTeamID);

            GUI.Label(new Rect(x, y, panelWidth - 20, 20),
                BuildRoleSummary(counts));
            y += 22;

            int sites = JobManager.Instance.GetActiveConstructionSiteCount(playerTeamID);
            GUI.Label(new Rect(x, y, panelWidth - 20, 20), $"Construction Sites: {sites}");
            y += 26;
        }
        else
        {
            var counts = BuildFallbackRoleCounts();
            GUI.Label(new Rect(x, y, panelWidth - 20, 20),
                BuildRoleSummary(counts));
            y += 22;
        }

        if (TeamStorageManager.Instance == null)
        {
            GUI.Label(new Rect(x, y, panelWidth - 20, 20), "TeamStorageManager missing (add it to the scene).");
            return;
        }

        GUI.Label(new Rect(x, y, panelWidth - 20, 20), "Stored in Buildings (Stored/Cap)  [Reserved]");
        y += 20;

        int lines = 0;
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            int stored = TeamStorageManager.Instance.GetTotalStoredInBuildings(playerTeamID, t);
            int cap = TeamStorageManager.Instance.GetTotalCapacityInBuildings(playerTeamID, t);
            int reserved = TeamStorageManager.Instance.GetReservedTotal(playerTeamID, t);

            string line = $"{t}: {stored}/{cap}";
            if (reserved > 0) line += $"  [{reserved}]";
            if (cap > 0 && stored >= cap) line += " (FULL)";
            if (cap == 0) line += " (NO STORAGE)";

            GUI.Label(new Rect(x, y, panelWidth - 20, 18), line);
            y += 18;

            lines++;
            if (lines >= maxResourceLines) break;
            if (y > panelRect.yMax - 20) break;
        }

    }
    Dictionary<CivilianRole, int> BuildFallbackRoleCounts()
    {
        var result = new Dictionary<CivilianRole, int>();
        foreach (CivilianRole role in Enum.GetValues(typeof(CivilianRole)))
            result[role] = 0;

        var all = FindObjectsOfType<Civilian>();
        for (int i = 0; i < all.Length; i++)
        {
            var c = all[i];
            if (c == null || c.teamID != playerTeamID) continue;
            if (!result.ContainsKey(c.role)) result[c.role] = 0;
            result[c.role]++;
        }

        return result;
    }

    string BuildRoleSummary(Dictionary<CivilianRole, int> counts)
    {
        var parts = new List<string>();
        foreach (CivilianRole role in Enum.GetValues(typeof(CivilianRole)))
        {
            int value = (counts != null && counts.ContainsKey(role)) ? counts[role] : 0;
            parts.Add($"{role} {value}");
        }

        return "Civilians: " + string.Join(" | ", parts);
    }

}