using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BuildGridOccupant))]

public class ConstructionSite : MonoBehaviour
{
    [Header("Team & Definition")]
    public int teamID;
    public BuildItemDefinition buildItem;   // what we’re building
    public BuildGridCell gridCell;         // cell this site occupies
    private readonly List<BuildGridCell> occupiedCells = new();

    [Header("Build Settings")]
    public float baseBuildTime = 10f;      // fallback if item has no override
    public ResourceCost[] costs;

    // State
    public bool InitOK { get; private set; }
    public bool IsComplete => completed;
    public bool MaterialsComplete => HasAllResources();
    public int SiteKey { get; private set; }

    float buildProgress;
    bool completed;

    // delivered[type] = amount delivered to this site
    Dictionary<ResourceDefinition, int> delivered = new();

    void Awake()
    {
        SiteKey = GetInstanceID();

        if (costs != null)
        {
            foreach (var c in costs)
                delivered[c.resource] = 0;
        }
    }

    void OnEnable()
    {
        if (ConstructionRegistry.Instance != null)
            ConstructionRegistry.Instance.Register(this);
    }

    void OnDisable()
    {
        if (ConstructionRegistry.Instance != null)
            ConstructionRegistry.Instance.Unregister(this);
    }

    void Update()
    {
        if (!InitOK || completed)
            return;

        if (!MaterialsComplete)
            return;

        // Passive build (optional)
        buildProgress += Time.deltaTime;

        float requiredTime = GetBuildTime();
        if (buildProgress >= requiredTime)
            Complete();
        else
            NotifyChanged();
    }

    float GetBuildTime()
    {
        if (buildItem != null && buildItem.buildTime > 0f)
            return buildItem.buildTime;
        return Mathf.Max(0.1f, baseBuildTime);
    }

    bool HasAllResources()
    {
        if (costs == null) return true;

        foreach (var c in costs)
        {
            if (!delivered.TryGetValue(c.resource, out int have) || have < c.amount)
                return false;
        }
        return true;
    }

    // ---------------- INIT ----------------

    public void Init(BuildGridCell cell, int team, BuildItemDefinition item, bool reserveResources)
    {
        Init(cell, team, item, reserveResources, null);
    }

    public void Init(BuildGridCell cell, int team, BuildItemDefinition item, bool reserveResources, List<BuildGridCell> footprintCells)
    {
        teamID = team;
        gridCell = cell;
        buildItem = item;
        costs = item != null ? item.costs : costs;

        occupiedCells.Clear();
        if (footprintCells != null && footprintCells.Count > 0)
            occupiedCells.AddRange(footprintCells);
        else if (cell != null)
            occupiedCells.Add(cell);

        delivered.Clear();
        if (costs != null)
        {
            foreach (var c in costs)
                delivered[c.resource] = 0;
        }

        // Reserve resources
        if (reserveResources && TeamStorageManager.Instance != null && costs != null && costs.Length > 0)
        {
            bool ok = TeamStorageManager.Instance.ReserveForSite(teamID, SiteKey, costs);
            if (!ok)
            {
                InitOK = false;
                NotifyChanged();
                return;
            }
        }

        InitOK = true;
        NotifyChanged();
    }

    // ---------------- WORK / PROGRESS ----------------

    public void AddWork(float workAmount)
    {
        if (!InitOK || completed) return;
        if (!MaterialsComplete) return;

        buildProgress += workAmount;

        float requiredTime = GetBuildTime();
        if (buildProgress >= requiredTime)
            Complete();
        else
            NotifyChanged();
    }

    // ---------------- RESOURCE DELIVERY ----------------

    public int GetMissing(ResourceDefinition type)
    {
        if (costs == null) return 0;

        int required = 0;
        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i].resource == type)
            {
                required = costs[i].amount;
                break;
            }
        }

        delivered.TryGetValue(type, out int have);
        return Mathf.Max(0, required - have);
    }

    public int ReceiveDelivery(ResourceDefinition type, int amount)
    {
        if (amount <= 0) return 0;

        int missing = GetMissing(type);
        if (missing <= 0) return 0;

        int accepted = Mathf.Min(missing, amount);

        if (!delivered.ContainsKey(type))
            delivered[type] = 0;

        delivered[type] += accepted;

        NotifyChanged();
        return accepted;
    }

    public ResourceCost[] GetRequiredCosts() => costs;

    public int GetDeliveredAmount(ResourceDefinition type) =>
        delivered.TryGetValue(type, out int v) ? v : 0;

    // ---------------- COMPLETE ----------------

    void Complete()
    {
        if (completed) return;
        completed = true;

        if (TeamStorageManager.Instance != null)
            TeamStorageManager.Instance.ReleaseReservation(teamID, SiteKey);

        if (buildItem != null && buildItem.prefab != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            GameObject placed = Instantiate(buildItem.prefab, pos, rot);

            BuildItemInstance bii = placed.GetComponent<BuildItemInstance>();
            if (bii == null) bii = placed.AddComponent<BuildItemInstance>();
            bii.itemId = buildItem.name;

            ApplyTeamToPlacedObject(placed, teamID);

            BuildGridOccupant occ = placed.GetComponent<BuildGridOccupant>();
            if (occ == null) occ = placed.AddComponent<BuildGridOccupant>();
            occ.SetOccupiedCells(occupiedCells, placed);
        }

        // Notify completion before destruction
        NotifyChanged();

        // This destroys ONLY the ConstructionSite, not the Team.
        Destroy(gameObject);
    }

    void ApplyTeamToPlacedObject(GameObject placed, int team)
    {
        if (placed == null) return;

        TeamAssignmentUtility.ApplyTeamToHierarchy(placed, team);
    }

    // ---------------- UI HELPERS ----------------

    public float Progress01 =>
        GetBuildTime() > 0f ? Mathf.Clamp01(buildProgress / GetBuildTime()) : 1f;

    public string GetStatusLine()
    {
        if (completed) return "Completed";
        if (!MaterialsComplete) return "Awaiting Materials";
        return "Under Construction";
    }

    public int AssignedBuilderCount =>
        JobManager.Instance != null ? JobManager.Instance.CountBuildersOnSite(this) : 0;

    // ---------------- Registry Notification ----------------

    void NotifyChanged()
    {
        if (ConstructionRegistry.Instance != null)
            ConstructionRegistry.Instance.NotifySiteChanged(this);
    }
}