using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    [Header("Resource")]
    public ResourceDefinition resource;

    [FormerlySerializedAs("amount")]
    public int remaining = 500;

    [Header("Gathering")]
    [Min(1)] public int maxGatherers = 2;

    [HideInInspector] public int claimedByTeam = -1;

    private readonly HashSet<int> gathererIds = new HashSet<int>();
    bool isDestroyed;

    public bool IsDepleted => remaining <= 0;
    public int amount => remaining;
    public int ActiveGatherers => gathererIds.Count;
    public bool HasAvailableGatherSlots => gathererIds.Count < Mathf.Max(1, maxGatherers);

    void OnEnable()
    {
        ValidateConfiguration();

        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Register(this);
    }


    void ValidateConfiguration()
    {
        if (resource == null)
        {
            Debug.LogWarning($"[{nameof(ResourceNode)}] {name}: no ResourceDefinition assigned.", this);
            return;
        }

        string key = ResourceIdUtility.GetKey(resource);
        bool inLoadedDatabase = false;
        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
        if (loaded != null && loaded.resources != null && loaded.resources.resources != null)
        {
            for (int i = 0; i < loaded.resources.resources.Count; i++)
            {
                if (ResourceIdUtility.GetKey(loaded.resources.resources[i]) == key)
                {
                    inLoadedDatabase = true;
                    break;
                }
            }
        }

        bool inGlobalDatabase = false;
        if (ResourcesDatabase.Instance != null && ResourcesDatabase.Instance.resources != null)
        {
            for (int i = 0; i < ResourcesDatabase.Instance.resources.Count; i++)
            {
                if (ResourceIdUtility.GetKey(ResourcesDatabase.Instance.resources[i]) == key)
                {
                    inGlobalDatabase = true;
                    break;
                }
            }
        }

        if (!inLoadedDatabase && !inGlobalDatabase)
            Debug.LogWarning($"[{nameof(ResourceNode)}] {name}: resource '{resource.id}' is not present in loaded ResourcesDatabase.", this);
    }

    void OnDisable()
    {
        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Unregister(this);

        gathererIds.Clear();
    }

    void Update()
    {
        if (!isDestroyed && remaining <= 0)
            Deplete();
    }

    public bool IsClaimedByOther(int teamID)
    {
        return claimedByTeam != -1 && claimedByTeam != teamID;
    }

    public bool TryReserveGatherSlot(Civilian civilian)
    {
        if (civilian == null || IsDepleted)
            return false;

        int id = civilian.GetInstanceID();
        if (gathererIds.Contains(id))
            return true;

        if (gathererIds.Count >= Mathf.Max(1, maxGatherers))
            return false;

        gathererIds.Add(id);
        return true;
    }

    public void ReleaseGatherSlot(Civilian civilian)
    {
        if (civilian == null)
            return;

        gathererIds.Remove(civilian.GetInstanceID());
    }

    public int Harvest(int amount)
    {
        if (amount <= 0 || IsDepleted) return 0;

        int taken = Mathf.Min(remaining, amount);
        remaining -= taken;

        if (remaining <= 0)
            Deplete();

        return taken;
    }

    void Deplete()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Unregister(this);

        Destroy(gameObject);
    }
}