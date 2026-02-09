using UnityEngine;

public class UnitInspectorUI : MonoBehaviour
{
    [Header("Layout (OnGUI)")]
    public float width = 360f;
    public float height = 520f;
    public float padding = 10f;

    private GameObject selected;
    private Vector2 scroll;

    public void SetSelected(GameObject obj)
    {
        selected = obj;
        scroll = Vector2.zero;

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
        scroll = GUILayout.BeginScrollView(scroll);

        DrawHeader();
        DrawHealth();
        DrawCivilian();
        DrawUnit();
        DrawBarracks();
        DrawCombat();
        DrawConstruction();
        DrawStorage();
        DrawTurret();
        DrawCombatStance();

        GUILayout.EndScrollView();
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
            GUILayout.Label($"Health: {health.CurrentHealth:0}/{health.MaxHealth:0}");
    }

    void DrawCivilian()
    {
        if (!selected.TryGetComponent<Civilian>(out var civ))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Civilian", GUI.skin.box);
        GUILayout.Label($"Role: {civ.role}");
        GUILayout.Label($"State: {civ.CurrentStateName}");
        GUILayout.Label($"Target: {civ.GetCurrentTargetLabel()}");
        GUILayout.Label($"Carrying: {civ.CarriedAmount} {civ.CarriedType}");
        GUILayout.Label($"Gather Tick: {civ.gatherTickSeconds:0.00}s");
        GUILayout.Label($"Harvest / Tick: {civ.harvestPerTick}");
        GUILayout.Label($"Carry Capacity: {civ.carryCapacity}");
        GUILayout.Label($"Move Speed: {civ.speed:0.0}");

        GUILayout.Space(4);
        GUILayout.Label("Job", GUI.skin.box);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Gatherer")) civ.SetRole(CivilianRole.Gatherer);
        if (GUILayout.Button("Builder")) civ.SetRole(CivilianRole.Builder);
        if (GUILayout.Button("Hauler")) civ.SetRole(CivilianRole.Hauler);
        if (GUILayout.Button("Idle")) civ.SetRole(CivilianRole.Idle);
        GUILayout.EndHorizontal();
    }

    void DrawUnit()
    {
        if (!selected.TryGetComponent<Unit>(out var unit))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Unit", GUI.skin.box);
        GUILayout.Label($"Combat Enabled: {(unit.combatEnabled ? "Yes" : "No")}");
        GUILayout.Label($"Damage: {unit.damage}");
        GUILayout.Label($"Range: {unit.attackRange}");
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
                if (GUILayout.Button("Train")) barracks.QueueUnit(def);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
        }

        if (barracks.CurrentBuildTime > 0f)
        {
            float pct = barracks.CurrentProgress / barracks.CurrentBuildTime;
            GUILayout.HorizontalSlider(pct, 0f, 1f);
        }

        if (GUILayout.Button("Cancel Last")) barracks.CancelLast();
    }

    void DrawCombat()
    {
        if (!selected.TryGetComponent<UnitCombatController>(out var combat))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Combat", GUI.skin.box);
        GUILayout.Label("Target: " + combat.GetTargetStatus());

        bool newToggle = GUILayout.Toggle(combat.canAttackCivilians, "Attack Civilians");
        if (newToggle != combat.canAttackCivilians)
            combat.ToggleAttackCivilians();
    }

    void DrawTurret()
    {
        if (!selected.TryGetComponent<Turret>(out var turret))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Turret", GUI.skin.box);
        if (selected.TryGetComponent<UnitCombatController>(out var combat))
            GUILayout.Label("Target: " + combat.GetTargetStatus());
    }

    void DrawCombatStance()
    {
        if (!selected.TryGetComponent<UnitCombatController>(out var combat))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Combat Stance", GUI.skin.box);
        var newStance = (UnitCombatController.CombatStance)
            GUILayout.SelectionGrid((int)combat.stance, new[] { "Hold", "Guard", "Aggressive" }, 3);

        if (newStance != combat.stance)
            combat.SetStance(newStance);
    }

    void DrawConstruction()
    {
        if (!selected.TryGetComponent<ConstructionSite>(out var site))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Construction", GUI.skin.box);
        GUILayout.Label(site.GetStatusLine());

        var costs = site.GetRequiredCosts();
        if (costs == null) return;

        foreach (var c in costs)
        {
            int delivered = site.GetDeliveredAmount(c.type);
            GUILayout.Label($"{c.type}: {delivered}/{c.amount}");
        }
    }

    void DrawStorage()
    {
        if (!selected.TryGetComponent<ResourceStorageContainer>(out var storage))
            return;

        GUILayout.Space(6);
        GUILayout.Label("Storage", GUI.skin.box);
        foreach (ResourceType t in ResourceDefaults.AllTypes)
        {
            int cap = storage.GetCapacity(t);
            int stored = storage.GetStored(t);
            if (cap <= 0 && stored <= 0) continue;
            GUILayout.Label($"{t}: {stored}/{cap}");
        }
    }

    int GetTeamID()
    {
        if (selected.TryGetComponent<Unit>(out var unit)) return unit.teamID;
        if (selected.TryGetComponent<Civilian>(out var civ)) return civ.teamID;
        if (selected.TryGetComponent<Building>(out var b)) return b.teamID;
        if (selected.TryGetComponent<ConstructionSite>(out var site)) return site.teamID;
        if (selected.TryGetComponent<ResourceStorageContainer>(out var storage)) return storage.teamID;
        return -1;
    }
}
