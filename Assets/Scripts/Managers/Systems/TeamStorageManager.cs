using System.Collections.Generic;
using UnityEngine;

public class TeamStorageManager : MonoBehaviour
{
    public static TeamStorageManager Instance;
    public int defaultCapacityPerType = 0;

    readonly Dictionary<int, List<ResourceStorageContainer>> storages = new Dictionary<int, List<ResourceStorageContainer>>();
    readonly Dictionary<int, Dictionary<string, int>> siteReserved = new Dictionary<int, Dictionary<string, int>>();
    readonly Dictionary<int, Dictionary<string, int>> reservedTotals = new Dictionary<int, Dictionary<string, int>>();
    readonly Dictionary<int, Dictionary<string, int>> baselineStored = new Dictionary<int, Dictionary<string, int>>();
    readonly Dictionary<int, Dictionary<string, int>> baselineCapacity = new Dictionary<int, Dictionary<string, int>>();

    string Key(ResourceDefinition resource) => ResourceIdUtility.GetKey(resource);

    void Awake() => Instance = this;

    void EnsureTeam(int teamID)
    {
        if (!storages.ContainsKey(teamID)) storages[teamID] = new List<ResourceStorageContainer>();
        if (!reservedTotals.ContainsKey(teamID)) reservedTotals[teamID] = new Dictionary<string, int>();
        if (!baselineStored.ContainsKey(teamID)) baselineStored[teamID] = new Dictionary<string, int>();
        if (!baselineCapacity.ContainsKey(teamID)) baselineCapacity[teamID] = new Dictionary<string, int>();
    }

    void EnsureResource(int teamID, ResourceDefinition resource)
    {
        EnsureTeam(teamID);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return;
        if (!reservedTotals[teamID].ContainsKey(key)) reservedTotals[teamID][key] = 0;
        if (!baselineStored[teamID].ContainsKey(key)) baselineStored[teamID][key] = 0;
        if (!baselineCapacity[teamID].ContainsKey(key)) baselineCapacity[teamID][key] = Mathf.Max(0, defaultCapacityPerType);
    }

    public void Register(ResourceStorageContainer c)
    {
        if (c == null) return;
        EnsureTeam(c.teamID);
        if (!storages[c.teamID].Contains(c)) storages[c.teamID].Add(c);
    }

    public void Unregister(ResourceStorageContainer c)
    {
        if (c == null) return;
        foreach (var kv in storages) kv.Value.Remove(c);
    }

    public int GetTotalStored(int teamID, ResourceDefinition resource)
    {
        EnsureResource(teamID, resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;
        int sum = baselineStored[teamID][key];
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++) if (list[i] != null) sum += list[i].GetStored(resource);
        return sum;
    }

    public int GetTotalCapacity(int teamID, ResourceDefinition resource)
    {
        EnsureResource(teamID, resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;
        int sum = baselineCapacity[teamID][key];
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++) if (list[i] != null) sum += list[i].GetCapacity(resource);
        return sum;
    }

    public int GetTotalFree(int teamID, ResourceDefinition resource)
    {
        EnsureResource(teamID, resource);
        return Mathf.Max(0, GetTotalCapacity(teamID, resource) - GetTotalStored(teamID, resource));
    }

    public int GetReservedTotal(int teamID, ResourceDefinition resource)
    {
        EnsureResource(teamID, resource);
        string key = Key(resource);
        return string.IsNullOrEmpty(key) ? 0 : reservedTotals[teamID][key];
    }

    public int GetReservedForSite(int siteKey, ResourceDefinition resource)
    {
        if (!siteReserved.TryGetValue(siteKey, out var dict)) return 0;
        string key = Key(resource);
        return string.IsNullOrEmpty(key) ? 0 : (dict.TryGetValue(key, out int v) ? v : 0);
    }

    public int GetAvailable(int teamID, ResourceDefinition resource)
    {
        return Mathf.Max(0, GetTotalStored(teamID, resource) - GetReservedTotal(teamID, resource));
    }

    public bool CanAffordAvailable(int teamID, ResourceCost[] costs)
    {
        if (costs == null) return true;
        for (int i = 0; i < costs.Length; i++)
            if (GetAvailable(teamID, costs[i].resource) < costs[i].amount) return false;
        return true;
    }

    public bool ReserveForSite(int teamID, int siteKey, ResourceCost[] costs)
    {
        if (!CanAffordAvailable(teamID, costs)) return false;
        EnsureTeam(teamID);
        if (!siteReserved.TryGetValue(siteKey, out var dict))
        {
            dict = new Dictionary<string, int>();
            siteReserved[siteKey] = dict;
        }

        for (int i = 0; i < costs.Length; i++)
        {
            string key = Key(costs[i].resource);
            if (string.IsNullOrEmpty(key)) continue;
            EnsureResource(teamID, costs[i].resource);
            dict[key] = dict.TryGetValue(key, out int d) ? d + costs[i].amount : costs[i].amount;
            reservedTotals[teamID][key] += costs[i].amount;
        }
        return true;
    }

    public void ReleaseReservation(int teamID, int siteKey)
    {
        EnsureTeam(teamID);
        if (!siteReserved.TryGetValue(siteKey, out var dict)) return;
        foreach (var kv in dict)
            if (reservedTotals[teamID].ContainsKey(kv.Key))
                reservedTotals[teamID][kv.Key] = Mathf.Max(0, reservedTotals[teamID][kv.Key] - kv.Value);
        siteReserved.Remove(siteKey);
    }

    public int ConsumeReserved(int teamID, int siteKey, ResourceDefinition resource, int amount)
    {
        EnsureResource(teamID, resource);
        if (amount <= 0) return 0;
        if (!siteReserved.TryGetValue(siteKey, out var dict)) return 0;
        string key = Key(resource);
        if (string.IsNullOrEmpty(key) || !dict.TryGetValue(key, out int current)) return 0;
        int can = Mathf.Min(amount, current);
        dict[key] = current - can;
        reservedTotals[teamID][key] = Mathf.Max(0, reservedTotals[teamID][key] - can);
        return can;
    }

    public int Withdraw(int teamID, ResourceDefinition resource, int amount)
    {
        EnsureResource(teamID, resource);
        if (amount <= 0) return 0;
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;

        int remaining = amount;
        int baselineTake = Mathf.Min(remaining, baselineStored[teamID][key]);
        baselineStored[teamID][key] -= baselineTake;
        remaining -= baselineTake;
        int takenTotal = baselineTake;

        var list = storages[teamID];
        for (int i = 0; i < list.Count && remaining > 0; i++)
        {
            var s = list[i];
            if (s == null) continue;
            int took = s.Withdraw(resource, remaining);
            takenTotal += took;
            remaining -= took;
        }
        return takenTotal;
    }

    public int Deposit(int teamID, ResourceDefinition resource, int amount)
    {
        EnsureResource(teamID, resource);
        if (amount <= 0) return 0;
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;

        int remaining = amount;
        int free = Mathf.Max(0, baselineCapacity[teamID][key] - baselineStored[teamID][key]);
        int baseAccepted = Mathf.Min(remaining, free);
        baselineStored[teamID][key] += baseAccepted;
        remaining -= baseAccepted;
        int acceptedTotal = baseAccepted;

        var list = storages[teamID];
        for (int i = 0; i < list.Count && remaining > 0; i++)
        {
            var s = list[i];
            if (s == null) continue;
            int accepted = s.Deposit(resource, remaining);
            acceptedTotal += accepted;
            remaining -= accepted;
        }

        return acceptedTotal;
    }

    public ResourceStorageContainer FindNearestStorageWithFree(int teamID, ResourceDefinition resource, Vector3 pos)
    {
        EnsureTeam(teamID);
        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null || !s.CanReceive(resource) || s.GetFree(resource) <= 0) continue;
            float d = (s.transform.position - pos).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }

    public ResourceStorageContainer FindNearestStorageWithStored(int teamID, ResourceDefinition resource, Vector3 pos)
    {
        EnsureTeam(teamID);
        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null || !s.CanSupply(resource) || s.GetStored(resource) <= 0) continue;
            float d = (s.transform.position - pos).sqrMagnitude;
            if (d < bestD) { bestD = d; best = s; }
        }
        return best;
    }


    public int GetTotalStoredInBuildings(int teamID, ResourceDefinition resource) => GetTotalStored(teamID, resource);
    public int GetTotalCapacityInBuildings(int teamID, ResourceDefinition resource) => GetTotalCapacity(teamID, resource);
    public int GetTotalFreeInBuildings(int teamID, ResourceDefinition resource) => GetTotalFree(teamID, resource);

    public void AddCapacity(int teamID, ResourceDefinition resource, int amount)
    {
        EnsureResource(teamID, resource);
        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
            if (list[i] != null)
                list[i].AddCapacity(resource, amount);
    }

    public void RemoveCapacity(int teamID, ResourceDefinition resource, int amount) => AddCapacity(teamID, resource, -amount);
public ResourceStorageContainer FindNearestStorageCached(int teamID, ResourceDefinition resource, Vector3 pos, bool requiresFree)
    {
        return requiresFree ? FindNearestStorageWithFree(teamID, resource, pos) : FindNearestStorageWithStored(teamID, resource, pos);
    }
}
