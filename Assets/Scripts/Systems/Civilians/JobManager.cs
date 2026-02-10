// =============================================================
// JobManager.cs
//
// DEPENDENCIES:
// - Civilian: registered/unregistered here, roles tracked
// - CivilianRole: enum of roles (Gatherer, Builder, Hauler, Idle)
// - ConstructionSite: used for CountBuildersOnSite + GetActiveConstructionSiteCount
// - TaskBoardUI: calls GetRoleCounts() and GetActiveConstructionSiteCount()
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new roles, CivilianRole and any UI must be updated.
// - Keep civilians list in sync with Civilian.OnEnable/OnDisable.
// =============================================================

using UnityEngine;
using System.Collections.Generic;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance;

    private readonly List<Civilian> civilians = new List<Civilian>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterCivilian(Civilian civ)
    {
        if (civ == null) return;
        if (!civilians.Contains(civ))
            civilians.Add(civ);
    }

    public void UnregisterCivilian(Civilian civ)
    {
        if (civ == null) return;
        civilians.Remove(civ);
    }

    public bool IsCivilianAssigned(Civilian civ)
    {
        return civ != null && civ.HasJob;
    }

    public int CountBuildersOnSite(ConstructionSite site)
    {
        int count = 0;
        for (int i = 0; i < civilians.Count; i++)
        {
            var civ = civilians[i];
            if (civ == null) continue;
            if (civ.CurrentAssignedSite == site)
                count++;
        }
        return count;
    }

    public Dictionary<CivilianRole, int> GetRoleCounts(int teamID)
    {
        var result = new Dictionary<CivilianRole, int>();

        foreach (CivilianRole r in System.Enum.GetValues(typeof(CivilianRole)))
            result[r] = 0;

        for (int i = 0; i < civilians.Count; i++)
        {
            var civ = civilians[i];
            if (civ == null) continue;
            if (civ.teamID != teamID) continue;

            if (!result.ContainsKey(civ.role))
                result[civ.role] = 0;

            result[civ.role]++;
        }

        return result;
    }

    public bool AssignCivilianToCraftingBuilding(Civilian civilian, CraftingBuilding building, bool manual = true)
    {
        if (civilian == null || building == null)
            return false;

        if (CraftingJobManager.Instance == null)
            return false;

        return CraftingJobManager.Instance.TryAssignManually(civilian, building);
    }

    public void SetCraftingBuildingPriority(CraftingBuilding building, int priority)
    {
        if (building == null)
            return;

        building.assignmentPriority = Mathf.Clamp(priority, 0, 10);
    }

    public int GetActiveConstructionSiteCount(int teamID)
    {
        int count = 0;
        var sites = GameObject.FindObjectsOfType<ConstructionSite>();
        for (int i = 0; i < sites.Length; i++)
        {
            var s = sites[i];
            if (s == null) continue;
            if (s.teamID != teamID) continue;
            if (s.IsComplete) continue;
            count++;
        }
        return count;
    }
}