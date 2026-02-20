using UnityEngine;

public static class TeamAssignmentUtility
{
    public static void ApplyTeamToHierarchy(GameObject root, int teamID)
    {
        if (root == null) return;

        ApplyToAll(root, teamID, (Building c, int t) => c.teamID = t);
        ApplyToAll(root, teamID, (Headquarters c, int t) => c.teamID = t);
        ApplyToAll(root, teamID, (Barracks c, int t) => c.teamID = t);
        ApplyToAll(root, teamID, (Civilian c, int t) => c.SetTeamID(t));
        ApplyToAll(root, teamID, (Unit c, int t) => c.SetTeamID(t));
        ApplyToAll(root, teamID, (ResourceDropoff c, int t) => c.SetTeamID(t));
        ApplyToAll(root, teamID, (ResourceStorageProvider c, int t) => c.SetTeamID(t));
        ApplyToAll(root, teamID, (ResourceStorageContainer c, int t) => c.SetTeamID(t));
        ApplyToAll(root, teamID, (Attackable c, int t) =>
        {
            c.teamID = t;
            c.isCivilian = c.GetComponent<Civilian>() != null;
            c.isBuilding = c.GetComponent<Building>() != null;
        });
        ApplyToAll(root, teamID, (TeamVisual tv, int t) =>
        {
            tv.teamID = t;
            tv.kind = (tv.GetComponent<Building>() != null) ? VisualKind.Building : VisualKind.Unit;
            tv.Apply();
        });

        ApplyCombatDefaults(root, teamID);
        EnsureDisplayName(root);
    }

    static void EnsureDisplayName(GameObject root)
    {
        bool isUnitLike = root.GetComponentInChildren<Civilian>(true) != null || root.GetComponentInChildren<Unit>(true) != null;
        if (!isUnitLike) return;

        if (string.IsNullOrWhiteSpace(root.name) || root.name.Contains("(Clone)"))
            root.name = UnitNamePool.GetRandomDisplayName();
    }


    static void ApplyCombatDefaults(GameObject root, int teamID)
    {
        var combats = root.GetComponentsInChildren<UnitCombatController>(true);
        for (int i = 0; i < combats.Length; i++)
        {
            var combat = combats[i];
            if (combat == null) continue;
            combat.teamID = teamID;

            if (CombatManager.Instance != null)
                CombatManager.Instance.ConfigureCombatant(combat);
        }
    }

    static void ApplyToAll<T>(GameObject root, int teamID, System.Action<T, int> apply) where T : Component
    {
        var comps = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            if (comps[i] == null) continue;
            apply(comps[i], teamID);
        }
    }
}
