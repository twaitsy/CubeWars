using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskBoardUI : MonoBehaviour
{
    public int playerTeamID = 0;

    [Header("Toggle")]
    public bool show = false;
    public KeyCode toggleKey = KeyCode.Tab;

    [Header("Panel Layout (Bottom Left)")]
    public int panelWidth = 720;
    public int panelHeight = 620;
    public int marginLeft = 12;
    public int marginBottom = 12;

    [Header("Typography")]
    [Min(0)] public int fontSizeBoost = 1;

    [Header("Display")]
    public int maxResourceLines = 16;
    public int maxNotificationLines = 8;

    void Start() => AutoAssignPlayerTeam();

    void AutoAssignPlayerTeam()
    {
        if (GameManager == null)
            return;

        if (GameManager.playerTeam != null)
            playerTeamID = GameManager.playerTeam.teamID;
    }

    GameManager GameManager => FindFirstObjectByType<GameManager>();

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

        Rect panelRect = new(left, top, width, height);
        IMGUIInputBlocker.Register(panelRect);

        int prevLabelSize = GUI.skin.label.fontSize;
        int prevBoxSize = GUI.skin.box.fontSize;
        int prevButtonSize = GUI.skin.button.fontSize;
        GUI.skin.label.fontSize = Mathf.Max(10, prevLabelSize + fontSizeBoost);
        GUI.skin.box.fontSize = Mathf.Max(10, prevBoxSize + fontSizeBoost);
        GUI.skin.button.fontSize = Mathf.Max(10, prevButtonSize + fontSizeBoost);

        GUI.Box(panelRect, $"TASK BOARD | Team {playerTeamID}");

        float x = panelRect.x + (10 * scale);
        float y = panelRect.y + (24 * scale);
        float row = 18 * scale;
        float sectionGap = 8 * scale;
        float sectionWidth = panelRect.width - 20 * scale;

        DrawRolesAndStateBreakdown(x, ref y, sectionWidth, row);
        y += sectionGap;
        DrawSceneJobSummary(x, ref y, sectionWidth, row);
        y += sectionGap;
        DrawResources(x, ref y, sectionWidth, row);
        y += sectionGap;
        DrawNotifications(x, ref y, sectionWidth, row);

        GUI.skin.label.fontSize = prevLabelSize;
        GUI.skin.box.fontSize = prevBoxSize;
        GUI.skin.button.fontSize = prevButtonSize;
    }

    void DrawRolesAndStateBreakdown(float x, ref float y, float width, float row)
    {
        GUI.Label(new Rect(x, y, width, row), "Civilian Workforce");
        y += row;

        Civilian[] civilians = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
        int teamTotal = 0;
        int waitingForJob = 0;
        int movingToTarget = 0;
        int collectingResource = 0;
        int producingGoods = 0;

        for (int i = 0; i < civilians.Length; i++)
        {
            Civilian civ = civilians[i];
            if (civ == null || civ.teamID != playerTeamID)
                continue;

            teamTotal++;
            string state = civ.CurrentState;

            if (state == "Idle")
                waitingForJob++;

            if (state.StartsWith("GoingTo") || state.StartsWith("Seeking"))
                movingToTarget++;

            if (state == "Gathering" || state == "PickingUp" || state == "CollectingCraftOutput")
                collectingResource++;

            if (state == "CraftingAtWorkPoint")
                producingGoods++;
        }

        Dictionary<CivilianRole, int> counts = JobManager.Instance != null
            ? JobManager.Instance.GetRoleCounts(playerTeamID)
            : BuildFallbackRoleCounts();

        GUI.Label(new Rect(x, y, width, row), BuildRoleSummary(counts));
        y += row;

        GUI.Label(new Rect(x, y, width, row), $"Active Civilians: {teamTotal} | Waiting For Job: {waitingForJob} | Moving To Target: {movingToTarget}");
        y += row;
        GUI.Label(new Rect(x, y, width, row), $"Collecting Resource: {collectingResource} | Producing Goods: {producingGoods}");
        y += row;
    }

    void DrawSceneJobSummary(float x, ref float y, float width, float row)
    {
        GUI.Label(new Rect(x, y, width, row), "Scene Job Board");
        y += row;

        ResourceNode[] nodes = FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
        int activeNodes = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null && nodes[i].remaining > 0)
                activeNodes++;
        }

        Civilian[] civilians = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
        int nodeJobsFulfilled = 0;
        int buildersWorking = 0;
        int haulersWorking = 0;

        for (int i = 0; i < civilians.Length; i++)
        {
            Civilian civ = civilians[i];
            if (civ == null || civ.teamID != playerTeamID)
                continue;

            if (civ.CurrentState == "Gathering" || civ.CurrentState == "GoingToNode")
                nodeJobsFulfilled++;

            if (civ.CurrentState == "Building" || civ.CurrentState == "GoingToBuildSite")
                buildersWorking++;

            if (IsHaulerActiveState(civ.CurrentState))
                haulersWorking++;
        }

        ConstructionSite[] sites = FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None);
        int activeSites = 0;
        int sitesAwaitingMaterials = 0;
        int sitesUnderConstruction = 0;

        for (int i = 0; i < sites.Length; i++)
        {
            ConstructionSite site = sites[i];
            if (site == null || site.teamID != playerTeamID || site.IsComplete)
                continue;

            activeSites++;
            if (site.MaterialsComplete) sitesUnderConstruction++;
            else sitesAwaitingMaterials++;
        }

        CraftingBuilding[] craftBuildings = FindObjectsByType<CraftingBuilding>(FindObjectsSortMode.None);
        int craftingActive = 0;
        int craftingWaitingInputs = 0;
        int craftingWaitingPickup = 0;

        for (int i = 0; i < craftBuildings.Length; i++)
        {
            CraftingBuilding building = craftBuildings[i];
            if (building == null || building.teamID != playerTeamID)
                continue;

            if (building.State == CraftingBuilding.ProductionState.InProgress)
                craftingActive++;
            else if (building.State == CraftingBuilding.ProductionState.WaitingForInputs)
                craftingWaitingInputs++;
            else if (building.State == CraftingBuilding.ProductionState.WaitingForPickup || building.State == CraftingBuilding.ProductionState.OutputReady)
                craftingWaitingPickup++;
        }

        GUI.Label(new Rect(x, y, width, row), $"Node Jobs: {activeNodes} total | Fulfilled: {nodeJobsFulfilled}");
        y += row;
        GUI.Label(new Rect(x, y, width, row), $"Construction Jobs: {activeSites} total | Awaiting Materials: {sitesAwaitingMaterials} | Building: {sitesUnderConstruction}");
        y += row;
        GUI.Label(new Rect(x, y, width, row), $"Builders Active: {buildersWorking} | Haulers Active: {haulersWorking}");
        y += row;
        GUI.Label(new Rect(x, y, width, row), $"Crafting: In Progress {craftingActive} | Waiting Inputs {craftingWaitingInputs} | Waiting Pickup {craftingWaitingPickup}");
        y += row;

        int queuedTasks = WorkerTaskDispatcher.Instance != null ? WorkerTaskDispatcher.Instance.GetQueuedTaskCount(playerTeamID) : 0;
        int registeredWorkers = WorkerTaskDispatcher.Instance != null ? WorkerTaskDispatcher.Instance.GetRegisteredWorkerCount(playerTeamID) : 0;
        GUI.Label(new Rect(x, y, width, row), $"Dispatcher: Registered Workers {registeredWorkers} | Queued Tasks {queuedTasks}");
        y += row;
    }


    static bool IsHaulerActiveState(string state)
    {
        return state == "PickingUp"
            || state == "Delivering"
            || state == "GoingToDeliverSite"
            || state == "SearchingSupplySite"
            || state == "GoingToPickupStorage"
            || state == "FetchingCraftInput"
            || state == "DeliveringCraftInput"
            || state == "CollectingCraftOutput"
            || state == "DeliveringCraftOutput";
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

        ResourcesDatabase db = ResourcesDatabase.Instance;

        if (db == null || db.resources == null || db.resources.Count == 0)
        {
            GUI.Label(new Rect(x, y, width, row), "No resources in database.");
            y += row;
            return;
        }

        int lines = 0;

        foreach (ResourceDefinition def in db.resources)
        {
            int stored = TeamStorageManager.Instance.GetTotalStoredInBuildings(playerTeamID, def);
            int cap = TeamStorageManager.Instance.GetTotalCapacityInBuildings(playerTeamID, def);
            int reserved = TeamStorageManager.Instance.GetReservedTotal(playerTeamID, def);
            int free = Mathf.Max(0, cap - stored);

            string line = $"{def.displayName}: {stored}/{cap} | Free {free}";
            if (reserved > 0) line += $" | Reserved {reserved}";
            if (cap > 0 && stored >= cap) line += " | FULL";
            if (cap == 0) line += " | NO STORAGE";

            GUI.Label(new Rect(x, y, width, row), line);
            y += row;

            lines++;
            if (lines >= maxResourceLines)
                break;
        }
    }

    void DrawNotifications(float x, ref float y, float width, float row)
    {
        y += 6;
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

        Civilian[] all = FindObjectsByType<Civilian>(FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++)
        {
            Civilian c = all[i];
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

        return "Roles: " + string.Join(" | ", parts);
    }
}
