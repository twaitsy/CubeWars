using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
#if UNITY_EDITOR
    public void EditorAutoAssignFromPrefabName()
    {
        TryAutoAssignResourceFromAssetName();
    }
#endif

    [Header("Resource")]
    public ResourceDefinition resource;

    [FormerlySerializedAs("amount")]
    public int remaining = 500;

    public bool IsDepleted => remaining <= 0;

    [Header("Gathering")]
    [Min(1)] public int maxGatherers = 2;

    [HideInInspector] public int claimedByTeam = -1;

    readonly HashSet<int> gathererIds = new();
    public int ActiveGatherers => gathererIds.Count;
    public bool HasAvailableGatherSlots => gathererIds.Count < Mathf.Max(1, maxGatherers);

    bool isDepleted;

    void OnEnable()
    {
        ValidateConfiguration();
        ResourceRegistry.Instance?.Register(this);
    }

    void OnDisable()
    {
        ResourceRegistry.Instance?.Unregister(this);
        gathererIds.Clear();
    }

    void ValidateConfiguration()
    {
        if (resource == null)
            TryAutoAssignResourceFromAssetName();

        if (resource == null)
        {
            Debug.LogWarning($"[{nameof(ResourceNode)}] {name}: no ResourceDefinition assigned.", this);
            return;
        }

        if (!IsResourceInDatabases(resource))
        {
            Debug.LogWarning(
                $"[{nameof(ResourceNode)}] {name}: resource '{resource.id}' is not present in any ResourcesDatabase.",
                this
            );
        }
    }

    bool IsResourceInDatabases(ResourceDefinition def)
    {
        string key = ResourceIdUtility.GetKey(def);

        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
        if (loaded?.resources?.resources != null)
        {
            foreach (var r in loaded.resources.resources)
                if (ResourceIdUtility.GetKey(r) == key)
                    return true;
        }

        var global = ResourcesDatabase.Instance;
        if (global?.resources != null)
        {
            foreach (var r in global.resources)
                if (ResourceIdUtility.GetKey(r) == key)
                    return true;
        }

        return false;
    }

    void TryAutoAssignResourceFromAssetName()
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        string candidate = name;

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
        if (civilian != null)
            gathererIds.Remove(civilian.GetInstanceID());
    }

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
        ResourceRegistry.Instance?.NotifyNodeChanged(this);
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