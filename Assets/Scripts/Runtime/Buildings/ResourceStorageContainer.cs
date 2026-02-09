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
    public int teamID;

    Dictionary<ResourceType, int> stored = new Dictionary<ResourceType, int>();
    Dictionary<ResourceType, int> capacity = new Dictionary<ResourceType, int>();

    void Awake()
    {
        foreach (ResourceType t in ResourceDefaults.AllTypes)
        {
            stored[t] = 0;
            capacity[t] = ResourceDefaults.DefaultStoragePerResource;
        }
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
        int free = GetFree(type);
        int accepted = Mathf.Min(free, amount);
        stored[type] += accepted;
        return accepted;
    }

    public int Withdraw(ResourceType type, int amount)
    {
        int taken = Mathf.Min(stored[type], amount);
        stored[type] -= taken;
        return taken;
    }

    public void AddCapacity(ResourceType type, int amount)
    {
        capacity[type] += amount;
    }
}