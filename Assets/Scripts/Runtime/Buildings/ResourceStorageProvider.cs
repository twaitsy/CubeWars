using System.Collections.Generic;
using UnityEngine;

public class ResourceStorageProvider : MonoBehaviour
{
    public int teamID;

    [Header("Capacities provided by this building")]
    public ResourceCapacityEntry[] capacities;

    [Header("Optional starting resources")]
    [Tooltip("If enabled, these resources are granted once when this provider first registers.")]
    public bool grantStartingResources = false;
    public ResourceCost[] startingResources;

    private bool started;
    private bool registered;
    private bool grantedStartingResources;
    private ResourceStorageContainer localStorage;

    void Start()
    {
        started = true;
        Register();
    }

    void OnEnable()
    {
        if (started) Register();
    }

    void OnDisable()
    {
        if (registered) Unregister();
    }

    void Register()
    {
        if (registered) return;

        ResolveLocalStorage();
        if (localStorage == null)
            return;

        Dictionary<ResourceDefinition, int> capacitiesToApply = BuildCapacityMap();
        foreach (var pair in capacitiesToApply)
        {
            int cap = Mathf.Max(0, pair.Value);
            if (cap > 0)
                localStorage.AddCapacity(pair.Key, cap);
        }

        if (grantStartingResources && !grantedStartingResources && TeamResources.Instance != null && startingResources != null)
        {
            for (int i = 0; i < startingResources.Length; i++)
            {
                int amount = Mathf.Max(0, startingResources[i].amount);
                if (amount <= 0) continue;

                TeamResources.Instance.Deposit(teamID, startingResources[i].resource, amount);
            }

            grantedStartingResources = true;
        }

        registered = true;
    }

    void Unregister()
    {
        ResolveLocalStorage();
        if (localStorage == null) return;

        Dictionary<ResourceDefinition, int> capacitiesToApply = BuildCapacityMap();
        foreach (var pair in capacitiesToApply)
        {
            int cap = Mathf.Max(0, pair.Value);
            if (cap > 0)
                localStorage.AddCapacity(pair.Key, -cap);
        }

        registered = false;
    }

    Dictionary<ResourceDefinition, int> BuildCapacityMap()
    {
        var result = new Dictionary<ResourceDefinition, int>();

        if (capacities != null)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                ResourceCapacityEntry entry = capacities[i];
                if (entry == null || entry.resource == null)
                    continue;

                int cap = Mathf.Max(0, entry.capacity);
                if (!result.ContainsKey(entry.resource))
                    result[entry.resource] = cap;
                else
                    result[entry.resource] = Mathf.Max(result[entry.resource], cap);
            }
        }

        BuildingDefinition def = ResolveBuildingDefinition();
        if (def != null && def.isStorage)
        {
            int minCapacity = Mathf.Max(1000, def.storageSettings != null ? def.storageSettings.capacity : 0);
            var db = ResourcesDatabase.Instance;
            if (db != null && db.resources != null)
            {
                for (int i = 0; i < db.resources.Count; i++)
                {
                    var resource = db.resources[i];
                    if (resource == null)
                        continue;

                    if (!result.ContainsKey(resource))
                        result[resource] = minCapacity;
                    else
                        result[resource] = Mathf.Max(result[resource], minCapacity);
                }
            }
        }

        return result;
    }

    BuildingDefinition ResolveBuildingDefinition()
    {
        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null)
            return null;

        string id = null;

        Building building = GetComponentInParent<Building>();
        if (building != null)
            id = building.buildingDefinitionId;

        if (string.IsNullOrWhiteSpace(id))
        {
            BuildItemInstance item = GetComponentInParent<BuildItemInstance>();
            if (item != null)
                id = item.itemId;
        }

        if (string.IsNullOrWhiteSpace(id))
            id = gameObject.name.Replace("(Clone)", string.Empty).Trim();

        return loaded.TryGetBuildingById(id, out var def) ? def : null;
    }

    public void SetTeamID(int newTeamID)
    {
        if (teamID == newTeamID) return;
        teamID = newTeamID;
        RefreshRegistration();
    }

    public void RefreshRegistration()
    {
        if (!started) return;

        if (registered)
            Unregister();

        Register();
    }

    void ResolveLocalStorage()
    {
        if (localStorage != null)
            return;

        localStorage = GetComponent<ResourceStorageContainer>();
        if (localStorage == null)
            localStorage = GetComponentInChildren<ResourceStorageContainer>();
    }
}
