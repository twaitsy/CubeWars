using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType type;
    public int remaining = 500;

    [HideInInspector]
    public int claimedByTeam = -1;

    public bool IsDepleted => remaining <= 0;

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
        return taken;
    }
}