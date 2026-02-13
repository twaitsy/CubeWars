using System.Collections.Generic;
using UnityEngine;

public class House : Building
{
    static readonly List<House> allHouses = new List<House>();

    static readonly string[] houseNamePrefixes =
    {
        "Amber", "Oak", "Stone", "River", "Iron", "Silver", "Golden", "Cedar", "Dawn", "Moon"
    };

    static readonly string[] houseNameSuffixes =
    {
        "Haven", "Cottage", "Manor", "Lodge", "Homestead", "Hall", "Retreat", "Dwelling", "Nest", "House"
    };

    static int nextHouseSerial = 1;

    [Header("House Stats")]
    public int prestige = 5;
    public int comfort = 10;
    [Min(0)] public int storage = 30;
    [Min(1)] public int maxInhabitants = 4;

    [SerializeField]
    List<Civilian> civilians = new List<Civilian>();

    public IReadOnlyList<Civilian> Civilians => civilians;
    public static IReadOnlyList<House> AllHouses => allHouses;

    void OnEnable()
    {
        if (!allHouses.Contains(this))
            allHouses.Add(this);

        EnsureRandomizedName();
        ConfigureHouseStorageReceiveOnly();
    }

    void OnDisable()
    {
        allHouses.Remove(this);
    }


    void ConfigureHouseStorageReceiveOnly()
    {
        ResourceStorageContainer[] storages = GetComponentsInChildren<ResourceStorageContainer>(true);
        for (int i = 0; i < storages.Length; i++)
        {
            var storage = storages[i];
            if (storage == null)
                continue;

            storage.teamID = teamID;

            for (int e = 0; e < storage.resourceFlow.Count; e++)
            {
                var entry = storage.resourceFlow[e];
                entry.flowMode = ResourceStorageContainer.ResourceFlowMode.ReceiveOnly;
                storage.resourceFlow[e] = entry;
            }
        }
    }

    ResourceStorageContainer GetHouseStorage()
    {
        return GetComponentInChildren<ResourceStorageContainer>();
    }

    public bool TryConsumeFood(ResourceType type, int amount, out int consumed)
    {
        consumed = 0;
        var storage = GetHouseStorage();
        if (storage == null || amount <= 0)
            return false;

        int available = storage.GetStored(type);
        if (available <= 0)
            return false;

        // House storage is receive-only for logistics; residents can still consume from it directly.
        consumed = Mathf.Min(amount, available);
        storage.SetStoredForRuntime(type, available - consumed);
        return consumed > 0;
    }
    void EnsureRandomizedName()
    {
        string cleanName = SanitizeName(name);
        if (!string.IsNullOrWhiteSpace(cleanName) && cleanName != "House")
            return;

        int prefixIndex = Random.Range(0, houseNamePrefixes.Length);
        int suffixIndex = Random.Range(0, houseNameSuffixes.Length);
        name = $"{houseNamePrefixes[prefixIndex]} {houseNameSuffixes[suffixIndex]} #{nextHouseSerial++:000}";
    }

    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return string.Empty;

        return raw.Replace("(Clone)", string.Empty).Trim();
    }

    public static House FindAvailableForTeam(int teamID, Civilian civilian, Vector3 fromPosition)
    {
        House best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < allHouses.Count; i++)
        {
            House house = allHouses[i];
            if (house == null || !house.IsAlive || house.teamID != teamID)
                continue;

            if (!house.CanAcceptResident(civilian))
                continue;

            float distance = (house.transform.position - fromPosition).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = house;
            }
        }

        return best;
    }

    public bool CanAcceptResident(Civilian civilian)
    {
        if (civilian == null)
            return false;

        if (civilians.Contains(civilian))
            return true;

        return civilians.Count < Mathf.Max(1, maxInhabitants);
    }

    public bool TryAddResident(Civilian civilian)
    {
        if (!CanAcceptResident(civilian))
            return false;

        if (!civilians.Contains(civilian))
            civilians.Add(civilian);

        return true;
    }

    public void RemoveResident(Civilian civilian)
    {
        if (civilian == null)
            return;

        civilians.Remove(civilian);
    }

    void LateUpdate()
    {
        for (int i = civilians.Count - 1; i >= 0; i--)
        {
            if (civilians[i] == null)
                civilians.RemoveAt(i);
        }
    }

    protected override void Die()
    {
        civilians.Clear();
        base.Die();
    }
}
