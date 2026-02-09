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
        if (PlayerTeamResolver.TryGetPlayerTeamID(out int teamID))
            playerTeamID = teamID;
    }

    void Update()
    {
        AutoAssignPlayerTeam();

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

            int gatherers = counts.ContainsKey(CivilianRole.Gatherer) ? counts[CivilianRole.Gatherer] : 0;
            int builders = counts.ContainsKey(CivilianRole.Builder) ? counts[CivilianRole.Builder] : 0;
            int haulers = counts.ContainsKey(CivilianRole.Hauler) ? counts[CivilianRole.Hauler] : 0;
            int idles = counts.ContainsKey(CivilianRole.Idle) ? counts[CivilianRole.Idle] : 0;

            GUI.Label(new Rect(x, y, panelWidth - 20, 20),
                $"Civilians: Gatherer {gatherers} | Builder {builders} | Hauler {haulers} | Idle {idles}");
            y += 22;

            int sites = JobManager.Instance.GetActiveConstructionSiteCount(playerTeamID);
            GUI.Label(new Rect(x, y, panelWidth - 20, 20), $"Construction Sites: {sites}");
            y += 26;
        }
        else
        {
            int gatherers = 0, builders = 0, haulers = 0, idles = 0;
            var all = FindObjectsOfType<Civilian>();
            for (int i = 0; i < all.Length; i++)
            {
                var c = all[i];
                if (c == null) continue;
                if (c.teamID != playerTeamID) continue;
                switch (c.role)
                {
                    case CivilianRole.Gatherer: gatherers++; break;
                    case CivilianRole.Builder: builders++; break;
                    case CivilianRole.Hauler: haulers++; break;
                    case CivilianRole.Idle: idles++; break;
                }
            }

            GUI.Label(new Rect(x, y, panelWidth - 20, 20),
                $"Civilians: Gatherer {gatherers} | Builder {builders} | Hauler {haulers} | Idle {idles}");
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

            if (stored == 0 && cap == 0 && reserved == 0) continue;

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

        if (lines == 0)
            GUI.Label(new Rect(x, y, panelWidth - 20, 18), "No storage buildings registered for this team.");
    }
}