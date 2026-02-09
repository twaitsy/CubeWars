using UnityEngine;
using UnityEngine.Serialization;

public class ResourceNode : MonoBehaviour
{
    bool isDestroyed;
    public ResourceType type;
    [FormerlySerializedAs("amount")]
    public int remaining = 500;

    [HideInInspector]
    public int claimedByTeam = -1;

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
    }

    void Update()
    {
        if (!isDestroyed && remaining <= 0)
            Deplete();
    }

    // Legacy compatibility: some code uses node.amount
    public int amount => remaining;

    public bool IsClaimedByOther(int teamID)
    {
        return claimedByTeam != -1 && claimedByTeam != teamID;
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