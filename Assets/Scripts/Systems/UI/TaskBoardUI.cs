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
    public int panelHeight = 380;
    public int marginLeft = 12;
    public int marginBottom = 12;

    [Header("Display")]
    public int maxResourceLines = 10;
    public int maxNotificationLines = 6;

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

    GameManager GameManager => FindObjectOfType<GameManager>();

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleShow();
    }

    void ToggleShow()
    {
        show = !show;

        if (RTSGameSettings.Instance != null)
            RTSGameSettings.Instance.display.showTaskBoard = show;
    }

    void OnGUI()
    {
        if (!show) return;

        float scale = RTSGameSettings.UIScale;
        float width = panelWidth * scale;
        float height = panelHeight * scale;

        float left = marginLeft;
        float top = Screen.height - height - marginBottom;

        Rect panelRect = new Rect(left, top, width, height);
        IMGUIInputBlocker.Register(panelRect);

        GUI.Box(panelRect, $"TASK BOARD | Team {playerTeamID}");

        float x = panelRect.x + (10 * scale);
        float y = panelRect.y + (24 * scale);
        float row = 18 * scale;

        DrawRolesAndSites(x, ref y, panelRect.width - 20 * scale, row);
        DrawResources(x, ref y, panelRect.width - 20 * scale, row);
        DrawNotifications(x, ref y, panelRect.width - 20 * scale, row);
    }

    void DrawRolesAndSites(float x, ref float y, float width, float row)
    {
        if (JobManager.Instance != null)
        {
            Dictionary<CivilianRole, int> counts = JobManager.Instance.GetRoleCounts(playerTeamID);
            GUI.Label(new Rect(x, y, width, row), BuildRoleSummary(counts));
            y += row;

            int sites = JobManager.Instance.GetActiveConstructionSiteCount(playerTeamID);
            GUI.Label(new Rect(x, y, width, row), $"Construction Sites: {sites}");
            y += row + 8;
        }
        else
        {
            GUI.Label(new Rect(x, y, width, row), BuildRoleSummary(BuildFallbackRoleCounts()));
            y += row + 8;
        }
    }

    void DrawResources(float x, ref float y, float width, float row)
    {
        GUI.Label(new Rect(x, y, width, row), "Resources (Stored/Cap) [Reserved]");
        y += row;

        if (TeamStorageManager.Instance == null)
        {
            GUI.Label(new Rect(x, y, width, row), "TeamStorageManager missing.");
            y += row;
            return;
        }

        int lines = 0;
        foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
        {
            int stored = TeamStorageManager.Instance.GetTotalStoredInBuildings(playerTeamID, t);
            int cap = TeamStorageManager.Instance.GetTotalCapacityInBuildings(playerTeamID, t);
            int reserved = TeamStorageManager.Instance.GetReservedTotal(playerTeamID, t);

            string line = $"{t}: {stored}/{cap}";
            if (reserved > 0) line += $" [{reserved}]";
            if (cap > 0 && stored >= cap) line += " FULL";
            if (cap == 0) line += " NO STORAGE";

            GUI.Label(new Rect(x, y, width, row), line);
            y += row;

            lines++;
            if (lines >= maxResourceLines) break;
        }

        y += 6;
    }

    void DrawNotifications(float x, ref float y, float width, float row)
    {
        GUI.Label(new Rect(x, y, width, row), "Recent Notifications");
        y += row;

        List<string> recent = AlertManager.Instance != null
            ? AlertManager.Instance.GetRecent(maxNotificationLines)
            : new List<string>();

        if (recent.Count == 0)
        {
            GUI.Label(new Rect(x, y, width, row), "No recent notifications.");
            return;
        }

        for (int i = recent.Count - 1; i >= 0; i--)
        {
            GUI.Label(new Rect(x, y, width, row), $"• {recent[i]}");
            y += row;
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
