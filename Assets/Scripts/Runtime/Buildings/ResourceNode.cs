using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
#if UNITY_EDITOR
    /// <summary>
    /// Public wrapper so the Editor can safely auto-assign using the same logic as runtime.
    /// </summary>
    public void EditorAutoAssignFromPrefabName()
    {
        TryAutoAssignResourceFromAssetName();
    }
#endif

    // --- Resource -----------------------------------------------------------

    [Header("Resource")]
    public ResourceDefinition resource;

    [FormerlySerializedAs("amount")]
    public int remaining = 500;

    // 🔹 Compatibility + clarity
    public int amount => remaining;     // legacy API (DO NOT REMOVE)
    public int Amount => remaining;     // preferred API

    public bool IsDepleted => remaining <= 0;

    // --- Gathering ----------------------------------------------------------

    [Header("Gathering")]
    [Min(1)] public int maxGatherers = 2;

    [HideInInspector] public int claimedByTeam = -1;

    readonly HashSet<int> gathererIds = new HashSet<int>();
    public int ActiveGatherers => gathererIds.Count;
    public bool HasAvailableGatherSlots => gathererIds.Count < Mathf.Max(1, maxGatherers);

    bool isDepleted;

    // --- Unity Lifecycle ----------------------------------------------------

    void OnEnable()
    {
        ValidateConfiguration();

        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Register(this);
    }

    void OnDisable()
    {
        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Unregister(this);

        gathererIds.Clear();
    }

    // --- Validation ---------------------------------------------------------

    void ValidateConfiguration()
    {
        if (resource == null)
            TryAutoAssignResourceFromAssetName();

        if (resource == null)
        {
            Debug.LogWarning(
                $"[{nameof(ResourceNode)}] {name}: no ResourceDefinition assigned.",
                this
            );
            return;
        }

        string key = ResourceIdUtility.GetKey(resource);
        bool found = false;

        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
        if (loaded?.resources?.resources != null)
        {
            for (int i = 0; i < loaded.resources.resources.Count; i++)
            {
                if (ResourceIdUtility.GetKey(loaded.resources.resources[i]) == key)
                {
                    found = true;
                    break;
                }
            }
        }

        if (!found && ResourcesDatabase.Instance?.resources != null)
        {
            for (int i = 0; i < ResourcesDatabase.Instance.resources.Count; i++)
            {
                if (ResourceIdUtility.GetKey(ResourcesDatabase.Instance.resources[i]) == key)
                {
                    found = true;
                    break;
                }
            }
        }

        if (!found)
        {
            Debug.LogWarning(
                $"[{nameof(ResourceNode)}] {name}: resource '{resource.id}' is not present in any ResourcesDatabase.",
                this
            );
        }
    }

    void TryAutoAssignResourceFromAssetName()
    {
        string candidate = name;
        if (string.IsNullOrWhiteSpace(candidate))
            return;

        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
        if (loaded?.resources != null &&
            loaded.resources.TryGetById(candidate, out ResourceDefinition loadedResource))
        {
            resource = loadedResource;
            return;
        }

        if (ResourcesDatabase.Instance != null &&
            ResourcesDatabase.Instance.TryGetById(candidate, out ResourceDefinition globalResource))
        {
            resource = globalResource;
        }
    }

    // --- Claiming & Gathering ----------------------------------------------

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

        if (!HasAvailableGatherSlots)
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

    // --- Harvesting ---------------------------------------------------------

    public int Harvest(int amount)
    {
        if (amount <= 0 || IsDepleted)
            return 0;

        int taken = Mathf.Min(remaining, amount);
        remaining -= taken;

        NotifyChanged();

        if (remaining <= 0)
            Deplete();

        return taken;
    }

    void NotifyChanged()
    {
        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.NotifyNodeChanged(this);
    }

    void Deplete()
    {
        if (isDepleted)
            return;

        isDepleted = true;
        gathererIds.Clear();

        NotifyChanged();

        Destroy(gameObject);
    }
}
