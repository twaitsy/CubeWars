using System.Collections.Generic;
using UnityEngine;

public class ResourceDropoff : MonoBehaviour
{
    public int teamID;

    [Header("If empty, accepts ANY resource.")]
    public ResourceDefinition[] acceptsOnly;

    private static readonly List<ResourceDropoff> all = new();

    void OnEnable()
    {
        if (!all.Contains(this)) all.Add(this);
    }

    void OnDisable()
    {
        all.Remove(this);
    }

    public void SetTeamID(int newTeamID)
    {
        teamID = newTeamID;
    }

    public bool CanAccept(ResourceDefinition t)
    {
        if (acceptsOnly == null || acceptsOnly.Length == 0) return true;
        for (int i = 0; i < acceptsOnly.Length; i++)
            if (acceptsOnly[i] == t) return true;
        return false;
    }

    public int Deposit(ResourceDefinition type, int amount)
    {
        if (!CanAccept(type)) return 0;
        if (TeamResources.Instance == null) return 0;
        return TeamResources.Instance.Deposit(teamID, type, amount);
    }

    public static ResourceDropoff FindNearest(int teamID, ResourceDefinition type, Vector3 from)
    {
        ResourceDropoff best = null;
        float bestD = float.MaxValue;

        for (int i = 0; i < all.Count; i++)
        {
            var d = all[i];
            if (d == null) continue;
            if (d.teamID != teamID) continue;
            if (!d.CanAccept(type)) continue;

            float sq = (d.transform.position - from).sqrMagnitude;
            if (sq < bestD)
            {
                bestD = sq;
                best = d;
            }
        }

        return best;
    }
}
