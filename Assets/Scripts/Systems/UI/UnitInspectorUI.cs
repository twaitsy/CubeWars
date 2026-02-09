// =============================================================
// UnitInspectorUI.cs
//
// PURPOSE:
// - On-screen inspector for the currently selected object.
// - Shows health, team, civilian/unit info, barracks queue, combat stance,
//   construction progress, storage contents, and turret info.
//
// DEPENDENCIES:
// - SelectionManager (or equivalent):
//      * Must call SetSelected(GameObject) when selection changes.
// - Barracks:
//      * Expected API:
//          - List<UnitProductionDefinition> producibleUnits
//          - bool CanQueue(UnitProductionDefinition def)
//          - void QueueUnit(UnitProductionDefinition def)
//          - float CurrentBuildTime
//          - float CurrentProgress
//          - void CancelLast()
// - UnitCombatController:
//      * Used for target display, stance, and "Attack Civilians" toggle.
//      * Must implement:
//          - string GetTargetStatus()
//          - bool canAttackCivilians
//          - void ToggleAttackCivilians()
//          - CombatStance stance
//          - void SetStance(CombatStance newStance)
// - Turret:
//      * Marker component for turret-specific UI.
// - Unit:
//      * Exposes teamID, combatEnabled, damage, attackRange.
// - Civilian:
//      * Exposes teamID.
// - ConstructionSite:
//      * Expected API:
//          - int teamID
//          - string GetStatusLine()
//          - ResourceCost[] GetRequiredCosts()
//          - int GetDeliveredAmount(ResourceType type)
// - ResourceStorageContainer:
//      * Expected API:
//          - int teamID
//          - int GetCapacity(ResourceType type)
//          - int GetStored(ResourceType type)
// - IHasHealth:
//      * Interface with:
//          - float CurrentHealth { get; }
//          - float MaxHealth { get; }
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you change any of the dependent APIs (Barracks, ConstructionSite, etc.),
//   update this UI to match.
// - Keep this script UI-only: do not put game logic here.
// - If you add new unit types or systems, add new DrawX() sections rather than
//   overloading existing ones.
// =============================================================

using UnityEngine;

public class UnitInspectorUI : MonoBehaviour
{
    [Header("Layout (OnGUI)")]
    public float width = 320f;
    public float height = 420f;
    public float padding = 10f;

    private GameObject selected;

    // Called by SelectionManager
    public void SetSelected(GameObject obj)
    {
        selected = obj;

        if (selected != null)
            Debug.Log("UnitInspectorUI: Selected " + selected.name);
        else
            Debug.Log("UnitInspectorUI: Selection cleared");
    }

    void OnGUI()
    {
        if (selected == null)
            return;

        Rect rect = new Rect(
            Screen.width - width - padding,
            Screen.height - height - padding,
            width,
            height
        );

        GUILayout.BeginArea(rect, GUI.skin.window);
        GUILayout.BeginVertical();

        DrawHeader();
        DrawHealth();
        DrawCivilian();
        DrawUnit();
        DrawBarracks();
        DrawCombat();
        DrawConstruction();
        DrawStorage();
        DrawResourceNode();
        DrawTurret();
        DrawCombatStance();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void DrawHeader()
    {
        GUILayout.Label(selected.name, GUI.skin.box);

        int team = GetTeamID();
        if (team >= 0)
            GUILayout.Label($"Team: {team}");
    }

    void DrawHealth()
    {
        if (selected.TryGetComponent<IHasHealth>(out var health))
        {
            GUILayout.Label($"Health: {health.CurrentHealth:0}/{health.MaxHealth:0}");
        }
    }

    void DrawCivilian()
    {
        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Space(6);
            GUILayout.Label("Civilian", GUI.skin.box);
            GUILayout.Label($"Role: {civ.role}");
            GUILayout.Label($"Carrying: {civ.CarriedType} {civ.CarriedAmount}/{civ.carryCapacity}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gatherer")) civ.SetRole(CivilianRole.Gatherer);
            if (GUILayout.Button("Builder")) civ.SetRole(CivilianRole.Builder);
            if (GUILayout.Button("Hauler")) civ.SetRole(CivilianRole.Hauler);
            if (GUILayout.Button("Idle")) civ.SetRole(CivilianRole.Idle);
            GUILayout.EndHorizontal();
        }
    }

    void DrawUnit()
    {
        if (selected.TryGetComponent<Unit>(out var unit))
        {
            GUILayout.Space(6);
            GUILayout.Label("Unit", GUI.skin.box);
            GUILayout.Label($"Combat Enabled: {(unit.combatEnabled ? "Yes" : "No")}");
            GUILayout.Label($"Damage: {unit.damage}");
            GUILayout.Label($"Range: {unit.attackRange}");
        }
    }

    void DrawBarracks()
    {
        if (!selected.TryGetComponent<Barracks>(out var barracks))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Barracks", GUI.skin.box);

        if (barracks.producibleUnits != null)
        {
            foreach (var def in barracks.producibleUnits)
            {
                if (def == null) continue;

                GUILayout.BeginHorizontal(GUI.skin.box);

                GUILayout.Label(def.unitName, GUILayout.Width(120));

                bool canAfford = barracks.CanQueue(def);
                GUI.enabled = canAfford;

                if (GUILayout.Button("Train"))
                {
                    barracks.QueueUnit(def);
                }

                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (def.costs != null)
                {
                    foreach (var c in def.costs)
                    {
                        GUILayout.Label($"  {c.type}: {c.amount}");
                    }
                }
            }
        }

        GUILayout.Space(4);
        GUILayout.Label("Queue", GUI.skin.box);

        if (barracks.CurrentBuildTime > 0f)
        {
            float pct = barracks.CurrentProgress / barracks.CurrentBuildTime;
            GUILayout.HorizontalSlider(pct, 0f, 1f);
        }

        if (GUILayout.Button("Cancel Last"))
        {
            barracks.CancelLast();
        }
    }

    void DrawCombat()
    {
        if (!selected.TryGetComponent<UnitCombatController>(out var combat))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Combat", GUI.skin.box);

        GUILayout.Label("Target: " + combat.GetTargetStatus());

        bool newToggle = GUILayout.Toggle(
            combat.canAttackCivilians,
            "Attack Civilians"
        );

        if (newToggle != combat.canAttackCivilians)
        {
            combat.ToggleAttackCivilians();
        }
    }


    void DrawResourceNode()
    {
        if (selected.TryGetComponent<ResourceNode>(out var node))
        {
            GUILayout.Space(6);
            GUILayout.Label("Resource Node", GUI.skin.box);
            GUILayout.Label($"Type: {node.type}");
            GUILayout.Label($"Remaining: {node.remaining}");
        }
    }

    void DrawTurret()
    {
        if (!selected.TryGetComponent<Turret>(out var turret))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Turret", GUI.skin.box);

        if (selected.TryGetComponent<UnitCombatController>(out var combat))
        {
            GUILayout.Label("Target: " + combat.GetTargetStatus());

            bool newToggle = GUILayout.Toggle(
                combat.canAttackCivilians,
                "Attack Civilians"
            );

            if (newToggle != combat.canAttackCivilians)
                combat.ToggleAttackCivilians();
        }
    }

    void DrawCombatStance()
    {
        if (!selected.TryGetComponent<UnitCombatController>(out var combat))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Combat Stance", GUI.skin.box);

        var newStance = (UnitCombatController.CombatStance)
            GUILayout.SelectionGrid(
                (int)combat.stance,
                new[] { "Hold", "Guard", "Aggressive" },
                3
            );

        if (newStance != combat.stance)
            combat.SetStance(newStance);
    }

    void DrawConstruction()
    {
        if (selected.TryGetComponent<ConstructionSite>(out var site))
        {
            GUILayout.Space(6);
            GUILayout.Label("Construction", GUI.skin.box);
            GUILayout.Label(site.GetStatusLine());

            var costs = site.GetRequiredCosts();
            if (costs != null)
            {
                foreach (var c in costs)
                {
                    int delivered = site.GetDeliveredAmount(c.type);
                    GUILayout.Label($"{c.type}: {delivered}/{c.amount}");
                }
            }
        }
    }

    void DrawStorage()
    {
        if (selected.TryGetComponent<ResourceStorageContainer>(out var storage))
        {
            GUILayout.Space(6);
            GUILayout.Label("Storage", GUI.skin.box);
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
            {
                int cap = storage.GetCapacity(t);
                if (cap <= 0) continue;
                int stored = storage.GetStored(t);
                GUILayout.Label($"{t}: {stored}/{cap}");
            }
        }
    }

    int GetTeamID()
    {
        if (selected.TryGetComponent<Unit>(out var unit)) return unit.teamID;
        if (selected.TryGetComponent<Civilian>(out var civ)) return civ.teamID;
        if (selected.TryGetComponent<ConstructionSite>(out var site)) return site.teamID;
        if (selected.TryGetComponent<ResourceStorageContainer>(out var storage)) return storage.teamID;
        return -1;
    }
}