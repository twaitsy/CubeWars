using UnityEngine;
using System.Collections.Generic;

public class JobManager : MonoBehaviour
{
    public static JobManager Instance;

    private readonly List<Civilian> civilians = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ------------------------------------------------------------------------
    // Registration
    // ------------------------------------------------------------------------

    public void RegisterCivilian(Civilian civ)
    {
        if (civ == null)
            return;

        if (!civilians.Contains(civ))
            civilians.Add(civ);
    }

    public void UnregisterCivilian(Civilian civ)
    {
        if (civ == null)
            return;

        civilians.Remove(civ);
    }

    // ------------------------------------------------------------------------
    // Queries
    // ------------------------------------------------------------------------

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
            if (civ == null)
                continue;

            if (civ.CurrentAssignedSite == site)
                count++;
        }

        return count;
    }

    public Dictionary<CivilianJobType, int> GetJobCounts(int teamID)
    {
        var result = new Dictionary<CivilianJobType, int>();

        foreach (CivilianJobType job in System.Enum.GetValues(typeof(CivilianJobType)))
            result[job] = 0;

        for (int i = 0; i < civilians.Count; i++)
        {
            var civ = civilians[i];
            if (civ == null)
                continue;

            if (civ.teamID != teamID)
                continue;

            result[civ.JobType]++;
        }

        return result;
    }

    // ------------------------------------------------------------------------
    // Crafting helpers
    // ------------------------------------------------------------------------

    public bool AssignCivilianToCraftingBuilding(Civilian civilian, CraftingBuilding building, bool manual = true)
    {
        if (civilian == null || building == null)
            return false;

        if (WorkerTaskDispatcher.Instance == null)
            return false;

        return WorkerTaskDispatcher.Instance.TryAssignTaskToWorker(
            civilian,
            WorkerTaskRequest.Craft(civilian.teamID, building)
        );
    }

    public void SetCraftingBuildingPriority(CraftingBuilding building, int priority)
    {
        if (building == null)
            return;

        building.assignmentPriority = Mathf.Clamp(priority, 0, 10);
    }

    // ------------------------------------------------------------------------
    // Construction (updated to use ConstructionRegistry)
    // ------------------------------------------------------------------------

    public int GetActiveConstructionSiteCount(int teamID)
    {
        int count = 0;

        var sites = ConstructionRegistry.Instance.GetSitesForTeam(teamID);
        for (int i = 0; i < sites.Count; i++)
        {
            var s = sites[i];
            if (s == null)
                continue;

            if (!s.IsComplete)
                count++;
        }

        return count;
    }
}