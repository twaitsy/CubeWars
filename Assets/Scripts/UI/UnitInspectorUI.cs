using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitInspectorUI : MonoBehaviour
{
    public enum InspectorTab
    {
        Info,
        Attributes,
        Needs,
        RolesSkills,
        Inventory,
        BuildingStats,
        Storage,
        Production,
        TrainingQueue,
        CombatSettings,
        ConstructionStatus
    }

    [Header("Layout (OnGUI)")]
    public float width = 340f;
    public float height = 616f;
    public float padding = 10f;

    [Header("Typography")]
    [Min(0)] public int fontSizeBoost = 6;   // ← Increased from 1 → much bigger text

    private GameObject selected;
    private InspectorTab currentTab = InspectorTab.Info;
    private readonly List<InspectorTab> activeTabs = new();
    private Vector2 recipeSelectionScroll;
    private Vector2 recipeDetailsScroll;
    public bool show = true;

    public void SetSelected(GameObject obj)
    {
        selected = obj;
        RebuildTabs();
    }

    void OnGUI()
    {
        if (!show || selected == null)
            return;

        float scale = RTSGameSettings.UIScale;
        Rect rect = new(
            Screen.width - (width * scale) - padding,
            Screen.height - (height * scale) - padding,
            width * scale,
            height * scale
        );

        IMGUIInputBlocker.Register(rect);

        // Save original skin sizes
        int prevLabelSize = GUI.skin.label.fontSize;
        int prevBoxSize = GUI.skin.box.fontSize;
        int prevButtonSize = GUI.skin.button.fontSize;
        int prevToggleSize = GUI.skin.toggle.fontSize;

        // Apply bigger text
        GUI.skin.label.fontSize = Mathf.Max(10, prevLabelSize + fontSizeBoost);
        GUI.skin.box.fontSize = Mathf.Max(10, prevBoxSize + fontSizeBoost);
        GUI.skin.button.fontSize = Mathf.Max(10, prevButtonSize + fontSizeBoost);
        GUI.skin.toggle.fontSize = Mathf.Max(10, prevToggleSize + fontSizeBoost);

        GUILayout.BeginArea(rect, GUI.skin.window);

        DrawHeader();
        DrawTabs();

        GUILayout.BeginVertical(GUI.skin.box);
        switch (currentTab)
        {
            case InspectorTab.Info: DrawInfo(); break;
            case InspectorTab.Attributes: DrawAttributes(); break;
            case InspectorTab.Needs: DrawNeeds(); break;
            case InspectorTab.RolesSkills: DrawSkillsAndTools(); break;
            case InspectorTab.Inventory: DrawInventory(); break;
            case InspectorTab.BuildingStats: DrawBuildingStatsForSelected(); break;
            case InspectorTab.Storage: DrawStorage(); break;
            case InspectorTab.Production: DrawProductionForCrafting(); break;
            case InspectorTab.TrainingQueue: DrawTrainingQueue(); break;
            case InspectorTab.CombatSettings: DrawCombat(); break;
            case InspectorTab.ConstructionStatus: DrawConstructionStatus(); break;
        }
        GUILayout.EndVertical();

        GUILayout.EndArea();

        // Restore original sizes
        GUI.skin.label.fontSize = prevLabelSize;
        GUI.skin.box.fontSize = prevBoxSize;
        GUI.skin.button.fontSize = prevButtonSize;
        GUI.skin.toggle.fontSize = prevToggleSize;
    }

    // -----------------------
    // Tab configuration
    // -----------------------
    void DrawSkillsAndTools()
    {
        if (!selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("No roles or skills for this selection.");
            return;
        }
        GUILayout.Label("Assignment", GUI.skin.box);
        GUILayout.Label(ResolveAssignment(civ));
        GUILayout.Label("Role", GUI.skin.box);
        DrawRoleButtons(civ);
        GUILayout.Label("Job Specialization", GUI.skin.box);
        DrawJobTypeButtons(civ);
    }

    void DrawButtonGrid<T>(
        T[] values,
        int columns,
        Action<T> onSelect,
        Func<T, string> label)
    {
        for (int i = 0; i < values.Length; i += columns)
        {
            GUILayout.BeginHorizontal();
            for (int col = 0; col < columns; col++)
            {
                int idx = i + col;
                if (idx >= values.Length)
                {
                    GUILayout.FlexibleSpace();
                    continue;
                }
                if (GUILayout.Button(label(values[idx])))
                    onSelect(values[idx]);
            }
            GUILayout.EndHorizontal();
        }
    }


    void DrawRoleButtons(Civilian civ)
    {
        if (civ == null)
            return;

        GUILayout.Label("Role controls are currently unified under Job Specialization.");
    }

    void DrawJobTypeButtons(Civilian civ)
    {
        var jobs = (CivilianJobType[])Enum.GetValues(typeof(CivilianJobType));
        DrawButtonGrid(jobs, 3, job => civ.SetJobType(job), job => job.ToString());
    }

    void RebuildTabs()
    {
        activeTabs.Clear();
        if (selected == null) return;

        activeTabs.Add(InspectorTab.Info);

        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            activeTabs.Add(InspectorTab.Attributes);
            activeTabs.Add(InspectorTab.Needs);
            activeTabs.Add(InspectorTab.RolesSkills);
            activeTabs.Add(InspectorTab.Inventory);
            return;
        }
        if (selected.TryGetComponent<UnitCombatController>(out _))
        {
            activeTabs.Add(InspectorTab.Attributes);
            activeTabs.Add(InspectorTab.CombatSettings);
            return;
        }
        if (selected.TryGetComponent<Unit>(out _))
        {
            activeTabs.Add(InspectorTab.Attributes);
            return;
        }
        if (selected.TryGetComponent<Building>(out var building))
        {
            activeTabs.Add(InspectorTab.BuildingStats);
            if (building.TryGetComponent<ResourceStorageContainer>(out _))
                activeTabs.Add(InspectorTab.Storage);
            if (building.TryGetComponent<CraftingBuilding>(out _))
                activeTabs.Add(InspectorTab.Production);
            if (building.TryGetComponent<Barracks>(out _))
                activeTabs.Add(InspectorTab.TrainingQueue);
            return;
        }
        if (selected.TryGetComponent<ConstructionSite>(out _))
        {
            activeTabs.Add(InspectorTab.ConstructionStatus);
            return;
        }
    }

    string GetTabLabel(InspectorTab tab)
    {
        switch (tab)
        {
            case InspectorTab.Info: return "Info";
            case InspectorTab.Attributes: return "Attributes";
            case InspectorTab.Needs: return "Needs";
            case InspectorTab.RolesSkills: return "Jobs & Skills";
            case InspectorTab.Inventory: return "Inventory";
            case InspectorTab.BuildingStats: return "Building Stats";
            case InspectorTab.Storage: return "Storage";
            case InspectorTab.Production: return "Production";
            case InspectorTab.TrainingQueue: return "Training Queue";
            case InspectorTab.CombatSettings: return "Combat Settings";
            case InspectorTab.ConstructionStatus: return "Construction Status";
            default: return tab.ToString();
        }
    }

    void DrawTabs()
    {
        if (activeTabs.Count == 0) RebuildTabs();
        string[] labels = activeTabs.Select(t => GetTabLabel(t)).ToArray();
        int currentIndex = activeTabs.IndexOf(currentTab);
        if (currentIndex < 0) currentIndex = 0;
        int newIndex = GUILayout.Toolbar(currentIndex, labels);
        if (newIndex != currentIndex && newIndex >= 0 && newIndex < activeTabs.Count)
            currentTab = activeTabs[newIndex];
    }

    // -----------------------
    // Header
    // -----------------------
    void DrawHeader()
    {
        GUILayout.Label(SanitizeName(selected.name), GUI.skin.box);
        int team = GetTeamID();
        if (team >= 0)
            GUILayout.Label($"Team: {team}");
    }

    // -----------------------
    // Tab content routing
    // -----------------------
    void DrawInfo() => DrawOverview();
    void DrawAttributes() => DrawStats();
    void DrawNeeds()
    {
        if (!selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("No needs for this selection.");
            return;
        }
        GUILayout.Label("Needs", GUI.skin.box);
        GUILayout.Label($"Hunger: {civ.CurrentHunger:0.0}/{civ.maxHunger:0.0}");
        GUILayout.Label($"Tiredness: {civ.CurrentTiredness:0.0}/{civ.maxTiredness:0.0}");
        GUILayout.Label($"Water Need: {civ.CurrentWaterNeed:0.0}/{civ.maxWaterNeed:0.0}");
        GUILayout.Label($"House: {(civ.AssignedHouse != null ? SanitizeName(civ.AssignedHouse.name) : "Homeless")}");
    }

    void DrawInventory()
    {
        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("Inventory", GUI.skin.box);
            GUILayout.Label($"Carrying: {civ.CarriedType} {civ.CarriedAmount}/{civ.carryCapacity}");
        }
        else
        {
            GUILayout.Label("No inventory for this selection.");
        }
    }

    void DrawBuildingStatsForSelected()
    {
        if (TryGetSelectedComponent(out Building building))
            DrawBuildingStats(building);
        else
            GUILayout.Label("No building stats for this selection.");
    }

    void DrawProductionForCrafting()
    {
        if (!TryGetSelectedComponent(out CraftingBuilding crafting))
        {
            GUILayout.Label("No production for this selection.");
            return;
        }
        DrawRecipeSelection(crafting);
        GUILayout.Label($"State: {crafting.State}");
        GUILayout.Label($"Status: {crafting.GetProductionBlockerReason()}");
        GUILayout.Label($"Missing Inputs: {crafting.GetMissingInputSummary()}");
        GUILayout.Label($"Progress: {crafting.CraftProgress01:P0}");
        DrawRecipeSummary(crafting.recipe);
    }

    void DrawRecipeSelection(CraftingBuilding crafting)
    {
        GameDatabase loaded = GameDatabaseLoader.ResolveLoaded();
        var recipesDb = loaded != null ? loaded.recipes : null;
        if (recipesDb == null || recipesDb.recipes == null || recipesDb.recipes.Count == 0)
        {
            string recipeName = crafting.recipe != null ? crafting.recipe.recipeName : "None";
            GUILayout.Label($"Recipe: {recipeName}");
            return;
        }
        GUILayout.Label("Recipe", GUI.skin.box);
        recipeSelectionScroll = GUILayout.BeginScrollView(recipeSelectionScroll, GUILayout.Height(160f));
        for (int i = 0; i < recipesDb.recipes.Count; i++)
        {
            ProductionRecipeDefinition option = recipesDb.recipes[i];
            if (option == null) continue;
            bool isSelected = crafting.recipe == option;
            GUI.enabled = !isSelected;
            if (GUILayout.Button(isSelected ? $"✓ {option.recipeName}" : option.recipeName))
                crafting.SetRecipe(option);
            GUI.enabled = true;
        }
        GUILayout.EndScrollView();
    }

    void DrawRecipeSummary(ProductionRecipeDefinition recipe)
    {
        if (recipe == null)
        {
            GUILayout.Label("Recipe Details: None");
            return;
        }
        GUILayout.Space(4f);
        GUILayout.Label("Recipe Details", GUI.skin.box);
        recipeDetailsScroll = GUILayout.BeginScrollView(recipeDetailsScroll, GUILayout.Height(180f));
        GUILayout.Label($"Name: {recipe.recipeName}");
        GUILayout.Label($"Required Job: {recipe.requiredJobType}");
        GUILayout.Label($"Craft Time: {recipe.craftTimeSeconds:0.##}s");
        GUILayout.Label($"Batch Size: {recipe.batchSize}");
        GUILayout.Label("Inputs");
        if (recipe.inputs == null || recipe.inputs.Length == 0)
            GUILayout.Label("- None");
        else
        {
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                var input = recipe.inputs[i];
                if (input == null || input.resource == null) continue;
                GUILayout.Label($"- {input.resource.displayName}: {input.amount}");
            }
        }
        GUILayout.Label("Outputs");
        if (recipe.outputs == null || recipe.outputs.Length == 0)
            GUILayout.Label("- None");
        else
        {
            for (int i = 0; i < recipe.outputs.Length; i++)
            {
                var output = recipe.outputs[i];
                if (output == null || output.resource == null) continue;
                GUILayout.Label($"- {output.resource.displayName}: {output.amount}");
            }
        }
        GUILayout.EndScrollView();
    }

    void DrawTrainingQueue()
    {
        if (!selected.TryGetComponent<Barracks>(out var barracks))
        {
            GUILayout.Label("No training queue for this selection.");
            return;
        }
        GUILayout.Label("Training Queue", GUI.skin.box);
        foreach (var def in barracks.producibleUnits)
        {
            if (def == null) continue;
            bool canAfford = barracks.CanQueue(def);
            GUI.enabled = canAfford;
            if (GUILayout.Button("Train " + def.unitName)) barracks.QueueUnit(def);
            GUI.enabled = true;
        }
        if (GUILayout.Button("Cancel Last")) barracks.CancelLast();
        GUILayout.Space(4f);
        GUILayout.Label($"Queue Size: {barracks.QueueCount}");
        GUILayout.Label($"Build Progress: {barracks.CurrentProgress:P0}");
        GUILayout.Label($"Current Build Time: {barracks.CurrentBuildTime:0.00}s");
    }

    void DrawConstructionStatus()
    {
        if (!TryGetSelectedComponent(out ConstructionSite site))
        {
            GUILayout.Label("No construction status for this selection.");
            return;
        }
        GUILayout.Label("Construction Status", GUI.skin.box);
        GUILayout.Label(site.GetStatusLine());
        var costs = site.GetRequiredCosts();
        if (costs != null)
        {
            foreach (var c in costs)
                GUILayout.Label($"{c.resource}: {site.GetDeliveredAmount(c.resource)}/{c.amount}");
        }
    }

    // -----------------------
    // Overview & Stats
    // -----------------------
    void DrawOverview()
    {
        if (selected.TryGetComponent<IHasHealth>(out var health))
            GUILayout.Label($"Health: {health.CurrentHealth:0}/{health.MaxHealth:0}");

        if (selected.TryGetComponent<Civilian>(out var civ))
        {
            GUILayout.Label("Type: Civilian");
            GUILayout.Label($"Assignment: {ResolveAssignment(civ)}");
            GUILayout.Label($"Task: {civ.CurrentTaskLabel}");
            GUILayout.Label($"State: {civ.CurrentState} ({civ.CurrentStateDetails})");
            GUILayout.Label($"Target: {civ.CurrentTargetName}");
            GUILayout.Label($"Hunger: {civ.CurrentHunger:0.0}/{civ.maxHunger:0.0}");
            GUILayout.Label($"Tiredness: {civ.CurrentTiredness:0.0}/{civ.maxTiredness:0.0}");
            GUILayout.Label($"Water Need: {civ.CurrentWaterNeed:0.0}/{civ.maxWaterNeed:0.0}");
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
        else if (selected.TryGetComponent<ResourceNode>(out var node))
        {
            GUILayout.Label($"Resource Node: {node.resource}");
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
            GUILayout.Label($"Water Need Rate/s: {civ.waterRatePerSecond:0.00}");
        }
        if (selected.TryGetComponent<Building>(out var building))
            DrawBuildingStats(building);
        if (selected.TryGetComponent<House>(out var house))
        {
            GUILayout.Label($"Prestige: {house.prestige}");
            GUILayout.Label($"Comfort: {house.comfort}");
            GUILayout.Label($"Storage: {house.storage}");
            GUILayout.Label($"Inhabitants: {house.Civilians.Count}/{house.maxInhabitants}");
            foreach (var resident in house.Civilians)
            {
                if (resident != null)
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
                    GUILayout.Label($"{c.resource}: {site.GetDeliveredAmount(c.resource)}/{c.amount}");
            }
        }
    }

    void DrawBuildingStats(Building building)
    {
        if (building == null) return;
        GUILayout.Label($"Max Health: {building.maxHealth:0}");
        if (building.TryGetComponent<Headquarters>(out _))
            GUILayout.Label("Role: Team Core / Spawn Anchor");
        if (building.TryGetComponent<DefenseTurret>(out var turret))
        {
            GUILayout.Label($"Range: {turret.range:0.0}");
            GUILayout.Label($"Fire Rate: {turret.fireRate:0.00}/s");
            GUILayout.Label($"Damage: {turret.damage:0.0}");
            GUILayout.Label($"Targets Civilians: {(turret.canTargetCivilians ? "Yes" : "No")}");
        }
        if (building.TryGetComponent<ResourceStorageContainer>(out var storage))
        {
            int totalStored = 0;
            int totalCapacity = 0;
            var db = ResourcesDatabase.Instance;
            if (db != null && db.resources != null)
            {
                foreach (var t in db.resources)
                {
                    if (t == null) continue;
                    totalStored += storage.GetStored(t);
                    totalCapacity += storage.GetCapacity(t);
                }
            }
            GUILayout.Label($"Storage Total: {totalStored}/{totalCapacity}");
        }
        if (building.TryGetComponent<CraftingBuilding>(out var crafting))
        {
            GUILayout.Label($"Max Workers: {crafting.GetMaxWorkers()}");
            GUILayout.Label($"Assigned Workers: {crafting.AssignedWorkers.Count}");
            GUILayout.Label($"Hauler Logistics: {(crafting.requireHaulerLogistics ? "Required" : "Worker-handled")}");
            GUILayout.Label($"Input Buffer: {crafting.GetMissingInputSummary()}");
        }
    }

    // -----------------------
    // Storage (fixed)
    // -----------------------
    void DrawStorage()
    {
        if (selected.TryGetComponent<Civilian>(out var civ))
            GUILayout.Label($"Carrying: {civ.CarriedType} {civ.CarriedAmount}/{civ.carryCapacity}");

        bool hasStorage = TryGetSelectedComponent(out ResourceStorageContainer storage);
        bool any = false;
        var db = ResourcesDatabase.Instance;
        if (hasStorage && db != null && db.resources != null)
        {
            foreach (var t in db.resources)
            {
                if (t == null) continue;
                int cap = storage.GetCapacity(t);
                int stored = storage.GetStored(t);
                var flow = storage.GetFlowSetting(t);
                if (cap <= 0 && stored <= 0 && flow == ResourceStorageContainer.ResourceFlowMode.ReceiveAndSupply)
                    continue;
                any = true;
                GUILayout.Label($"{t.displayName} — {stored}/{cap} | Flow: {flow}");
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
                GUILayout.Label("Inputs");
                foreach (var entry in crafting.recipe.inputs)
                {
                    int amount = crafting.InputBuffer.TryGetValue(entry.resource, out int v) ? v : 0;
                    GUILayout.Label($"- {entry.resource.displayName}: {amount}/{crafting.maxInputCapacityPerResource}");
                }
                GUILayout.Label("Outputs");
                foreach (var entry in crafting.recipe.outputs)
                {
                    int amount = crafting.OutputQueue.TryGetValue(entry.resource, out int v) ? v : 0;
                    GUILayout.Label($"- {entry.resource.displayName}: {amount}/{crafting.maxOutputCapacityPerResource}");
                }
            }
        }

        if (!hasStorage && !selected.TryGetComponent<CraftingBuilding>(out _))
            GUILayout.Label("No local storage component.");
        else if (!any && !selected.TryGetComponent<CraftingBuilding>(out _))
            GUILayout.Label("Local storage: none configured.");
    }

    // -----------------------
    // Combat
    // -----------------------
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

    // -----------------------
    // Helpers
    // -----------------------
    static string ResolveAssignment(Civilian civ)
    {
        if (civ.JobType != CivilianJobType.Generalist)
            return civ.JobType.ToString();
        return civ.JobType.ToString();
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
        if (selected == null) return false;
        if (selected.TryGetComponent(out component)) return true;
        component = selected.GetComponentInParent<T>();
        return component != null;
    }
}