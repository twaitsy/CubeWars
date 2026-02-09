using UnityEngine;

public static class TeamOwnershipUtility
{
    public static void ApplyTeamToHierarchy(GameObject root, int teamID)
    {
        if (root == null) return;

        ApplyToAllInChildren<Building>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<Headquarters>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<ResourceDropoff>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<ResourceStorageProvider>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<ResourceStorageContainer>(root, teamID, (c, t) => c.teamID = t);

        ApplyToAllInChildren<Civilian>(root, teamID, (c, t) =>
        {
            c.teamID = t;
            c.RefreshJobManagerRegistration();
        });

        ApplyToAllInChildren<Unit>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<Attackable>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<UnitCombatController>(root, teamID, (c, t) => c.teamID = t);
        ApplyToAllInChildren<Turret>(root, teamID, (c, t) => c.teamID = t);

        var buildingOnRoot = root.GetComponent<Building>() != null;
        ApplyToAllInChildren<TeamVisual>(root, teamID, (tv, t) =>
        {
            tv.teamID = t;
            tv.kind = (tv.GetComponent<Building>() != null || buildingOnRoot) ? VisualKind.Building : VisualKind.Unit;
            tv.Apply();
        });

        if (TeamColorManager.Instance != null)
            TeamColorManager.Instance.ApplyTeamColor(root, teamID);
    }

    static void ApplyToAllInChildren<T>(GameObject root, int teamID, System.Action<T, int> apply)
        where T : Component
    {
        var comps = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            var c = comps[i];
            if (c == null) continue;
            apply(c, teamID);
        }
    }
}
