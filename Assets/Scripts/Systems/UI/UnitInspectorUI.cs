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
            GUILayout.Label($"Hunger: {civ.CurrentHunger:0.0}/{civ.maxHunger:0.0}");
            GUILayout.Label($"Tiredness: {civ.CurrentTiredness:0.0}/{civ.maxTiredness:0.0}");
            GUILayout.Label($"House: {(civ.AssignedHouse != null ? SanitizeName(civ.AssignedHouse.name) : "Homeless")}");
        }
        else if (selected.TryGetComponent<Unit>(out _))
        {
            GUILayout.Label("Type: Unit");
        }
        else if (selected.TryGetComponent<Building>(out var building))
        {
            GUILayout.Label($"Type: {building.GetType().Name}");
            GUI.enabled = building.IsAlive;
            if (GUILayout.Button("Demolish Building"))
                building.Demolish();
            GUI.enabled = true;
        }
        else if (selected.TryGetComponent<Barracks>(out var barracks))
        {
            GUILayout.Label("Type: Barracks");
            if (GUILayout.Button("Demolish Building"))
            {
                if (barracks.TryGetComponent<Building>(out var barracksBuilding))
                    barracksBuilding.Demolish();
                else
                    Destroy(barracks.gameObject);
            }
        }
        else if (selected.TryGetComponent<Turret>(out var turret))
        {
            GUILayout.Label("Type: Turret");
            if (GUILayout.Button("Demolish Building"))
            {
                if (turret.TryGetComponent<Building>(out var turretBuilding))
                    turretBuilding.Demolish();
                else
                    Destroy(turret.gameObject);
            }
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
            GUILayout.Label($"Hunger Rate/s: {civ.hungerRatePerSecond:0.00}");
            GUILayout.Label($"Food/Meal: {civ.foodToEatPerMeal}");
            GUILayout.Label($"Tiredness Rate/s: {civ.tirednessRatePerSecond:0.00}");
        }

        if (selected.TryGetComponent<Building>(out var anyBuilding))
            DrawBuildingStats(anyBuilding);

        if (selected.TryGetComponent<House>(out var house))
        {
            GUILayout.Label($"Prestige: {house.prestige}");
            GUILayout.Label($"Comfort: {house.comfort}");
            GUILayout.Label($"Storage: {house.storage}");
            GUILayout.Label($"Inhabitants: {house.Civilians.Count}/{house.maxInhabitants}");
            for (int i = 0; i < house.Civilians.Count; i++)
            {
                var resident = house.Civilians[i];
                if (resident == null) continue;
                GUILayout.Label($"- {SanitizeName(resident.name)}");
            }
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


    void DrawBuildingStats(Building building)
    {
        if (building == null)
            return;

        GUILayout.Label($"Max Health: {building.maxHealth:0}");

        if (building.TryGetComponent<Headquarters>(out _))
            GUILayout.Label("Role: Team Core / Spawn Anchor");

        if (building.TryGetComponent<Farm>(out var farm))
        {
            GUILayout.Label($"Farm Level: {farm.level}/{farm.maxLevel}");
            GUILayout.Label($"Production Interval: {farm.productionInterval:0.00}s");
            int idx = Mathf.Clamp(farm.level - 1, 0, farm.foodPerTickByLevel.Length - 1);
            int output = farm.foodPerTickByLevel.Length > 0 ? farm.foodPerTickByLevel[idx] : 0;
            GUILayout.Label($"Food Per Tick: {output}");
        }

        if (building.TryGetComponent<DefenseTurret>(out var turret))
        {
            GUILayout.Label($"Range: {turret.range:0.0}");
            GUILayout.Label($"Fire Rate: {turret.fireRate:0.00}/s");
            GUILayout.Label($"Damage: {turret.damage:0.0}");
            GUILayout.Label($"Targets Civilians: {(turret.canTargetCivilians ? "Yes" : "No")}");
        }

        if (building.TryGetComponent<VehicleFactory>(out var vf))
        {
            GUILayout.Label($"Vehicle Build Time: {vf.buildSeconds:0.00}s");
            GUILayout.Label($"Vehicle Prefab: {(vf.vehiclePrefab != null ? vf.vehiclePrefab.name : "None")}");
        }

        if (building.TryGetComponent<WeaponsFactory>(out var wf))
        {
            GUILayout.Label($"Craft Time: {wf.craftSeconds:0.00}s");
            GUILayout.Label($"Weapon Tool: {(wf.weaponTool != null ? wf.weaponTool.name : "None")}");
        }

        if (building.TryGetComponent<Barracks>(out var barracks))
        {
            GUILayout.Label($"Queue Size: {barracks.QueueCount}");
            GUILayout.Label($"Build Progress: {barracks.CurrentProgress:P0}");
            GUILayout.Label($"Current Build Time: {barracks.CurrentBuildTime:0.00}s");
        }

        if (building.TryGetComponent<BuildingInteractionSettings>(out var interaction))
        {
            GUILayout.Label($"Stop Distances: D {interaction.defaultStopDistance:0.00} | H {interaction.houseStopDistance:0.00} | S {interaction.storageStopDistance:0.00}");
        }

        if (building.TryGetComponent<CraftingBuilding>(out var crafting))
        {
            GUILayout.Label($"Max Workers: {crafting.GetMaxWorkers()}");
            GUILayout.Label($"Assigned Workers: {crafting.AssignedWorkers.Count}");
            GUILayout.Label($"Hauler Logistics: {(crafting.requireHaulerLogistics ? "Required" : "Worker-handled")}");
            GUILayout.Label($"Input Buffer: {crafting.GetMissingInputSummary()}");
        }

        if (building.TryGetComponent<ResourceStorageContainer>(out var storage))
        {
            int totalStored = 0;
            int totalCapacity = 0;
            foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            {
                totalStored += storage.GetStored(t);
                totalCapacity += storage.GetCapacity(t);
            }
            GUILayout.Label($"Storage Total: {totalStored}/{totalCapacity}");
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

        bool hasStorage = TryGetSelectedComponent(out ResourceStorageContainer storage);
        bool any = false;

        if (hasStorage)
        {
            foreach (ResourceType t in Enum.GetValues(typeof(ResourceType)))
            {
                int cap = storage.GetCapacity(t);
                int stored = storage.GetStored(t);
                var flow = storage.GetFlowSetting(t);

                if (cap <= 0 && stored <= 0 && flow == ResourceStorageContainer.ResourceFlowMode.ReceiveAndSupply)
                    continue;

                any = true;
                GUILayout.Label($"{t} Storage — {stored}/{cap} | Flow: {flow}");
            }
        }

        if (TryGetSelectedComponent(out CraftingBuilding crafting))
        {
            GUILayout.Space(6f);
            GUILayout.Label("Crafting Buffers", GUI.skin.box);

            if (crafting.recipe == null)
            {
                GUILayout.Label("No recipe configured.");
            }
            else
            {
                var inputBuffer = crafting.InputBuffer;
                var outputQueue = crafting.OutputQueue;

                GUILayout.Label("Inputs");
                for (int i = 0; i < crafting.recipe.inputs.Length; i++)
                {
                    var entry = crafting.recipe.inputs[i];
                    int amount = inputBuffer != null && inputBuffer.ContainsKey(entry.resourceType) ? inputBuffer[entry.resourceType] : 0;
                    GUILayout.Label($"- {entry.resourceType}: {amount}/{crafting.maxInputCapacityPerResource}");
                }

                GUILayout.Label("Outputs");
                for (int i = 0; i < crafting.recipe.outputs.Length; i++)
                {
                    var entry = crafting.recipe.outputs[i];
                    int amount = outputQueue != null && outputQueue.ContainsKey(entry.resourceType) ? outputQueue[entry.resourceType] : 0;
                    GUILayout.Label($"- {entry.resourceType}: {amount}/{crafting.maxOutputCapacityPerResource}");
                }
            }
        }

        if (!hasStorage && !selected.TryGetComponent<CraftingBuilding>(out _))
            GUILayout.Label("No local storage component.");
        else if (!any && !selected.TryGetComponent<CraftingBuilding>(out _))
            GUILayout.Label("Local storage: none configured.");
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
