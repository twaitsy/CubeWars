using System;
using UnityEngine;

public class UnitInspectorUI : MonoBehaviour
{
    enum InspectorTab { Overview, Stats, SkillsTools, Storage, Production, Combat }

    [Header("Layout (OnGUI)")]
    public float width = 340f;
    public float height = 440f;
    public float padding = 10f;

    private GameObject selected;
    private InspectorTab currentTab;

    public bool show = true;

    public void SetSelected(GameObject obj)
    {
        selected = obj;
    }

    void OnGUI()
    {
        if (!show || selected == null)
            return;

        float scale = RTSGameSettings.UIScale;
        Rect rect = new Rect(
            Screen.width - (width * scale) - padding,
            Screen.height - (height * scale) - padding,
            width * scale,
            height * scale
        );

        IMGUIInputBlocker.Register(rect);
        GUILayout.BeginArea(rect, GUI.skin.window);

        DrawHeader();
        DrawTabs();

        GUILayout.BeginVertical(GUI.skin.box);
        switch (currentTab)
        {
            case InspectorTab.Overview: DrawOverview(); break;
            case InspectorTab.Stats: DrawStats(); break;
            case InspectorTab.SkillsTools: DrawSkillsAndTools(); break;
            case InspectorTab.Storage: DrawStorage(); break;
            case InspectorTab.Production: DrawProduction(); break;
            case InspectorTab.Combat: DrawCombat(); break;
        }
        GUILayout.EndVertical();

        GUILayout.EndArea();
    }

    void DrawHeader()
    {
        GUILayout.Label(SanitizeName(selected.name), GUI.skin.box);
        int team = GetTeamID();
        if (team >= 0)
            GUILayout.Label($"Team: {team}");
    }

    void DrawTabs()
    {
        currentTab = (InspectorTab)GUILayout.Toolbar((int)currentTab,
            new[] { "Overview", "Stats", "Skills", "Storage", "Production", "Combat" });
    }

    void DrawOverview()
    {
        if (selected.TryGetComponent<IHasHealth>(out var health))
            GUILayout.Label($"Health: {health.CurrentHealth:0}/{health.MaxHealth:0}");

        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("Type: Civilian");
            GUILayout.Label($"Assignment: {ResolveAssignment(civ)}");
            GUILayout.Label($"Task: {civ.CurrentTaskLabel}");
            GUILayout.Label($"State: {civ.CurrentState}");
            GUILayout.Label($"Target: {civ.CurrentTargetName}");
        }
        else if (selected.TryGetComponent<Unit>(out _))
        {
            GUILayout.Label("Type: Unit");
        }
        else if (selected.TryGetComponent<Building>(out var building))
        {
            GUILayout.Label($"Type: {building.GetType().Name}");
        }
        else if (selected.TryGetComponent<ResourceNode>(out var node))
        {
            GUILayout.Label($"Resource Node: {node.type}");
            GUILayout.Label($"Remaining: {node.remaining}");
        }
    }

    void DrawStats()
    {
        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label($"Move Speed: {civ.speed:0.0}");
            GUILayout.Label($"Gather Tick: {civ.gatherTickSeconds:0.00}s");
            GUILayout.Label($"Harvest/Tick: {civ.harvestPerTick}");
            GUILayout.Label($"Carry Capacity: {civ.carryCapacity}");
        }

        if (selected.TryGetComponent<Unit>(out var unit))
        {
            GUILayout.Label($"Damage: {unit.damage}");
            GUILayout.Label($"Range: {unit.attackRange}");
            GUILayout.Label($"Combat Enabled: {(unit.combatEnabled ? "Yes" : "No")}");
        }

        if (TryGetSelectedComponent(out ConstructionSite site))
        {
            GUILayout.Label(site.GetStatusLine());
            var costs = site.GetRequiredCosts();
            if (costs != null)
            {
                foreach (var c in costs)
                    GUILayout.Label($"{c.type}: {site.GetDeliveredAmount(c.type)}/{c.amount}");
            }
        }
    }

    void DrawSkillsAndTools()
    {
        if (!selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("No skills/tools for this selection.");
            return;
        }

        GUILayout.Label("Assignment", GUI.skin.box);
        GUILayout.Label(ResolveAssignment(civ));

        GUILayout.Label("Role", GUI.skin.box);
        DrawRoleButtons(civ);

        GUILayout.Label("Job Specialization", GUI.skin.box);
        DrawJobTypeButtons(civ);
    }

    void DrawStorage()
    {
        if (selected.TryGetComponent<Civilian>(out var civ))
            GUILayout.Label($"Carrying: {civ.CarriedType} {civ.CarriedAmount}/{civ.carryCapacity}");

        if (selected.TryGetComponent<ResourceStorageContainer>(out var storage))
        {
            foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            {
                int cap = storage.GetCapacity(t);
                if (cap <= 0) continue;
                GUILayout.Label($"{t}: {storage.GetStored(t)}/{cap}");
            }
        }
        else
        {
            GUILayout.Label("No storage component.");
        }
    }

    void DrawProduction()
    {
        if (selected.TryGetComponent<Barracks>(out var barracks))
        {
            foreach (var def in barracks.producibleUnits)
            {
                if (def == null) continue;
                bool canAfford = barracks.CanQueue(def);
                GUI.enabled = canAfford;
                if (GUILayout.Button("Train " + def.unitName)) barracks.QueueUnit(def);
                GUI.enabled = true;
            }
            if (GUILayout.Button("Cancel Last")) barracks.CancelLast();
        }

        if (TryGetSelectedComponent(out CraftingBuilding crafting))
        {
            string recipeName = crafting.recipe != null ? crafting.recipe.recipeName : "None";
            GUILayout.Label($"Recipe: {recipeName}");
            GUILayout.Label($"State: {crafting.State}");
            GUILayout.Label($"Missing Inputs: {crafting.GetMissingInputSummary()}");
            GUILayout.Label($"Progress: {crafting.CraftProgress01:P0}");
        }
    }

    void DrawCombat()
    {
        if (!selected.TryGetComponent<UnitCombatController>(out var combat))
        {
            GUILayout.Label("No combat controls for this selection.");
            return;
        }

        GUILayout.Label("Target: " + combat.GetTargetStatus());

        bool attackCivilians = GUILayout.Toggle(combat.canAttackCivilians, "Attack Civilians");
        if (attackCivilians != combat.canAttackCivilians)
            combat.ToggleAttackCivilians();

        var newStance = (UnitCombatController.CombatStance)
            GUILayout.SelectionGrid((int)combat.stance, new[] { "Hold", "Guard", "Aggressive" }, 3);
        if (newStance != combat.stance)
            combat.SetStance(newStance);
    }

    static string ResolveAssignment(Civilian civ)
    {
        if (civ.JobType != CivilianJobType.Generalist)
            return civ.JobType.ToString();

        return civ.role.ToString();
    }

    void DrawRoleButtons(Civilian civ)
    {
        var roles = (CivilianRole[])Enum.GetValues(typeof(CivilianRole));
        DrawButtonGrid(roles, 3, role => civ.SetRole(role), role => role.ToString());
    }

    void DrawJobTypeButtons(Civilian civ)
    {
        var jobs = (CivilianJobType[])Enum.GetValues(typeof(CivilianJobType));
        DrawButtonGrid(jobs, 3, job => civ.SetJobType(job), job => job.ToString());
    }

    void DrawButtonGrid<T>(T[] values, int columns, Action<T> onSelect, Func<T, string> label)
    {
        for (int i = 0; i < values.Length; i += columns)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < columns; col++)
            {
                int idx = i + col;
                if (idx >= values.Length) { GUILayout.FlexibleSpace(); continue; }
                if (GUILayout.Button(label(values[idx]))) onSelect(values[idx]);
            }
            GUILayout.EndHorizontal();
        }
    }

    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        return raw.Replace("(Clone)", "").Trim();
    }

    int GetTeamID()
    {
        if (TryGetSelectedComponent(out Unit unit)) return unit.teamID;
        if (TryGetSelectedComponent(out Civilian civ)) return civ.teamID;
        if (TryGetSelectedComponent(out Building building)) return building.teamID;
        if (TryGetSelectedComponent(out ConstructionSite site)) return site.teamID;
        if (TryGetSelectedComponent(out ResourceStorageContainer storage)) return storage.teamID;
        return -1;
    }

    bool TryGetSelectedComponent<T>(out T component) where T : Component
    {
        component = null;
        if (selected == null)
            return false;

        if (selected.TryGetComponent(out component))
            return true;

        component = selected.GetComponentInParent<T>();
        return component != null;
    }
}
