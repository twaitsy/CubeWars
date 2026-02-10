using UnityEngine;
using UnityEngine.Serialization;

public class ResourceNode : MonoBehaviour
{
    bool isDestroyed;
    public ResourceType type;
    [FormerlySerializedAs("amount")]
    public int remaining = 500;

    [Header("Gathering")]
    [Tooltip("Maximum number of civilians that can gather from this node at once.")]
    [Min(1)] public int maxGatherers = 2;

    [HideInInspector]
    public int claimedByTeam = -1;

    private readonly System.Collections.Generic.HashSet<int> gathererIds =
        new System.Collections.Generic.HashSet<int>();

    public bool IsDepleted => remaining <= 0;

    void OnEnable()
    {
        if (ResourceRegistry.Instance != null)
            ResourceRegistry.Instance.Register(this);
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

    // Legacy compatibility: some code uses node.amount
    public int amount => remaining;
    public int ActiveGatherers => gathererIds.Count;
    public bool HasAvailableGatherSlots => gathererIds.Count < Mathf.Max(1, maxGatherers);

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

    /// <summary>
    /// Harvests up to 'amount' from this node. Returns how much was actually taken.
    /// </summary>
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
