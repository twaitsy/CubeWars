// =============================================================
// ResourceStorageContainer.cs
//
// PURPOSE:
// - Represents a single storage container for a building or unit.
// - Tracks stored amounts and capacity for each ResourceType.
// - Registers itself with TeamStorageManager for global queries.
//
// DEPENDENCIES:
// - ResourceType:
//      * Enum defining all resource categories.
// - TeamStorageManager:
//      * Registers/unregisters this container.
//      * Aggregates totals, capacity, reservations, and withdrawals.
// - Building / Unit / Civilian:
//      * teamID determines which storage bucket this belongs to.
// - ConstructionSite:
//      * May use this container for temporary storage during building.
//
// NOTES FOR FUTURE MAINTENANCE:
// - This script NEVER deletes teams or GameObjects.
// - This script NEVER enforces singleton behavior.
// - This script NEVER modifies teamID automatically.
// - Capacity and stored values are per-container, not global.
// - If you add new ResourceTypes, they will auto-initialize in Awake().
// - If you add building upgrades, call AddCapacity() accordingly.
//
// ARCHITECTURE:
// - Attached to buildings that provide storage.
// - Registered with TeamStorageManager on enable.
// - Unregistered on disable.
// - Safe to have multiple containers per team.
// =============================================================

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
        public ResourceType type;

        [Tooltip("Disabled = no hauling, ReceiveOnly = drop-off only, SupplyOnly = pickup only, ReceiveAndSupply = both pickup and drop-off.")]
        public ResourceFlowMode flowMode;
    }

    public int teamID;

    [Header("Per-Resource Hauler Permissions")]
    [Tooltip("Controls whether haulers can deposit to and/or withdraw from this storage for each resource type.")]
    public List<ResourceFlowEntry> resourceFlow = new List<ResourceFlowEntry>();

    Dictionary<ResourceType, int> stored = new Dictionary<ResourceType, int>();
    Dictionary<ResourceType, int> capacity = new Dictionary<ResourceType, int>();

    void Awake()
    {
        EnsureFlowEntries();

        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            stored[t] = 0;
            capacity[t] = 0;
        }
    }

    void OnValidate()
    {
        EnsureFlowEntries();
    }

    void EnsureFlowEntries()
    {
        if (resourceFlow == null)
            resourceFlow = new List<ResourceFlowEntry>();

        foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
        {
            bool exists = false;
            for (int i = 0; i < resourceFlow.Count; i++)
            {
                if (resourceFlow[i].type != t) continue;
                exists = true;
                break;
            }

            if (exists) continue;
            resourceFlow.Add(new ResourceFlowEntry
            {
                type = t,
                flowMode = ResourceFlowMode.ReceiveAndSupply
            });
        }
    }

    ResourceFlowMode GetFlowMode(ResourceType type)
    {
        for (int i = 0; i < resourceFlow.Count; i++)
        {
            if (resourceFlow[i].type == type)
                return resourceFlow[i].flowMode;
        }

        return ResourceFlowMode.ReceiveAndSupply;
    }

    public ResourceFlowMode GetFlowSetting(ResourceType type)
    {
        return GetFlowMode(type);
    }

    public bool CanReceive(ResourceType type)
    {
        ResourceFlowMode mode = GetFlowMode(type);
        return mode == ResourceFlowMode.ReceiveOnly || mode == ResourceFlowMode.ReceiveAndSupply;
    }

    public bool CanSupply(ResourceType type)
    {
        ResourceFlowMode mode = GetFlowMode(type);
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

    public int GetStored(ResourceType type)
    {
        return stored[type];
    }

    public int GetCapacity(ResourceType type)
    {
        return capacity[type];
    }

    public int GetFree(ResourceType type)
    {
        return Mathf.Max(0, capacity[type] - stored[type]);
    }

    public int Deposit(ResourceType type, int amount)
    {
        if (!CanReceive(type)) return 0;

        int free = GetFree(type);
        int accepted = Mathf.Min(free, amount);
        stored[type] += accepted;
        return accepted;
    }

    public int Withdraw(ResourceType type, int amount)
    {
        if (!CanSupply(type)) return 0;

        int taken = Mathf.Min(stored[type], amount);
        stored[type] -= taken;
        return taken;
    }

    public void AddCapacity(ResourceType type, int amount)
    {
        capacity[type] += amount;
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
