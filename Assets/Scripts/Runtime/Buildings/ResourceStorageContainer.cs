using UnityEngine;
using System.Collections.Generic;

public class ResourceStorageContainer : MonoBehaviour
{
    public enum ResourceFlowMode
    {
        Disabled,
        ReceiveOnly,
        SupplyOnly,
        ReceiveAndSupply
    }

    [System.Serializable]
    public struct ResourceFlowEntry
    {
        public ResourceDefinition resource;
        public ResourceFlowMode flowMode;
    }

    public int teamID;
    public List<ResourceFlowEntry> resourceFlow = new List<ResourceFlowEntry>();

    readonly Dictionary<string, int> stored = new Dictionary<string, int>();
    readonly Dictionary<string, int> capacity = new Dictionary<string, int>();

    string Key(ResourceDefinition resource) => ResourceIdUtility.GetKey(resource);

    ResourceFlowMode GetFlowMode(ResourceDefinition resource)
    {
        for (int i = 0; i < resourceFlow.Count; i++)
        {
            if (ResourceIdUtility.GetKey(resourceFlow[i].resource) == Key(resource))
                return resourceFlow[i].flowMode;
        }

        return ResourceFlowMode.ReceiveAndSupply;
    }

    void EnsureResource(ResourceDefinition resource)
    {
        string key = Key(resource);
        if (string.IsNullOrEmpty(key))
            return;

        if (!stored.ContainsKey(key)) stored[key] = 0;
        if (!capacity.ContainsKey(key)) capacity[key] = 0;
    }

    public ResourceFlowMode GetFlowSetting(ResourceDefinition resource) => GetFlowMode(resource);

    public bool CanReceive(ResourceDefinition resource)
    {
        ResourceFlowMode mode = GetFlowMode(resource);
        return mode == ResourceFlowMode.ReceiveOnly || mode == ResourceFlowMode.ReceiveAndSupply;
    }

    public bool CanSupply(ResourceDefinition resource)
    {
        ResourceFlowMode mode = GetFlowMode(resource);
        return mode == ResourceFlowMode.SupplyOnly || mode == ResourceFlowMode.ReceiveAndSupply;
    }

    void OnEnable()
    {
        if (TeamStorageManager.Instance != null)
            TeamStorageManager.Instance.Register(this);
    }

    void OnDisable()
    {
        if (TeamStorageManager.Instance != null)
            TeamStorageManager.Instance.Unregister(this);
    }

    public int GetStored(ResourceDefinition resource)
    {
        EnsureResource(resource);
        string key = Key(resource);
        return string.IsNullOrEmpty(key) ? 0 : stored[key];
    }

    public int GetCapacity(ResourceDefinition resource)
    {
        EnsureResource(resource);
        string key = Key(resource);
        return string.IsNullOrEmpty(key) ? 0 : capacity[key];
    }

    public int GetFree(ResourceDefinition resource)
    {
        EnsureResource(resource);
        string key = Key(resource);
        return string.IsNullOrEmpty(key) ? 0 : Mathf.Max(0, capacity[key] - stored[key]);
    }

    public int Deposit(ResourceDefinition resource, int amount)
    {
        if (!CanReceive(resource)) return 0;
        EnsureResource(resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;

        int accepted = Mathf.Min(GetFree(resource), amount);
        stored[key] += accepted;
        return accepted;
    }

    public int Withdraw(ResourceDefinition resource, int amount)
    {
        if (!CanSupply(resource)) return 0;
        EnsureResource(resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return 0;

        int taken = Mathf.Min(stored[key], amount);
        stored[key] -= taken;
        return taken;
    }

    public void SetStoredForRuntime(ResourceDefinition resource, int amount)
    {
        EnsureResource(resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return;
        stored[key] = Mathf.Clamp(amount, 0, capacity[key]);
    }

    public void AddCapacity(ResourceDefinition resource, int amount)
    {
        EnsureResource(resource);
        string key = Key(resource);
        if (string.IsNullOrEmpty(key)) return;
        capacity[key] += amount;
    }

    public void SetTeamID(int newTeamID)
    {
        if (teamID == newTeamID) return;
        if (TeamStorageManager.Instance != null)
            TeamStorageManager.Instance.Unregister(this);
        teamID = newTeamID;
        if (isActiveAndEnabled && TeamStorageManager.Instance != null)
            TeamStorageManager.Instance.Register(this);
    }
}
