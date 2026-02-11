using System.Collections.Generic;
using UnityEngine;

public class House : Building
{
    [Header("House Stats")]
    public int prestige = 5;
    public int comfort = 10;
    [Min(0)] public int storage = 30;
    [Min(1)] public int maxInhabitants = 4;

    [SerializeField]
    List<Civilian> civilians = new List<Civilian>();

    public IReadOnlyList<Civilian> Civilians => civilians;

    public bool CanAcceptResident(Civilian civilian)
    {
        if (civilian == null)
            return false;

        if (civilians.Contains(civilian))
            return true;

        return civilians.Count < Mathf.Max(1, maxInhabitants);
    }

    public bool TryAddResident(Civilian civilian)
    {
        if (!CanAcceptResident(civilian))
            return false;

        if (!civilians.Contains(civilian))
            civilians.Add(civilian);

        return true;
    }

    public void RemoveResident(Civilian civilian)
    {
        if (civilian == null)
            return;

        civilians.Remove(civilian);
    }

    void LateUpdate()
    {
        for (int i = civilians.Count - 1; i >= 0; i--)
        {
            if (civilians[i] == null)
                civilians.RemoveAt(i);
        }
    }

    protected override void Die()
    {
        civilians.Clear();
        base.Die();
    }
}
