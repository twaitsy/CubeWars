// =============================================================
// TeamStorageManager.cs
//
// PURPOSE:
// - Central storage backend for all teams.
// - Tracks capacity, stored amounts, reservations, and withdrawals.
//
// DEPENDENCIES:
// - ResourceStorageContainer:
//      * Actual per-building storage.
// - ResourceType / ResourceCost:
//      * Defines resource kinds and costs.
// - TeamResources:
//      * Calls into this for all resource logic.
// - Building / Unit / Civilian:
//      * Excluded from building-only storage queries.
// - ConstructionSite:
//      * Uses reservation APIs.
// - Team.cs:
//      * teamID determines which storage bucket to use.
//
// NOTES FOR FUTURE MAINTENANCE:
// - This script uses a global singleton pattern.
//   DO NOT attach this script to multiple objects.
// - If you add new storage types, update IsBuildingStorage().
// - Withdraw/Deposit must remain consistent with capacity logic.
//
// IMPORTANT:
// - This script does NOT delete teams.
// - It ONLY deletes duplicate TeamStorageManager components,
//   NOT Team GameObjects.
// =============================================================

using System.Collections.Generic;
using UnityEngine;

public class TeamStorageManager : MonoBehaviour
{
    public static TeamStorageManager Instance;

    [Header("Default Team Storage (virtual, used when no buildings are registered)")]
    public int defaultCapacityPerType = 0;

    [Tooltip("If true, building-only totals (TaskBoard/Inspector) include virtual baseline storage.")]
    public bool includeBaselineInBuildingOnlyTotals = false;

    private readonly Dictionary<int, List<ResourceStorageContainer>> storages =
        new Dictionary<int, List<ResourceStorageContainer>>();

    private readonly Dictionary<int, ResourceStorageContainer> primaryStorage =
        new Dictionary<int, ResourceStorageContainer>();

    private readonly Dictionary<int, Dictionary<ResourceType, int>> siteReserved =
        new Dictionary<int, Dictionary<ResourceType, int>>();

    private readonly Dictionary<int, Dictionary<ResourceType, int>> reservedTotals =
        new Dictionary<int, Dictionary<ResourceType, int>>();

    private readonly Dictionary<int, Dictionary<ResourceType, int>> baselineStored =
        new Dictionary<int, Dictionary<ResourceType, int>>();

    private readonly Dictionary<int, Dictionary<ResourceType, int>> baselineCapacity =
        new Dictionary<int, Dictionary<ResourceType, int>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[TeamStorageManager] Duplicate detected, destroying extra instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void EnsureTeam(int teamID)
    {
        if (!storages.ContainsKey(teamID))
            storages[teamID] = new List<ResourceStorageContainer>();

        if (!reservedTotals.ContainsKey(teamID))
        {
            reservedTotals[teamID] = new Dictionary<ResourceType, int>();
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
                reservedTotals[teamID][t] = 0;
        }

        if (!baselineStored.ContainsKey(teamID))
        {
            baselineStored[teamID] = new Dictionary<ResourceType, int>();
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
                baselineStored[teamID][t] = 0;
        }

        if (!baselineCapacity.ContainsKey(teamID))
        {
            baselineCapacity[teamID] = new Dictionary<ResourceType, int>();
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
                baselineCapacity[teamID][t] = Mathf.Max(0, defaultCapacityPerType);
        }
    }

    // ------------------- Storage Registration -------------------

    public void Register(ResourceStorageContainer c)
    {
        if (c == null) return;

        RemoveFromAllTeams(c);
        EnsureTeam(c.teamID);

        if (!storages[c.teamID].Contains(c))
            storages[c.teamID].Add(c);

        if (!primaryStorage.ContainsKey(c.teamID) || primaryStorage[c.teamID] == null)
            primaryStorage[c.teamID] = c;
    }

    public void Unregister(ResourceStorageContainer c)
    {
        if (c == null) return;
        RemoveFromAllTeams(c);
    }

    void RemoveFromAllTeams(ResourceStorageContainer c)
    {
        foreach (var kv in storages)
            kv.Value.Remove(c);

        var keys = new List<int>(primaryStorage.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            int team = keys[i];
            if (primaryStorage.TryGetValue(team, out var p) && p == c)
            {
                if (storages.TryGetValue(team, out var list) && list.Count > 0)
                    primaryStorage[team] = list[0];
                else
                    primaryStorage[team] = null;
            }
        }
    }

    public ResourceStorageContainer GetPrimaryStorage(int teamID)
    {
        EnsureTeam(teamID);

        if (primaryStorage.TryGetValue(teamID, out var p) && p != null)
            return p;

        var list = storages[teamID];
        if (list.Count > 0)
        {
            primaryStorage[teamID] = list[0];
            return list[0];
        }

        return null;
    }

    // ------------------- Totals (All Storages) -------------------

    public int GetTotalStored(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetStored(type);

        return sum + baselineStored[teamID][type];
    }

    public int GetTotalCapacity(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetCapacity(type);

        return sum + baselineCapacity[teamID][type];
    }

    public int GetTotalFree(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetFree(type);

        if (includeBaselineInBuildingOnlyTotals)
            sum += Mathf.Max(0, baselineCapacity[teamID][type] - baselineStored[teamID][type]);

        return sum;
    }

    // ------------------- Building-only Totals -------------------

    bool IsBuildingStorage(ResourceStorageContainer s)
    {
        if (s == null) return false;

        if (s.GetComponentInParent<Civilian>() != null) return false;
        if (s.GetComponentInParent<Unit>() != null) return false;

        return s.GetComponentInParent<Building>() != null;
    }

    public int GetTotalStoredInBuildings(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetStored(type);
        }

        if (includeBaselineInBuildingOnlyTotals)
            sum += baselineStored[teamID][type];

        return sum;
    }

    public int GetTotalCapacityInBuildings(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetCapacity(type);
        }

        if (includeBaselineInBuildingOnlyTotals)
            sum += baselineCapacity[teamID][type];

        return sum;
    }

    public int GetTotalFreeInBuildings(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetFree(type);
        }

        if (includeBaselineInBuildingOnlyTotals)
            sum += Mathf.Max(0, baselineCapacity[teamID][type] - baselineStored[teamID][type]);

        return sum;
    }

    // ------------------- Capacity Management -------------------

    public void AddCapacity(int teamID, ResourceType type, int amount)
    {
        EnsureTeam(teamID);
        if (amount == 0) return;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            s.AddCapacity(type, amount);
        }
    }

    public void RemoveCapacity(int teamID, ResourceType type, int amount)
    {
        EnsureTeam(teamID);
        if (amount == 0) return;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            s.AddCapacity(type, -amount);
        }
    }

    // ------------------- Reservations -------------------

    public int GetReservedTotal(int teamID, ResourceType type)
    {
        EnsureTeam(teamID);
        return reservedTotals[teamID][type];
    }

    public int GetReservedForSite(int siteKey, ResourceType type)
    {
        if (!siteReserved.TryGetValue(siteKey, out var dict) || dict == null)
            return 0;

        return dict.TryGetValue(type, out var v) ? Mathf.Max(0, v) : 0;
    }

    public int GetAvailable(int teamID, ResourceType type)
    {
        int stored = GetTotalStored(teamID, type);
        int reserved = GetReservedTotal(teamID, type);
        return Mathf.Max(0, stored - reserved);
    }

    public bool CanAffordAvailable(int teamID, ResourceCost[] costs)
    {
        if (costs == null) return true;

        for (int i = 0; i < costs.Length; i++)
        {
            var c = costs[i];
            if (GetAvailable(teamID, c.type) < c.amount)
                return false;
        }
        return true;
    }

    public bool ReserveForSite(int teamID, int siteKey, ResourceCost[] costs)
    {
        if (!CanAffordAvailable(teamID, costs)) return false;

        EnsureTeam(teamID);

        if (!siteReserved.TryGetValue(siteKey, out var dict))
        {
            dict = new Dictionary<ResourceType, int>();
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
                dict[t] = 0;
            siteReserved[siteKey] = dict;
        }

        for (int i = 0; i < costs.Length; i++)
        {
            var c = costs[i];
            dict[c.type] += c.amount;
            reservedTotals[teamID][c.type] += c.amount;
        }

        return true;
    }

    public void ReleaseReservation(int teamID, int siteKey)
    {
        EnsureTeam(teamID);

        if (!siteReserved.TryGetValue(siteKey, out var dict))
            return;

        foreach (var kv in dict)
            reservedTotals[teamID][kv.Key] = Mathf.Max(0, reservedTotals[teamID][kv.Key] - kv.Value);

        siteReserved.Remove(siteKey);
    }

    public int ConsumeReserved(int teamID, int siteKey, ResourceType type, int amount)
    {
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        if (!siteReserved.TryGetValue(siteKey, out var dict))
            return 0;

        int can = Mathf.Min(amount, dict[type]);
        dict[type] -= can;
        reservedTotals[teamID][type] = Mathf.Max(0, reservedTotals[teamID][type] - can);
        return can;
    }

    // ------------------- Withdraw / Deposit -------------------

    public int Withdraw(int teamID, ResourceType type, int amount)
    {
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        int remaining = amount;
        int takenTotal = 0;

        int baselineTake = Mathf.Min(remaining, baselineStored[teamID][type]);
        if (baselineTake > 0)
        {
            baselineStored[teamID][type] -= baselineTake;
            takenTotal += baselineTake;
            remaining -= baselineTake;
        }

        var list = storages[teamID];
        for (int i = 0; i < list.Count && remaining > 0; i++)
        {
            var s = list[i];
            if (s == null) continue;

            int took = s.Withdraw(type, remaining);
            takenTotal += took;
            remaining -= took;
        }

        return takenTotal;
    }

    public int Deposit(int teamID, ResourceType type, int amount)
    {
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        int remaining = amount;
        int acceptedTotal = 0;

        int baselineFree = Mathf.Max(0, baselineCapacity[teamID][type] - baselineStored[teamID][type]);
        int baselineAccepted = Mathf.Min(remaining, baselineFree);
        if (baselineAccepted > 0)
        {
            baselineStored[teamID][type] += baselineAccepted;
            acceptedTotal += baselineAccepted;
            remaining -= baselineAccepted;
        }

        var list = storages[teamID];
        for (int i = 0; i < list.Count && remaining > 0; i++)
        {
            var s = list[i];
            if (s == null) continue;

            int accepted = s.Deposit(type, remaining);
            acceptedTotal += accepted;
            remaining -= accepted;
        }

        return acceptedTotal;
    }

    // ------------------- Queries -------------------

    public bool HasAnyPhysicalStorage(int teamID)
    {
        EnsureTeam(teamID);
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) return true;
        return false;
    }

    public ResourceStorageContainer FindNearestStorageWithFree(int teamID, ResourceType type, Vector3 pos)
    {
        EnsureTeam(teamID);

        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (s.GetFree(type) <= 0) continue;

            float d = (s.transform.position - pos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = s;
            }
        }

        return best;
    }

    public ResourceStorageContainer FindNearestStorageWithStored(int teamID, ResourceType type, Vector3 pos)
    {
        EnsureTeam(teamID);

        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (s.GetStored(type) <= 0) continue;

            float d = (s.transform.position - pos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = s;
            }
        }

        return best;
    }
}