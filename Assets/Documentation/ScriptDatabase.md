# CUBE WARS SCRIPT DATABASE
Generated: 2026-02-07 22:21:25

## Table of Contents
- [Core](#core)
- [Data](#data)
- [Documents](#documents)
- [Runtime](#runtime)
- [Systems](#systems)
- [Ungrouped](#ungrouped)
- [Utilities](#utilities)

# Core

## Assets/Scripts/Core/Events/EventManager.cs
**Script:** EventManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
EventManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: EventManager, MonoBehaviour, eventually, handle, scripted, events, campaign, triggers.

### Comments & Documentation
- // This will eventually handle scripted events,
- // campaign triggers, waves, and timed missions.

### Keywords
and, campaign, EventManager, events, eventually, handle, missions, MonoBehaviour, scripted, This, timed, triggers, waves, will

### Cross References
**Uses:** -
**Used By:** Dependencies, GameBootstrap


## Assets/Scripts/Core/GameLoop/GameBootstrap.cs
**Script:** GameBootstrap
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
GameBootstrap appears to be a core script derived from MonoBehaviour. It appears to be related to: GameBootstrap, MonoBehaviour, ROOTS, WORLD, SUBROOTS, TEAMS, SYSTEMS, created.

### Comments & Documentation
- // ROOTS
- // WORLD SUBROOTS
- // TEAMS
- // SYSTEMS (created at runtime)
- // GAME MANAGER
- // UI + CAMERA

### Keywords
CAMERA, created, GAME, GameBootstrap, MANAGER, MonoBehaviour, ROOTS, runtime, SUBROOTS, SYSTEMS, TEAMS, WORLD

### Cross References
**Uses:** AIManager, BuildGridManager, BuildPlacementManager, CombatManager, ConstructionManager, EventManager, GameManager, JobManager, ProjectilePool, TeamResources, UnitManager, WinConditionManager
**Used By:** Dependencies


## Assets/Scripts/Core/NewBehaviourScript.cs
**Script:** NewBehaviourScript
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
NewBehaviourScript appears to be a core script derived from MonoBehaviour. It appears to be related to: NewBehaviourScript, MonoBehaviour, Start, called, before, first, frame, update.

### Comments & Documentation
- // Start is called before the first frame update
- // Update is called once per frame

### Keywords
before, called, first, frame, MonoBehaviour, NewBehaviourScript, once, per, Start, the, update


## Assets/Scripts/Core/Scene/HQSpawner.cs
**Script:** HQSpawner
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
HQSpawner appears to be a core script derived from MonoBehaviour. It appears to be related to: HQSpawner, MonoBehaviour, summary, Spawns, every, scene, already, lightweight.

### Comments & Documentation
- // / <summary>
- // / Spawns an HQ for every Team in the scene that does not already have one.
- // / This is a lightweight bootstrap component that ONLY handles HQ spawning.
- // / It does NOT spawn UI, camera, systems, or anything else.
- // / </summary>
- // Find all Team components in the scene
- // --- SAFETY CHECK 1: Ensure the team has an HQ root ---
- // --- SAFETY CHECK 2: Skip if HQ already exists ---
- // HQ already placed manually or by another system
- // --- SPAWN HQ ---
- /// <summary>
- /// Spawns an HQ for every Team in the scene that does not already have one.
- /// This is a lightweight bootstrap component that ONLY handles HQ spawning.
- /// It does NOT spawn UI, camera, systems, or anything else.
- /// </summary>

### Keywords
all, already, another, anything, ASSIGN, bootstrap, Building, camera, CHECK, clean, component, components, does, else, Ensure, every, exists, Find, for, from, handles, has, have, Headquarters, hierarchy, HQSpawner, inherits, lightweight, manually, MonoBehaviour, not, one, ONLY, OWNERSHIP, Parent, placed, root, SAFETY, scene, Skip, spawn, spawning, Spawns, summary, system, systems, Team, teamID, that, the, This, under, via

### Cross References
**Uses:** Building, Headquarters, Team
**Used By:** Dependencies


## Assets/Scripts/Core/Scene/RTSCamera.cs
**Script:** RTSCamera
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
RTSCamera appears to be a core script derived from MonoBehaviour. It appears to be related to: RTSCamera, MonoBehaviour, Update, movement, basis, rotating, rotate, Keyboard.

### Comments & Documentation
- // Update flat movement basis only when not rotating
- // RMB rotate
- // MMB drag pan
- // Keyboard pan
- // Edge scrolling
- // Apply pan velocity
- // Mouse wheel zoom (smooth)
- // Smooth zoom
- // Optional rotate keys
- // Optional bounds clamp

### Keywords
Apply, based, basis, bounds, clamp, drag, Dynamic, Edge, flat, Keyboard, keys, MMB, MonoBehaviour, Mouse, movement, not, only, Optional, pan, RMB, rotate, rotating, RTSCamera, scrolling, smooth, tilt, Update, velocity, wheel, when, zoom

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Core/Scene/SciFiMapBootstrap.cs
**Script:** SciFiMapBootstrap
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SciFiMapBootstrap appears to be a core script derived from MonoBehaviour. It appears to be related to: SciFiMapBootstrap, MonoBehaviour.

### Keywords
MonoBehaviour, SciFiMapBootstrap

### Cross References
**Uses:** TeamColorManager
**Used By:** Dependencies


## Assets/Scripts/Core/Scene/SixTeamBootstrap.cs
**Script:** SixTeamBootstrap
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SixTeamBootstrap appears to be a core script derived from MonoBehaviour. It appears to be related to: SixTeamBootstrap, MonoBehaviour, Spawn, civilians, Apply, children, important, storage.

### Comments & Documentation
- // Spawn civilians near HQ
- // Apply to root AND children (important: HQ storage/dropoff often live on child objects)
- // FIX: also set ResourceStorageContainer teamID (this was missing)
- // Team visuals (root + children)
- // If this specific object has a Building component, treat it as building; otherwise unit

### Keywords
also, AND, Apply, Building, child, children, civilians, component, dropoff, FIX, has, important, live, missing, MonoBehaviour, near, object, objects, often, otherwise, ResourceStorageContainer, root, set, SixTeamBootstrap, Spawn, specific, storage, Team, teamID, this, treat, unit, visuals, was

### Cross References
**Uses:** Building, Civilian, CivilianRole, Headquarters, ResourceDropoff, ResourceStorageContainer, ResourceStorageProvider, Team, TeamVisual, Unit
**Used By:** Dependencies


## Assets/Scripts/Core/Scene/Spawner.cs
**Script:** Spawner
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Spawner appears to be a core script derived from MonoBehaviour. It appears to be related to: Spawner, MonoBehaviour.

### Keywords
MonoBehaviour, Spawner

### Cross References
**Uses:** TeamColorManager, Unit
**Used By:** Dependencies


## Assets/Scripts/Core/Services/Dependencies.cs
**Script:** Dependencies
**System Classification:** Core
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Dependencies appears to be a core script derived from MonoBehaviour. It appears to be related to: Dependencies, MonoBehaviour, Global, Dependency, Alphabetical, Category, Based, documentation.

### Comments & Documentation
- // ============================================================================
- // Cube Wars — Global Dependency Map (Alphabetical + Category-Based)
- // This file is documentation-only. It compiles, but contains no runtime logic.
- // ============================================================================

### Keywords
Alphabetical, Based, but, Category, compiles, contains, Cube, Dependencies, Dependency, documentation, file, Global, logic, Map, MonoBehaviour, only, runtime, This, Wars

### Cross References
**Uses:** AIBuilder, AIBuildingPriority, AIDifficulty, AIEconomy, AIManager, AIMilitary, AIPersonality, AIPlayer, AIRebuildManager, AIRepairManager, AIResourceManager, AIThreatDetector, AlertManager, Attackable, AutoDestroyFX, Barracks, BuildingsDatabase, BuildCellReservation, BuildGridCell, BuildGridManager, Building, BuildItemDefinition, BuildItemInstance, BuildMenuUI, BuildPlacementManager, BuildTimeSettings, CharacterStats, Civilian, CivilianRole, CivilianSpawner, CombatManager, CombatStance, CombatSystem, ConstructionManager, ConstructionSite, CraftingSystem, DefenseTurret, DiplomacyManager, EconomyUI, EventManager, Farm, FindTeamDuplicates, GameBootstrap, GameManager, Headquarters, HQSpawner, IAttackable, ICommandable, IHasHealth, IMGUIInputBlocker, ITargetable, JobManager, MainMenuUI, MapVisualsBootstrap, Minimap, MultiTeamAIDirector, NeonRing, ParentRenamer, Projectile, ProjectilePool, ResourceCapacityEntry, ResourceCost, ResourceDropoff, ResourceNode, ResourceRegistry, ResourceSpawner, ResourceStorageContainer, ResourceStorageProvider, ResourceType, ResourceVisualBuilder, RTSCamera, SaveLoadManager, SciFiMapBootstrap, SciFiTeamStyler, Selectable, SelectionManager, SelectionRing, SelfRenamer, SixTeamBootstrap, SkillType, SkirmishConfig, Spawner, TaskBoardUI, Team, TeamAIBuild, TeamBootstrap, TeamColorManager, TeamColorUtils, TeamInventory, TeamResources, TeamStorageManager, TeamVisual, TeamWatchdog, Turret, TurretProjectile, TurretProjectilePool, Unit, UnitCombatController, UnitCommandController, UnitInspectorUI, UnitManager, UnitProductionDefinition, UnitProductionQueue, VehicleFactory, WeaponComponent, WeaponsFactory, WinConditionManager, WorldHealthBar
**Used By:** -


# Data

## Assets/Scripts/Data/AI/AIDifficulty.cs
**Script:** AIDifficulty
**System Classification:** AI
**Base Class:** -
**Interfaces:** -

### Summary
AIDifficulty appears to be a ai script. It appears to be related to: AIDifficulty.

### Keywords
AIDifficulty

### Cross References
**Uses:** -
**Used By:** AIPlayer, Dependencies, SkirmishConfig


## Assets/Scripts/Data/AI/AIPersonality.cs
**Script:** AIPersonality
**System Classification:** AI
**Base Class:** -
**Interfaces:** -

### Summary
AIPersonality appears to be a ai script. It appears to be related to: AIPersonality.

### Keywords
AIPersonality

### Cross References
**Uses:** -
**Used By:** AIBuilder, AIEconomy, AIMilitary, AIPlayer, Dependencies


## Assets/Scripts/Data/AI/Team.cs
**Script:** Team
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Team appears to be a ai script derived from MonoBehaviour. It appears to be related to: MonoBehaviour, summary, Defines, whether, controlled, player, IMPORTANT, Keeping.

### Comments & Documentation
- // / <summary>
- // / Defines whether a team is controlled by the player or AI.
- // /
- // / IMPORTANT:
- // / - Keeping this enum in the SAME file as Team ensures Unity never loses it.
- // / - Avoid placing this enum in other folders or duplicate files.
- // / </summary>
- // / <summary>
- // / Represents a single team in Cube Wars.
- // /
- /// <summary>
- /// Defines whether a team is controlled by the player or AI.
- /// IMPORTANT:
- /// - Keeping this enum in the SAME file as Team ensures Unity never loses it.
- /// - Avoid placing this enum in other folders or duplicate files.
- /// </summary>
- /// <summary>
- /// Represents a single team in Cube Wars.
- /// DEPENDENCIES:
- /// - TeamResources:

### Keywords
all, and, anything, Applies, assigned, Auto, Avoid, Building, buildings, capacity, child, color, colors, combat, containers, controlled, crafting, Cube, data, Defines, deletes, DEPENDENCIES, detect, Detects, duplicate, ensures, enum, exist, file, files, folders, for, GameManager, HQs, Identify, IDENTITY, IMPORTANT, INITIALIZATION, items, Keeping, loses, Manages, materials, MonoBehaviour, never, not, object, other, ownership, placing, player, Provide, Pure, references, Represents, resources, RESPONSIBILITIES, root, roots, SAME, script, scripts, single, Spawns, storage, Store, Stores, STRUCTURE, summary, systems, targeting, Team, TeamBootstrap, TeamColorManager, teamID, TeamInventory, TeamResources, teams, TeamStorageManager, the, they, this, Tracks, type, Unit, units, Unity, Use, Wars, whether, workers

### Cross References
**Uses:** Building, GameManager, TeamBootstrap, TeamColorManager, TeamInventory, TeamResources, TeamStorageManager, Unit
**Used By:** AIPlayer, AIResourceManager, Attackable, Building, BuildPlacementManager, Civilian, ConstructionSite, Dependencies, EconomyUI, Farm, FindTeamDuplicates, GameManager, Headquarters, HQSpawner, SixTeamBootstrap, TaskBoardUI, TeamAIBuild, TeamBootstrap, TeamColorManager, TeamInventory, TeamStorageManager, TeamVisual, Unit, UnitInspectorUI


## Assets/Scripts/Data/AI/TeamInventory.cs
**Script:** TeamInventory
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamInventory appears to be a ai script derived from MonoBehaviour. It appears to be related to: TeamInventory, MonoBehaviour, return, tools, TryGetValue, teamID, Representative, amount.

### Public Methods (excerpts)
```csharp
{
        if (item == null) return 0;
        if (!tools.TryGetValue(teamID, out var dict)) return 0;
        dict.TryGetValue(item, out int c);
        return c;
    }// Representative: if (item == null) return 0;

{
        if (item == null || amount <= 0) return;
        if (!tools.TryGetValue(teamID, out var dict))
        {
            dict = new Dictionary<ToolItem, int>();
            tools[teamID] = dict;
        }

// Representative: if (item == null || amount <= 0) return;

{
        if (item == null || amount <= 0) return false;
        if (!tools.TryGetValue(teamID, out var dict)) return false;
        dict.TryGetValue(item, out int c);
        if (c < amount) return false;
        c -= amount;
        if (c <= 0) dict.Remove(item);
        else dict[item] = c;
// Representative: if (item == null || amount <= 0) return false;

```

### Comments & Documentation
- // =============================================================
- // TeamInventory.cs
- // PURPOSE:
- // - Stores per-team tool counts (e.g., equipment, crafting tools).
- // DEPENDENCIES:
- // - ToolItem:
- // * Represents a tool type.
- // - Team.cs:
- // * teamID determines which inventory bucket to use.
- // NOTES FOR FUTURE MAINTENANCE:

### Keywords
amount, attach, bucket, component, Consider, counts, crafting, delete, DEPENDENCIES, determines, dict, dictionaries, Dictionary, does, else, equipment, false, fast, FOR, FUTURE, global, IMPORTANT, int, inventory, item, lookup, MAINTENANCE, MonoBehaviour, multiple, new, NOT, NOTES, null, objects, out, pattern, per, PURPOSE, Remove, replacing, Representative, Represents, return, script, singleton, stored, Stores, team, teamID, TeamInventory, teams, the, This, tool, ToolItem, tools, TryGetValue, type, use, uses, var, which, with

### Cross References
**Uses:** Team, ToolItem
**Used By:** CraftingSystem, Dependencies, Team, WeaponsFactory


## Assets/Data/DatabaseScripts/BuildingsDatabase.cs
**Script:** BuildingsDatabase
**System Classification:** Buildings
**Base Class:** ScriptableObject
**Interfaces:** -

### Summary
BuildingsDatabase appears to be a buildings script derived from ScriptableObject. It appears to be related to: BuildingsDatabase, ScriptableObject, PURPOSE, Defines, buildable, items, available, Organizes.

### Comments & Documentation
- // ============================================================================
- // BuildingsDatabase.cs
- // PURPOSE:
- // - Defines the list of all buildable items available in the game.
- // - Organizes items into high-level categories for UI and AI.
- // - Acts as the source of truth for BuildMenuUI when auto-discovering items.
- // DEPENDENCIES:
- // - BuildItemDefinition:
- // * Each entry in 'items' references a ScriptableObject describing a building.
- // - BuildMenuUI:

### Keywords
Acts, add, AIBuilder, aiPriority, all, and, appear, are, assets, auto, available, build, buildable, BuildingsDatabase, building, buildings, BuildItemDefinition, BuildItems, BuildMenuUI, categories, category, categoryOrder, control, decision, Defines, DEPENDENCIES, describing, determine, determines, discovering, discovers, discovery, displayName, Each, ensure, entry, exist, expectations, folder, for, FUTURE, game, high, included, INSPECTOR, into, items, level, list, localization, localized, logic, MAINTENANCE, making, match, must, names, new, NOTES, optional, order, Organizes, path, placed, PURPOSE, Reads, references, REQUIREMENTS, Resources, ScriptableObject, should, sort, source, TeamAIBuild, the, they, truth, update, Uses, using, what, when, you

### Cross References
**Uses:** AIBuilder, BuildItemDefinition, BuildMenuUI, TeamAIBuild
**Used By:** BuildItemDefinition, BuildMenuUI, Dependencies, TeamAIBuild


## Assets/Scripts/Data/Buildings/BuildItemDefinition.cs
**Script:** BuildItemDefinition
**System Classification:** Buildings
**Base Class:** ScriptableObject
**Interfaces:** -

### Summary
BuildItemDefinition appears to be a buildings script derived from ScriptableObject. It appears to be related to: BuildItemDefinition, ScriptableObject, PURPOSE, Defines, buildable, structure, metadata, container.

### Comments & Documentation
- // ============================================================================
- // BuildItemDefinition.cs
- // PURPOSE:
- // - Defines a buildable structure in Cube Wars.
- // - Acts as the metadata container for placement, cost, category, AI priority,
- // and prefab reference.
- // - Used by UI, AI, construction, and placement systems.
- // DEPENDENCIES:
- // - BuildPlacementManager:
- // * Uses prefab, yOffset, placementOffset, costs.

### Keywords
Acts, add, AIBuilder, aiPriority, AIRebuildManager, and, assets, based, build, buildable, BuildingsDatabase, building, buildings, BuildItemDefinition, BuildMenuUI, BuildPlacementManager, buildTime, category, cell, component, consider, constructing, construction, ConstructionSite, contain, container, cost, costs, Cube, decide, Defines, DEPENDENCIES, displayName, Displays, ensure, expectations, fail, final, footprint, for, FUTURE, grouping, here, icon, INSPECTOR, linking, logic, MAINTENANCE, match, metadata, multi, MUST, new, NOTES, override, placement, placementOffset, prefab, prerequisites, priority, PURPOSE, Reads, reference, requirements, resource, ResourceCost, ResourceType, rotation, rules, ScriptableObject, shown, Spawns, Stores, structure, systems, TeamAIBuild, tech, the, this, tree, UpgradeDefinition, upgrades, Used, Uses, Wars, what, will, yOffset, you

### Cross References
**Uses:** AIBuilder, AIBuildingPriority, AIRebuildManager, BuildingsDatabase, Building, BuildMenuUI, BuildPlacementManager, ConstructionSite, ResourceCost, ResourceType, TeamAIBuild
**Used By:** AIBuilder, AIRebuildManager, BuildingsDatabase, BuildItemInstance, BuildMenuUI, BuildPlacementManager, BuildTimeSettings, ConstructionSite, Dependencies, MultiTeamAIDirector, TeamAIBuild


## Assets/Scripts/Data/Buildings/BuildTimeSettings.cs
**Script:** BuildTimeSettings
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
BuildTimeSettings appears to be a buildings script derived from MonoBehaviour. It appears to be related to: BuildTimeSettings, MonoBehaviour, PURPOSE, Defines, construction, takes, complete, worked.

### Comments & Documentation
- // ============================================================================
- // BuildTimeSettings.cs
- // PURPOSE:
- // - Defines how long a construction site takes to complete when worked on by builders.
- // - Provides tuning values for builder work rate, max builders, and total required work.
- // - Acts as a configuration component for ConstructionSite or future building systems.
- // DEPENDENCIES:
- // - ConstructionSite (future integration):
- // * Currently ConstructionSite uses passive build + AddWork().
- // * If you integrate BuildTimeSettings, ConstructionSite should read:

### Keywords
accordingly, Acts, add, AddWork, AIBuilder, AIRebuildManager, and, animation, animations, Assigns, based, build, builder, builders, building, BuildItemDefinition, buildSeconds, buildTime, BuildTimeSettings, Calls, caps, complete, component, configuration, construction, ConstructionSite, contribution, Currently, Defines, DEPENDENCIES, determine, durations, dynamically, each, Engineer, estimate, for, future, Hauler, how, INSPECTOR, integrate, integration, its, JobManager, Laborer, long, MAINTENANCE, max, maxBuilders, May, modify, MonoBehaviour, morale, multi, multipliers, need, NOTES, Overrides, own, passive, per, Provides, PURPOSE, rate, read, required, REQUIREMENTS, roles, second, should, simultaneous, site, sites, specializations, speed, stage, systems, takes, tech, these, tie, total, tuning, upgrades, use, uses, values, weather, when, work, worked, Worker, workers, workRate, workRatePerBuilder, you

### Cross References
**Uses:** AIBuilder, AIRebuildManager, BuildItemDefinition, ConstructionSite, JobManager
**Used By:** Dependencies


## Assets/Scripts/Data/Buildings/ToolItem.cs
**Script:** ToolItem
**System Classification:** Buildings
**Base Class:** ScriptableObject
**Interfaces:** -

### Summary
ToolItem appears to be a buildings script derived from ScriptableObject. It appears to be related to: ToolItem, ScriptableObject.

### Keywords
ScriptableObject, ToolItem

### Cross References
**Uses:** ResourceCost, TeamResources
**Used By:** CharacterStats, CraftingSystem, TeamInventory, WeaponsFactory


## Assets/Scripts/Data/Combat/SkirmishConfig.cs
**Script:** SkirmishConfig
**System Classification:** Combat
**Base Class:** -
**Interfaces:** -

### Summary
SkirmishConfig appears to be a combat script. It appears to be related to: SkirmishConfig.

### Keywords
SkirmishConfig

### Cross References
**Uses:** AIDifficulty
**Used By:** Dependencies


## Assets/Scripts/Data/Resources/ResourceCapacityEntry.cs
**Script:** ResourceCapacityEntry
**System Classification:** Resources
**Base Class:** -
**Interfaces:** -

### Summary
ResourceCapacityEntry appears to be a resources script. It appears to be related to: ResourceCapacityEntry.

### Keywords
ResourceCapacityEntry

### Cross References
**Uses:** ResourceType
**Used By:** Dependencies, ResourceStorageProvider


## Assets/Scripts/Data/Resources/ResourceCost.cs
**Script:** ResourceCost
**System Classification:** Resources
**Base Class:** -
**Interfaces:** -

### Summary
ResourceCost appears to be a resources script. It appears to be related to: ResourceCost.

### Keywords
ResourceCost

### Cross References
**Uses:** ResourceType
**Used By:** BuildItemDefinition, BuildMenuUI, Civilian, ConstructionSite, Dependencies, TeamResources, TeamStorageManager, ToolItem, UnitInspectorUI, UnitProductionDefinition, VehicleFactory


## Assets/Scripts/Data/Resources/ResourceType.cs
**Script:** ResourceType
**System Classification:** Resources
**Base Class:** -
**Interfaces:** -

### Summary
ResourceType appears to be a resources script. It appears to be related to: ResourceType.

### Keywords
ResourceType

### Cross References
**Uses:** -
**Used By:** BuildItemDefinition, Civilian, ConstructionSite, Dependencies, EconomyUI, Farm, ResourceCapacityEntry, ResourceCost, ResourceDropoff, ResourceNode, ResourceRegistry, ResourceSpawner, ResourceStorageContainer, ResourceVisualBuilder, TaskBoardUI, TeamResources, TeamStorageManager, UnitInspectorUI


## Assets/Scripts/Data/Resources/TeamResources.cs
**Script:** TeamResources
**System Classification:** Resources
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamResources appears to be a resources script derived from MonoBehaviour. It appears to be related to: TeamResources, MonoBehaviour, TeamStorageManager, Instance, return, GetAvailable, teamID, Representative.

### Public Methods (excerpts)
```csharp
{
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetAvailable(teamID, type);
    }// Representative: if (TeamStorageManager.Instance == null) return 0;

{
        if (TeamStorageManager.Instance == null) return false;
        return TeamStorageManager.Instance.CanAffordAvailable(teamID, costs);
    }// Representative: if (TeamStorageManager.Instance == null) return false;

{
        if (!CanAfford(teamID, costs))
            return false;

        foreach (var c in costs)
            SpendResource(teamID, c.type, c.amount);

        return true;
// Representative: if (!CanAfford(teamID, costs))

{
        if (TeamStorageManager.Instance == null) return false;
        if (amount <= 0) return true;

        int taken = TeamStorageManager.Instance.Withdraw(teamID, type, amount);
        return taken == amount;
    }// Representative: if (TeamStorageManager.Instance == null) return false;

{
        if (TeamStorageManager.Instance == null) return 0;
        if (amount <= 0) return 0;

        return TeamStorageManager.Instance.Deposit(teamID, type, amount);
    }// Representative: if (TeamStorageManager.Instance == null) return 0;

{
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetTotalFree(teamID, type);
    }// Representative: if (TeamStorageManager.Instance == null) return 0;

{
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetTotalStored(teamID, type);
    }// Representative: if (TeamStorageManager.Instance == null) return 0;

```

### Comments & Documentation
- // / <summary>
- // / Global resource façade for Cube Wars.
- // /
- // / DEPENDENCIES:
- // / - TeamStorageManager:
- // /     Actual storage implementation.
- // / - ResourceType / ResourceCost:
- // /     Defines resource kinds and costs.
- // /
- // / RESPONSIBILITIES:
- /// <summary>
- /// Global resource façade for Cube Wars.
- /// DEPENDENCIES:
- /// - TeamStorageManager:
- /// Actual storage implementation.
- /// - ResourceType / ResourceCost:
- /// Defines resource kinds and costs.
- /// RESPONSIBILITIES:
- /// - Provide a stable API for resource queries
- /// - Forward calls to TeamStorageManager

### Keywords
Actual, ade, amount, and, API, calls, CanAfford, CanAffordAvailable, costs, Cube, Defines, delete, DEPENDENCIES, Deposit, Does, exist, false, for, foreach, Forward, GetAvailable, GetTotalFree, GetTotalStored, Global, implementation, IMPORTANT, Instance, int, kinds, MonoBehaviour, NOT, null, ONE, Only, pattern, Provide, queries, Representative, resource, ResourceCost, ResourceType, RESPONSIBILITIES, return, should, Singleton, SpendResource, stable, storage, summary, taken, teamID, TeamResources, teams, TeamStorageManager, true, type, var, Wars, Withdraw

### Cross References
**Uses:** ResourceCost, ResourceType, TeamStorageManager
**Used By:** AIBuilder, AIRebuildManager, Barracks, BuildMenuUI, BuildPlacementManager, Civilian, ConstructionSite, CraftingSystem, Dependencies, EconomyUI, Farm, GameBootstrap, MultiTeamAIDirector, ResourceDropoff, Team, TeamAIBuild, TeamStorageManager, ToolItem, UnitProductionQueue, VehicleFactory


## Assets/Scripts/Data/Units/CharacterStats.cs
**Script:** CharacterStats
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
CharacterStats appears to be a units script derived from MonoBehaviour. It appears to be related to: CharacterStats, MonoBehaviour, SkillProgress, GetSkill, Simple, curve, level, return.

### Public Methods (excerpts)
```csharp
{
        SkillProgress sp = GetSkill(t);
        // Simple curve: +5% per level
        return 1f + (sp.level * 0.05f);
    }// Representative: SkillProgress sp = GetSkill(t);

{
        if (amount <= 0f) return;

        SkillProgress sp = GetSkill(t);
        sp.xp += amount;

        // XP curve: 10 * (level+1)^2
        while (sp.xp >= XpToNext(sp.level))
// Representative: if (amount <= 0f) return;

{
        switch (t)
        {
            case SkillType.Mining: return mining;
            case SkillType.Building: return building;
            case SkillType.Farming: return farming;
            case SkillType.Hauling: return hauling;
            case SkillType.Combat: return combat;
            case SkillType.Driving: return driving;
            default: return mining;
        }
// Representative: switch (t)

{
        switch (t)
        {
            case SkillType.Mining: mining = sp; break;
            case SkillType.Building: building = sp; break;
            case SkillType.Farming: farming = sp; break;
            case SkillType.Hauling: hauling = sp; break;
            case SkillType.Combat: combat = sp; break;
            case SkillType.Driving: driving = sp; break;
        }
// Representative: switch (t)

```

### Comments & Documentation
- // Simple curve: +5% per level
- // XP curve: 10 * (level+1)^2

### Keywords
05f, amount, break, Building, case, CharacterStats, Combat, curve, default, Driving, Farming, GetSkill, Hauling, level, Mining, MonoBehaviour, per, Representative, return, Simple, SkillProgress, SkillType, switch, while, XpToNext

### Cross References
**Uses:** Building, SkillType, ToolItem
**Used By:** Civilian, Dependencies


## Assets/Scripts/Data/Units/CivilianRole.cs
**Script:** CivilianRole
**System Classification:** Units
**Base Class:** -
**Interfaces:** -

### Summary
CivilianRole appears to be a units script. It appears to be related to: CivilianRole, DEPENDENCIES, Civilian, decide, behaviour, state, machine, JobManager.

### Comments & Documentation
- // =============================================================
- // CivilianRole.cs
- // DEPENDENCIES:
- // - Civilian: uses this to decide behaviour/state machine
- // - JobManager: counts civilians per role
- // - TaskBoardUI: displays counts per role, including Idle
- // NOTES FOR FUTURE MAINTENANCE:
- // - If you add new roles, update Civilian.SetRole(), TaskBoardUI, and any AI/job logic.
- // =============================================================

### Keywords
add, and, any, behaviour, Civilian, CivilianRole, civilians, counts, decide, DEPENDENCIES, displays, FOR, FUTURE, Idle, including, job, JobManager, logic, machine, MAINTENANCE, new, NOTES, per, role, roles, SetRole, state, TaskBoardUI, this, update, uses, you

### Cross References
**Uses:** Civilian, JobManager, TaskBoardUI
**Used By:** Civilian, Dependencies, JobManager, SixTeamBootstrap, TaskBoardUI


## Assets/Scripts/Data/Units/CombatStance.cs
**Script:** CombatStance
**System Classification:** Units
**Base Class:** -
**Interfaces:** -

### Summary
CombatStance appears to be a units script. It appears to be related to: CombatStance.

### Keywords
CombatStance

### Cross References
**Uses:** -
**Used By:** Dependencies, UnitCombatController, UnitCommandController, UnitInspectorUI


## Assets/Scripts/Data/Units/SkillType.cs
**Script:** SkillType
**System Classification:** Units
**Base Class:** -
**Interfaces:** -

### Summary
SkillType appears to be a units script. It appears to be related to: SkillType.

### Keywords
SkillType

### Cross References
**Uses:** Building
**Used By:** CharacterStats, Dependencies


## Assets/Scripts/Data/Units/UnitProductionDefinition.cs
**Script:** UnitProductionDefinition
**System Classification:** Units
**Base Class:** ScriptableObject
**Interfaces:** -

### Summary
UnitProductionDefinition appears to be a units script derived from ScriptableObject. It appears to be related to: UnitProductionDefinition, ScriptableObject, DEPENDENCIES, Barracks, unitPrefab, buildTime, costs, enqueue.

### Comments & Documentation
- // =============================================================
- // UnitProductionDefinition.cs
- // DEPENDENCIES:
- // - Barracks:
- // * Uses unitPrefab, buildTime, costs to enqueue units.
- // - UnitProductionQueue:
- // * Uses buildTime and unitName for timing and UI.
- // - UnitInspectorUI:
- // * Displays unitName and costs for the player.
- // NOTES FOR FUTURE MAINTENANCE:

### Keywords
accordingly, add, and, Barracks, buildTime, consistent, cost, costs, definition, DEPENDENCIES, Displays, enqueue, extend, for, FUTURE, Keep, MAINTENANCE, new, NOTES, player, population, ScriptableObject, setup, the, this, timing, types, Unit, UnitCombatController, UnitInspectorUI, unitName, unitPrefab, UnitProductionDefinition, UnitProductionQueue, units, update, Uses, with, you

### Cross References
**Uses:** Barracks, ResourceCost, Unit, UnitCombatController, UnitInspectorUI, UnitProductionQueue
**Used By:** Barracks, CombatSystem, Dependencies, UnitInspectorUI, UnitProductionQueue


# Documents

## Assets/Scripts/Documents/CombatSystem.cs
**Script:** CombatSystem
**System Classification:** Unclassified
**Base Class:** -
**Interfaces:** -

### Summary
CombatSystem appears to be a script. It appears to be related to: CombatSystem, COMBAT, SYSTEM, MASTER, DOCUMENT, documents, entire, architecture.

### Comments & Documentation
- // ============================================================================
- // CUBE WARS — COMBAT SYSTEM MASTER DOCUMENT
- // ============================================================================
- // This file documents the entire combat architecture of Cube Wars.
- // It is NOT executable code. It exists to help developers and AI assistants
- // understand how the combat system works, how to extend it, and what each
- // GameObject requires in the Inspector.
- // ============================================================================
- // 1. OVERVIEW
- // ============================================================================

### Keywords
acquire, acquisition, Add, adding, additionally, Aggressive, AIMilitary, AlertManager, all, Allows, also, amount, and, animation, animations, Animator, Any, AoE, application, Applies, Apply, Archer, architecture, armor, around, ArrowTower, assign, assigned, assistants, attack, Attackable, attackableLayers, AVOID, based, Beam, bias, bool, both, buildings, built, Calling, Calls, can, CanFire, CannonTurret, cannot, civilians, code, COMBAT, CombatSystem, COMMON, component, COMPONENTS, continuous, cooldown, cooldowns, core, CREATE, CUBE, CurrentHealth, damage, damaged, death, design, Destroy, developers, difference, display, DOCUMENT, documents, Does, each, END, entire, events, executable, exist, exists, explosions, Exposes, extend, EXTENDING, fails, file, Fire, FireAtTarget, fireCooldown, Firing, flash, float, FLOW, following, for, four, from, future, GameObject, Guard, Handle, Handles, have, health, help, Hold, Homes, Homing, hooks, how, impact, include, Inspector, instance, Instantiate, int, integration, interface, IsAlive, isBuilding, isCivilian, IsDamaged, layer, LAYERS, logic, Mage, Manual, mask, MASTER, MaxHealth, Melee, Missing, modify, modular, MonoBehaviour, move, movement, moving, Multiple, MUST, muzzle, named, NavMeshAgent, needed, never, NEW, NOT, Notes, notifications, object, objects, ONCE, One, only, Optional, other, OVERVIEW, participates, per, pillars, PITFALLS, player, Plays, pool, pooling, Pools, prefab, Prevents, Projectile, ProjectilePool, projectilePrefab, projectiles, range, RANGED, recommended, Remove, Repair, replace, requests, require, REQUIRED, requires, resistance, RESPONSIBILITIES, return, Returns, Rifleman, rotates, Rotating, safely, same, scene, searches, Set, sets, shields, should, silently, spawn, spawning, Spawns, spikes, stance, Starts, static, SUMMARY, supports, SYSTEM, TAGS, TakeDamage, target, targeted, Targeting, targets, teamID, tech, that, the, THEM, these, they, This, timer, toward, tracks, trainable, transform, tree, TURRET, Turrets, understand, UNIT, UnitCombatController, UnitInspectorUI, UnitProductionDefinition, Units, Universal, upgrades, use, Used, valid, validation, via, void, WARS, weapon, WeaponComponent, weapons, what, with, works, You

### Cross References
**Uses:** AIMilitary, AlertManager, Attackable, Projectile, ProjectilePool, Turret, Unit, UnitCombatController, UnitInspectorUI, UnitProductionDefinition, WeaponComponent
**Used By:** Dependencies


# Runtime

## Assets/Scripts/Runtime/Buildings/Barracks.cs
**Script:** Barracks
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Barracks appears to be a buildings script derived from MonoBehaviour. It appears to be related to: Barracks, MonoBehaviour, return, false, TeamResources, Instance, CanAfford, teamID. It may share responsibilities with other production buildings such as Barracks, VehicleFactory, or WeaponsFactory.

### Public Methods (excerpts)
```csharp
{
        if (def == null) return false;
        if (TeamResources.Instance == null) return false;

        return TeamResources.Instance.CanAfford(teamID, def.costs);
    }// Representative: if (def == null) return false;

{
        if (!CanQueue(def))
            return;

        TeamResources.Instance.Spend(teamID, def.costs);
        queue.Enqueue(def);
    }// Representative: if (!CanQueue(def))

{
        if (producibleUnits == null || producibleUnits.Length == 0)
            return;

        QueueUnit(producibleUnits[0]);
    }// Representative: if (producibleUnits == null || producibleUnits.Length == 0)

```

### Comments & Documentation
- // ============================================================================
- // Barracks.cs
- // PURPOSE:
- // - Produces units over time using UnitProductionQueue.
- // - Acts as a factory for infantry or other unit types.
- // - Integrates with UI, AI, and resource systems.
- // DEPENDENCIES:
- // - UnitProductionDefinition:
- // * Defines unit prefab, cost, buildTime.
- // - UnitProductionQueue:

### Keywords
Acts, add, added, AIBuilder, AIEconomy, and, API, applied, assign, auto, Barracks, before, building, buildings, buildTime, can, CanAfford, CancelLast, CanQueue, change, components, cost, costs, CurrentBuildTime, CurrentProgress, def, Defines, DEPENDENCIES, dependency, Enqueue, enqueueing, EnqueueUnit, factory, false, for, forward, FUTURE, Handles, have, header, infantry, INSPECTOR, Instance, instantiate, instead, Integrates, Keep, Length, list, location, MAINTENANCE, missing, modify, MonoBehaviour, multi, multiple, must, new, NOTES, null, offset, OnUnitCompleted, other, over, points, prefab, produce, Produces, producibleUnits, production, proxy, PURPOSE, queue, QueueCount, QueueUnit, rally, Reads, related, Representative, REQUIREMENTS, resource, return, spawn, Spawned, Spend, subscribes, systems, teamID, TeamResources, TeamStorageManager, tech, them, this, time, timed, types, unit, UnitInspectorUI, UnitProductionDefinition, UnitProductionQueue, units, updated, upgrades, Used, Uses, using, when, with, you

### Cross References
**Uses:** AIBuilder, AIEconomy, TeamResources, TeamStorageManager, Unit, UnitInspectorUI, UnitProductionDefinition, UnitProductionQueue
**Used By:** AIEconomy, Dependencies, UnitInspectorUI, UnitProductionDefinition, UnitProductionQueue


## Assets/Scripts/Runtime/Buildings/Building.cs
**Script:** Building
**System Classification:** Buildings
**Base Class:** -
**Interfaces:** -

### Summary
Building appears to be a buildings script. It appears to be related to: Building, summary, class, buildings, DEPENDENCIES, ITargetable, IAttackable, IHasHealth.

### Comments & Documentation
- // / <summary>
- // / Base class for all buildings in Cube Wars.
- // /
- // / DEPENDENCIES:
- // / - ITargetable / IAttackable / IHasHealth:
- // /     Used by combat, AI targeting, and UI.
- // / - ConstructionSite:
- // /     Spawns final building prefabs that inherit from Building.
- // / - BuildPlacementManager:
- // /     Assigns teamID when placing buildings.
- /// <summary>
- /// Base class for all buildings in Cube Wars.
- /// DEPENDENCIES:
- /// - ITargetable / IAttackable / IHasHealth:
- /// Used by combat, AI targeting, and UI.
- /// - ConstructionSite:
- /// Spawns final building prefabs that inherit from Building.
- /// - BuildPlacementManager:
- /// Assigns teamID when placing buildings.
- /// - TeamVisual:

### Keywords
all, and, Applies, Assigns, Base, Building, buildings, BuildPlacementManager, class, colors, combat, consistent, ConstructionSite, Cube, death, delete, DEPENDENCIES, destroys, Does, final, for, from, Handle, health, IAttackable, IHasHealth, IMPORTANT, include, info, inherit, ITargetable, Many, must, NOT, Only, ownership, placing, prefabs, Provide, reaches, ResourceDropoff, ResourceStorageProvider, RESPONSIBILITIES, Spawns, Store, summary, targeting, team, teamID, teams, TeamVisual, that, the, these, THIS, Used, Wars, when, zero

### Cross References
**Uses:** BuildPlacementManager, ConstructionSite, IAttackable, IHasHealth, ITargetable, ResourceDropoff, ResourceStorageProvider, Team, TeamVisual
**Used By:** Attackable, BuildGridCell, BuildGridManager, BuildItemDefinition, BuildPlacementManager, CharacterStats, Civilian, ConstructionSite, DefenseTurret, Dependencies, Farm, Headquarters, HQSpawner, NeonRing, ResourceStorageContainer, ResourceStorageProvider, SciFiTeamStyler, SelectionRing, SixTeamBootstrap, SkillType, Team, TeamBootstrap, TeamStorageManager, TeamVisual, VehicleFactory, WeaponsFactory, WorldHealthBar


## Assets/Scripts/Runtime/Buildings/BuildItemInstance.cs
**Script:** BuildItemInstance
**System Classification:** Buildings
**Base Class:** -
**Interfaces:** -

### Summary
BuildItemInstance appears to be a buildings script. It appears to be related to: BuildItemInstance, PURPOSE, Identifies, placed, building, construction, BuildItemDefinition, Allows.

### Comments & Documentation
- // ============================================================================
- // BuildItemInstance.cs
- // PURPOSE:
- // - Identifies a placed building or construction site by its BuildItemDefinition.
- // - Allows systems to query building type without relying on class names.
- // - Essential for AI, rebuilding logic, and analytics.
- // DEPENDENCIES:
- // - BuildItemDefinition:
- // * itemId matches the ScriptableObject name (item.name).
- // - BuildPlacementManager:
- /// <summary>
- /// Attached to placed buildings and construction sites so we can count/identify them
- /// without relying on specific script/class names like "Turret" or "Stockpile".
- /// </summary>

### Keywords
add, adding, Adds, AIBuilder, AIRebuildManager, Allows, analytics, and, assigned, Attached, auto, building, BuildingInspector, buildings, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, built, Can, change, check, class, completion, component, consider, construction, ConstructionSite, count, definition, DEPENDENCIES, destroyed, determine, display, Ensures, Essential, exists, final, for, future, identifier, Identifies, identify, INSPECTOR, item, itemId, its, level, levels, like, Load, logic, MAINTENANCE, matches, multi, name, names, NOTES, placed, prerequisites, PURPOSE, query, rebuilding, relying, REQUIREMENTS, restore, safest, Save, script, ScriptableObject, serialize, Should, site, sites, skins, specific, statistics, Stockpile, summary, System, systems, tech, the, them, this, tree, Turret, type, upgraded, upgrades, use, used, Uses, using, variantId, variants, was, what, without, you

### Cross References
**Uses:** AIBuilder, AIRebuildManager, BuildItemDefinition, BuildPlacementManager, ConstructionSite, Turret
**Used By:** BuildPlacementManager, ConstructionSite, Dependencies, MultiTeamAIDirector


## Assets/Scripts/Runtime/Buildings/ConstructionSite.cs
**Script:** ConstructionSite
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ConstructionSite appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ConstructionSite, MonoBehaviour, teamID, gridCell, buildItem, costs, delivered, Clear.

### Properties
```csharp
bool InitOK { get;
int SiteKey { get;
```

### Public Methods (excerpts)
```csharp
{
        teamID = team;
        gridCell = cell;
        buildItem = item;
        costs = item != null ? item.costs : costs;

        delivered.Clear();
        if (costs != null)
// Representative: teamID = team;

{
        if (!InitOK || completed) return;
        if (!MaterialsComplete) return;

        buildProgress += workAmount;

        float requiredTime = GetBuildTime();
        if (buildProgress >= requiredTime)
// Representative: if (!InitOK || completed) return;

{
        if (costs == null) return 0;

        int required = 0;
        for (int i = 0; i < costs.Length; i++)
        {
            if (costs[i].type == type)
            {
                required = costs[i].amount;
                break;
            }
        }
// Representative: if (costs == null) return 0;

{
        if (amount <= 0) return 0;

        int missing = GetMissing(type);
        if (missing <= 0) return 0;

        int accepted = Mathf.Min(missing, amount);

// Representative: if (amount <= 0) return 0;

{
        if (completed) return "Completed";
        if (!MaterialsComplete) return "Awaiting Materials";
        return "Under Construction";
    }// Representative: if (completed) return "Completed";

```

### Comments & Documentation
- // ============================================================================
- // ConstructionSite.cs
- // PURPOSE:
- // - Represents an in-progress building.
- // - Tracks resource delivery, build progress, and completion.
- // - Spawns the final building prefab when finished.
- // DEPENDENCIES:
- // - BuildItemDefinition:
- // * Provides prefab, costs, buildTime override, category.
- // - BuildGridCell:

### Keywords
accepted, add, Added, AddWork, ade, adjust, alignment, amount, and, animations, Applies, ApplyTeamToPlacedObject, assigns, auto, automatically, Awaiting, baseBuildTime, bool, break, build, Builders, BuildGridCell, building, buildItem, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, buildProgress, buildTime, call, Calls, cancel, category, cell, Clear, colors, Complete, completed, completion, Construction, ConstructionSite, constructionSitePrefab, ConstructionSites, contain, costs, CountBuildersOnSite, deletes, delivered, delivery, DEPENDENCIES, destroys, determines, elsewhere, etc, fallback, filled, final, finished, float, for, from, FUTURE, GameObject, get, GetBuildTime, GetMissing, grid, gridCell, has, have, HELPERS, High, IAttackable, identification, IHasHealth, IMPORTANT, include, indirect, Init, InitOK, INSPECTOR, int, integrate, into, item, JobManager, Length, level, load, MAINTENANCE, Materials, MaterialsComplete, Mathf, may, Min, missing, MonoBehaviour, multi, Must, NEVER, new, not, NOTES, null, objects, occupies, only, optional, override, owns, passive, persist, phases, placement, placing, position, prefab, progress, Provides, PURPOSE, ReceiveDelivery, refunds, ReleaseReservation, remove, Representative, Represents, required, requiredTime, REQUIREMENTS, reservation, reservations, Reserve, ReserveForSite, resource, ResourceDropoff, resources, ResourceStorageContainer, ResourceStorageProvider, return, save, script, site, SiteKey, Spawns, split, stage, State, team, teamID, TeamResources, TeamStorageManager, TeamVisual, terrain, the, them, these, this, Tracks, treat, trigger, type, Under, Update, updated, upgrades, Used, via, what, when, which, with, WORK, workAmount, worker, you

### Cross References
**Uses:** BuildGridCell, Building, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, Civilian, IAttackable, IHasHealth, JobManager, ResourceCost, ResourceDropoff, ResourceStorageContainer, ResourceStorageProvider, ResourceType, Team, TeamResources, TeamStorageManager, TeamVisual, Unit
**Used By:** BuildGridCell, Building, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, BuildTimeSettings, Civilian, Dependencies, JobManager, MultiTeamAIDirector, ResourceStorageContainer, TeamStorageManager, UnitInspectorUI


## Assets/Scripts/Runtime/Buildings/DefenseTurret.cs
**Script:** DefenseTurret
**System Classification:** Buildings
**Base Class:** Building
**Interfaces:** -

### Summary
DefenseTurret appears to be a buildings script derived from Building. It appears to be related to: DefenseTurret, Building, Targetable, layer, shots, second, assign, prefab. It may share responsibilities with other defensive buildings such as Turret or DefenseTurret.

### Comments & Documentation
- // set to Targetable layer
- // shots per second
- // assign in prefab
- // optional (poolable)
- // rotating part (optional)
- // degrees per second
- // optional (muzzle mesh or head mesh)
- // NonAlloc buffer (no GC). Increase if you expect lots of targets in range.
- // MaterialPropertyBlock avoids creating per-turret material instances
- // Reacquire target occasionally (not every frame)

### Keywords
aimed, allocations, already, assign, avoids, buffer, Building, call, calling, choose, civilians, clear, components, concrete, creating, damage, Deal, DefenseTurret, degrees, direct, don, DontRequireReceiver, emissive, every, Example, expect, expose, Fire, flash, float, for, frame, Get, have, head, health, hit, hygiene, Increase, instances, ITargetable, layer, lots, material, MaterialPropertyBlock, max, mesh, method, MonoBehaviour, MPB, muzzle, new, NonAlloc, not, occasionally, one, only, optional, own, part, per, performance, poolable, prefab, Prefer, projectile, range, Reacquire, refs, rotating, roughly, safely, second, SendMessage, SendMessageOptions, set, shoot, shots, Simple, TakeDamage, target, Targetable, targets, team, turret, units, via, visuals, without, you, your

### Cross References
**Uses:** Building, Civilian, IHasHealth, ITargetable, TurretProjectile, TurretProjectilePool, Unit
**Used By:** Dependencies


## Assets/Scripts/Runtime/Buildings/Farm.cs
**Script:** Farm
**System Classification:** Buildings
**Base Class:** Building
**Interfaces:** -

### Summary
Farm appears to be a buildings script derived from Building. It appears to be related to: Building, TeamResources, Instance, return, false, level, maxLevel, nextLevel.

### Public Methods (excerpts)
```csharp
{
        if (TeamResources.Instance == null) return false;

        if (level >= maxLevel)
            return false;

        int nextLevel = level + 1;
        int costIndex = Mathf.Clamp(level - 1, 0, goldCostByNextLevel.Length - 1);
// Representative: if (TeamResources.Instance == null) return false;

```

### Comments & Documentation
- // cost to upgrade from level 1->2 uses [0], 2->3 uses [1], etc.
- // If no free storage, don't produce (prevents wasting)
- // Optional: if partially accepted, you can log or just ignore
- // Storage filled up; remaining food is "wasted" (or you can pause production next ticks)
- // Debug.Log($"Team {teamID} Farm could only store {accepted}/{foodToProduce} Food (storage full).");
- // Spend gold to upgrade
- // Simple visual feedback: scale up slightly by level

### Keywords
accepted, Building, can, Clamp, cost, costIndex, could, Debug, don, etc, false, Farm, feedback, filled, food, foodToProduce, free, from, full, gold, goldCostByNextLevel, ignore, Instance, int, just, Length, level, log, Mathf, maxLevel, next, nextLevel, null, only, Optional, partially, pause, prevents, produce, production, remaining, Representative, return, scale, Simple, slightly, Spend, storage, store, Team, teamID, TeamResources, ticks, upgrade, uses, visual, wasted, wasting, you

### Cross References
**Uses:** Building, ResourceType, Team, TeamResources
**Used By:** Dependencies


## Assets/Scripts/Runtime/Buildings/Headquarters.cs
**Script:** Headquarters
**System Classification:** Buildings
**Base Class:** Building
**Interfaces:** -

### Summary
Headquarters appears to be a buildings script derived from Building. It appears to be related to: Headquarters, Building, later, trigger, victory, defeat, logic.

### Comments & Documentation
- // TODO (later): trigger victory / defeat logic here

### Keywords
Building, defeat, Headquarters, here, later, logic, TODO, trigger, victory

### Cross References
**Uses:** Building, Team
**Used By:** BuildGridManager, Dependencies, HQSpawner, MultiTeamAIDirector, SixTeamBootstrap, WinConditionManager


## Assets/Scripts/Runtime/Buildings/ResourceDropoff.cs
**Script:** ResourceDropoff
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceDropoff appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ResourceDropoff, MonoBehaviour, acceptsOnly, Length, return, false, Representative, CanAccept.

### Public Methods (excerpts)
```csharp
{
        if (acceptsOnly == null || acceptsOnly.Length == 0) return true;
        for (int i = 0; i < acceptsOnly.Length; i++)
            if (acceptsOnly[i] == t) return true;
        return false;
    }// Representative: if (acceptsOnly == null || acceptsOnly.Length == 0) return true;

{
        if (!CanAccept(type)) return 0;
        if (TeamResources.Instance == null) return 0;
        return TeamResources.Instance.Deposit(teamID, type, amount);
    }// Representative: if (!CanAccept(type)) return 0;

```

### Keywords
acceptsOnly, amount, CanAccept, Deposit, false, for, Instance, int, Length, MonoBehaviour, null, Representative, ResourceDropoff, return, teamID, TeamResources, true, type

### Cross References
**Uses:** ResourceType, TeamResources
**Used By:** Building, BuildPlacementManager, ConstructionSite, Dependencies, SixTeamBootstrap


## Assets/Scripts/Runtime/Buildings/ResourceNode.cs
**Script:** ResourceNode
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceNode appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ResourceNode, MonoBehaviour, return, claimedByTeam, teamID, Representative, amount, IsDepleted.

### Public Methods (excerpts)
```csharp
{
        return claimedByTeam != -1 && claimedByTeam != teamID;
    }// Representative: return claimedByTeam != -1 && claimedByTeam != teamID;

{
        if (amount <= 0 || IsDepleted) return 0;
        int taken = Mathf.Min(remaining, amount);
        remaining -= taken;
        return taken;
    }// Representative: if (amount <= 0 || IsDepleted) return 0;

```

### Comments & Documentation
- // Legacy compatibility: some code uses node.amount
- // / <summary>
- // / Harvests up to 'amount' from this node. Returns how much was actually taken.
- // / </summary>
- /// <summary>
- /// Harvests up to 'amount' from this node. Returns how much was actually taken.
- /// </summary>

### Keywords
actually, amount, claimedByTeam, code, compatibility, from, Harvests, how, int, IsDepleted, Legacy, Mathf, Min, MonoBehaviour, much, node, remaining, Representative, ResourceNode, return, Returns, some, summary, taken, teamID, this, uses, was

### Cross References
**Uses:** ResourceType
**Used By:** AIResourceManager, Civilian, Dependencies, Minimap, ResourceRegistry, ResourceSpawner


## Assets/Scripts/Runtime/Buildings/ResourceSpawner.cs
**Script:** ResourceSpawner
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceSpawner appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ResourceSpawner, MonoBehaviour, clearPreviousChildren, ClearChildren, spawnedTotal, foreach, configs, count.

### Public Methods (excerpts)
```csharp
{
        if (clearPreviousChildren)
            ClearChildren();

        int spawnedTotal = 0;

        foreach (var cfg in configs)
        {
            if (cfg == null || cfg.count <= 0 || cfg.prefab == null) continue;

            for (int i = 0; i < cfg.count; i++)
            {
                if (TrySpawnOne(cfg, out GameObject spawned))
                    spawnedTotal++;
            }
        }
// Representative: if (clearPreviousChildren)

```

### Comments & Documentation
- // ---------------------------------------------------------
- // DEPENDENCIES:
- // - ResourceNode: must expose `remaining` and `type`
- // - ResourceNode.amount is read-only → must assign to `remaining`
- // - Prefabs must contain ResourceNode
- // ---------------------------------------------------------
- // Assign type and random amount
- // FIX: ResourceNode.amount is read-only → assign to remaining
- // Optional: assign value if ResourceNode has a compatible field
- // Scale object height based on amount

### Keywords
amount, and, assign, based, cfg, ClearChildren, clearPreviousChildren, compatible, configs, contain, continue, count, DEPENDENCIES, expose, field, FIX, for, foreach, GameObject, has, height, int, MonoBehaviour, must, null, object, only, Optional, out, prefab, Prefabs, random, read, remaining, Representative, ResourceNode, ResourceSpawner, Scale, spawned, spawnedTotal, TrySpawnOne, type, value, var

### Cross References
**Uses:** ResourceNode, ResourceType
**Used By:** Dependencies


## Assets/Scripts/Runtime/Buildings/ResourceStorageContainer.cs
**Script:** ResourceStorageContainer
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceStorageContainer appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ResourceStorageContainer, MonoBehaviour, return, stored, capacity, Mathf, Representative, GetFree.

### Public Methods (excerpts)
```csharp
{
        return stored[type];
    }

{
        return capacity[type];
    }

{
        return Mathf.Max(0, capacity[type] - stored[type]);
    }// Representative: return Mathf.Max(0, capacity[type] - stored[type]);

{
        int free = GetFree(type);
        int accepted = Mathf.Min(free, amount);
        stored[type] += accepted;
        return accepted;
    }// Representative: int free = GetFree(type);

{
        int taken = Mathf.Min(stored[type], amount);
        stored[type] -= taken;
        return taken;
    }// Representative: int taken = Mathf.Min(stored[type], amount);

{
        capacity[type] += amount;
    }// Representative: capacity[type] += amount;

```

### Comments & Documentation
- // =============================================================
- // ResourceStorageContainer.cs
- // PURPOSE:
- // - Represents a single storage container for a building or unit.
- // - Tracks stored amounts and capacity for each ResourceType.
- // - Registers itself with TeamStorageManager for global queries.
- // DEPENDENCIES:
- // - ResourceType:
- // * Enum defining all resource categories.
- // - TeamStorageManager:

### Keywords
accepted, accordingly, add, AddCapacity, Aggregates, all, amount, amounts, and, ARCHITECTURE, are, Attached, auto, automatically, Awake, behavior, belongs, bucket, building, buildings, call, capacity, categories, Civilian, ConstructionSite, container, containers, defining, deletes, DEPENDENCIES, determines, disable, during, each, enable, enforces, Enum, for, free, FUTURE, GameObjects, GetFree, global, have, initialize, int, itself, MAINTENANCE, Mathf, Max, May, Min, modifies, MonoBehaviour, multiple, NEVER, new, not, NOTES, per, provide, PURPOSE, queries, Registered, Registers, Representative, Represents, reservations, resource, ResourceStorageContainer, ResourceType, ResourceTypes, return, Safe, script, single, singleton, storage, stored, taken, team, teamID, teams, TeamStorageManager, temporary, that, they, this, totals, Tracks, type, unit, Unregistered, unregisters, upgrades, use, values, which, will, with, withdrawals, you

### Cross References
**Uses:** Building, Civilian, ConstructionSite, ResourceType, TeamStorageManager, Unit
**Used By:** AIBuilder, BuildPlacementManager, Civilian, ConstructionSite, Dependencies, ResourceStorageProvider, SixTeamBootstrap, TeamStorageManager, UnitInspectorUI


## Assets/Scripts/Runtime/Buildings/ResourceStorageProvider.cs
**Script:** ResourceStorageProvider
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceStorageProvider appears to be a buildings script derived from MonoBehaviour. It appears to be related to: ResourceStorageProvider, MonoBehaviour, started, return, registered, Unregister, Register, Representative.

### Public Methods (excerpts)
```csharp
{
        if (!started) return;

        if (registered)
            Unregister();

        Register();
    }// Representative: if (!started) return;

```

### Comments & Documentation
- // ---------------------------------------------------------
- // DEPENDENCIES:
- // - TeamStorageManager: must implement AddCapacity(teamID, type, amount)
- // and RemoveCapacity(teamID, type, amount)
- // - ResourceStorageContainer: actual storage objects
- // - Building: teamID must be set before Start()
- // ---------------------------------------------------------

### Keywords
actual, AddCapacity, amount, and, before, Building, DEPENDENCIES, implement, MonoBehaviour, must, objects, Register, registered, RemoveCapacity, Representative, ResourceStorageContainer, ResourceStorageProvider, return, set, Start, started, storage, teamID, TeamStorageManager, type, Unregister

### Cross References
**Uses:** Building, ResourceCapacityEntry, ResourceStorageContainer, TeamStorageManager
**Used By:** Building, BuildPlacementManager, ConstructionSite, Dependencies, SixTeamBootstrap


## Assets/Scripts/Runtime/Buildings/Turret.cs
**Script:** Turret
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Turret appears to be a buildings script derived from MonoBehaviour. It appears to be related to: Turret, MonoBehaviour, Updated, WeaponComponent, DEPENDENCIES, UnitCombatController, Handles, targeting. It may share responsibilities with other defensive buildings such as Turret or DefenseTurret.

### Comments & Documentation
- // =============================================================
- // Turret.cs (Updated for WeaponComponent)
- // DEPENDENCIES:
- // - UnitCombatController:
- // * Handles targeting + stance logic.
- // - WeaponComponent:
- // * Handles firing.
- // - Attackable:
- // * Makes turret a valid target.
- // NOTES FOR FUTURE MAINTENANCE:

### Keywords
add, array, Attackable, base, component, DEPENDENCIES, firing, for, FUTURE, gain, Handles, logic, MAINTENANCE, Makes, MonoBehaviour, multiple, NOTES, rotating, rotation, stance, target, targeting, Turret, turrets, UnitCombatController, Updated, valid, WeaponComponent, weapons

### Cross References
**Uses:** Attackable, UnitCombatController, WeaponComponent
**Used By:** BuildItemInstance, CombatSystem, Dependencies, Minimap, TurretProjectile, TurretProjectilePool, UnitInspectorUI


## Assets/Scripts/Runtime/Buildings/VehicleFactory.cs
**Script:** VehicleFactory
**System Classification:** Buildings
**Base Class:** Building
**Interfaces:** -

### Summary
VehicleFactory appears to be a buildings script derived from Building. It appears to be related to: VehicleFactory, Building, return, TeamResources, Instance, vehiclePrefab, CanAfford, teamID. It may share responsibilities with other production buildings such as Barracks, VehicleFactory, or WeaponsFactory.

### Public Methods (excerpts)
```csharp
{
        if (building) return;
        if (TeamResources.Instance == null) return;
        if (vehiclePrefab == null) return;

        if (!TeamResources.Instance.CanAfford(teamID, buildCost)) return;
        if (!TeamResources.Instance.Spend(teamID, buildCost)) return;

// Representative: if (building) return;

```

### Keywords
buildCost, Building, CanAfford, Instance, null, Representative, return, Spend, teamID, TeamResources, VehicleFactory, vehiclePrefab

### Cross References
**Uses:** Building, ResourceCost, TeamResources
**Used By:** Dependencies


## Assets/Scripts/Runtime/Buildings/WeaponsFactory.cs
**Script:** WeaponsFactory
**System Classification:** Buildings
**Base Class:** Building
**Interfaces:** -

### Summary
WeaponsFactory appears to be a buildings script derived from Building. It appears to be related to: WeaponsFactory, Building, crafting, return, weaponTool, CraftingSystem, Instance, Spend. It may share responsibilities with other production buildings such as Barracks, VehicleFactory, or WeaponsFactory.

### Public Methods (excerpts)
```csharp
{
        if (crafting) return;
        if (weaponTool == null) return;
        if (CraftingSystem.Instance == null) return;

        // Spend immediately via CraftingSystem (uses ToolItem.craftCost)
        bool ok = CraftingSystem.Instance.CraftTool(teamID, weaponTool, 1);
        if (!ok) return;
// Representative: if (crafting) return;

```

### Comments & Documentation
- // Spend immediately via CraftingSystem (uses ToolItem.craftCost)
- // Optional: make it take time (we already spent cost)
- // Tool already added by CraftingSystem; this timer is just “manufacturing time”

### Keywords
added, already, bool, Building, cost, craftCost, crafting, CraftingSystem, CraftTool, immediately, Instance, just, make, manufacturing, null, Optional, Representative, return, Spend, spent, take, teamID, this, time, timer, Tool, ToolItem, uses, via, WeaponsFactory, weaponTool

### Cross References
**Uses:** Building, CraftingSystem, TeamInventory, ToolItem
**Used By:** Dependencies


## Assets/Scripts/Runtime/Effects/AutoDestroyFX.cs
**Script:** AutoDestroyFX
**System Classification:** Unclassified
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AutoDestroyFX appears to be a script derived from MonoBehaviour. It appears to be related to: AutoDestroyFX, MonoBehaviour.

### Keywords
AutoDestroyFX, MonoBehaviour

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Runtime/Projectiles/Projectile.cs
**Script:** Projectile
**System Classification:** Combat
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Projectile appears to be a combat script derived from MonoBehaviour. It appears to be related to: Projectile, MonoBehaviour, target, damage, sourceTeam, teamID, lifeTimer, Representative.

### Public Methods (excerpts)
```csharp
{
        this.target = target;
        this.damage = damage;
        this.sourceTeam = teamID;
        lifeTimer = 0f;
    }// Representative: this.target = target;

```

### Comments & Documentation
- // =============================================================
- // Projectile.cs (Unified + Pooling Compatible)
- // DEPENDENCIES:
- // - Attackable:
- // * Must expose: teamID, IsAlive, TakeDamage(float)
- // - ProjectilePool:
- // * Handles pooling.
- // - WeaponComponent:
- // * Spawns this projectile.
- // NOTES FOR FUTURE MAINTENANCE:

### Keywords
add, armor, Attackable, ballistic, based, Compatible, damage, DEPENDENCIES, expose, float, FOR, FUTURE, Handles, homing, IsAlive, lifeTimer, logic, MAINTENANCE, MonoBehaviour, Must, NOTES, physics, Pooling, Projectile, ProjectilePool, replace, Representative, resistance, Rigidbody, sourceTeam, Spawns, system, TakeDamage, target, teamID, this, Unified, Update, WeaponComponent, with, wrap, you

### Cross References
**Uses:** Attackable, ProjectilePool, WeaponComponent
**Used By:** Attackable, CombatSystem, Dependencies, ProjectilePool, TurretProjectile, WeaponComponent


## Assets/Scripts/Runtime/Projectiles/TurretProjectile.cs
**Script:** TurretProjectile
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TurretProjectile appears to be a buildings script derived from MonoBehaviour. It appears to be related to: TurretProjectile, MonoBehaviour, ownerTeamID, target, targetTf, damage, speed, Representative. It may share responsibilities with other defensive buildings such as Turret or DefenseTurret.

### Public Methods (excerpts)
```csharp
{
        this.ownerTeamID = ownerTeamID;
        target = targetTf;
        damage = dmg;
        speed = spd;
        life = 2.0f;
    }// Representative: this.ownerTeamID = ownerTeamID;

```

### Comments & Documentation
- // =============================================================
- // TurretProjectile.cs
- // DEPENDENCIES:
- // - TurretProjectilePool:
- // * Spawns and despawns this projectile.
- // - Attackable (MonoBehaviour):
- // * Used to apply damage on impact (via TakeDamage or similar).
- // NOTES FOR FUTURE MAINTENANCE:
- // - Right now this is a standalone projectile system, separate from the
- // Projectile used by UnitCombatController.
- /// <summary>
- /// Initialize the projectile.
- /// </summary>

### Keywords
and, API, apply, Attackable, compatible, component, damage, DEPENDENCIES, despawns, dmg, Ensure, FOR, from, FUTURE, generic, has, impact, Initialize, instead, life, MAINTENANCE, MonoBehaviour, NOTES, now, ownerTeamID, pool, projectile, Representative, Right, script, separate, similar, spawn, Spawns, spd, speed, standalone, summary, system, TakeDamage, target, targetTf, the, this, Turret, TurretProjectile, TurretProjectilePool, turrets, UnitCombatController, use, Used, via, want, wire, with, you

### Cross References
**Uses:** Attackable, Projectile, Turret, TurretProjectilePool, UnitCombatController
**Used By:** DefenseTurret, Dependencies, TurretProjectilePool


## Assets/Scripts/Runtime/Units/Attackable.cs
**Script:** Attackable
**System Classification:** Combat
**Base Class:** MonoBehaviour
**Interfaces:** IHasHealth

### Summary
Attackable appears to be a combat script derived from MonoBehaviour. It appears to be related to: Attackable, MonoBehaviour, IHasHealth, canBeRepaired, IsAlive, return, currentHealth, Mathf.

### Public Methods (excerpts)
```csharp
{
        if (!canBeRepaired || !IsAlive) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }// Representative: if (!canBeRepaired || !IsAlive) return;

{
        if (!IsAlive) return;

        currentHealth -= dmg;

        // Alert system
        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.Push($"{name} is under attack!");
        }
// Representative: if (!IsAlive) return;

```

### Comments & Documentation
- // ============================================================================
- // Attackable.cs
- // PURPOSE:
- // - Universal health + damage interface for ANY object that can be attacked.
- // - Used by units, civilians, buildings, turrets, and future entities.
- // - Provides a consistent API for combat, AI, UI, and projectiles.
- // DEPENDENCIES:
- // - IHasHealth:
- // * Interface requiring CurrentHealth, MaxHealth.
- // - UnitCombatController / WeaponComponent:

### Keywords
add, AIMilitary, Alert, AlertManager, amount, and, animation, animations, ANY, API, armor, attack, Attackable, attacked, auto, BEFORE, bias, Building, buildings, calculation, Call, Calls, can, canBeRepaired, civilians, combat, consistent, currentHealth, damage, death, DEPENDENCIES, Destroy, destroyed, detect, diplomacy, DiplomacyManager, Displays, dmg, drones, engineers, ensure, entities, event, factions, for, future, gameObject, handler, health, Helper, hit, IHasHealth, impact, Instance, integrates, interface, IsAlive, isBuilding, isCivilian, itself, layer, MAINTENANCE, marks, Mathf, maxHealth, Min, MonoBehaviour, name, NOTE, NOTES, notifications, null, object, personality, pool, pooling, present, Projectile, ProjectilePool, projectiles, property, Provides, PURPOSE, Push, Query, Reads, reduced, remains, repair, replace, Representative, requiring, resistance, return, safe, shield, shields, system, TakeDamage, targeting, teamID, that, this, turrets, under, UnitCombatController, UnitInspectorUI, units, Universal, Used, Uses, values, WeaponComponent, when, with, wrap, you

### Cross References
**Uses:** AIMilitary, AlertManager, Building, DiplomacyManager, IHasHealth, Projectile, ProjectilePool, Team, UnitCombatController, UnitInspectorUI, WeaponComponent
**Used By:** AIMilitary, AIRepairManager, AIResourceManager, AIThreatDetector, CombatSystem, Dependencies, DiplomacyManager, Projectile, SelectionManager, Turret, TurretProjectile, UnitCombatController, WeaponComponent


## Assets/Scripts/Runtime/Units/Civilian.cs
**Script:** Civilian
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** ITargetable, IHasHealth

### Summary
Civilian appears to be a units script derived from MonoBehaviour. It appears to be related to: Civilian, MonoBehaviour, ITargetable, IHasHealth, started, return, registeredWithJobManager, UnregisterFromJobManager.

### Properties
```csharp
ResourceNode CurrentReservedNode { get;
ConstructionSite CurrentAssignedSite { get;
ConstructionSite CurrentDeliverySite { get;
```

### Public Methods (excerpts)
```csharp
{
        if (!started) return;

        if (registeredWithJobManager)
            UnregisterFromJobManager();

        RegisterWithJobManager();
    }// Representative: if (!started) return;

{
        role = newRole;

        targetNode = null;
        targetSite = null;
        targetStorage = null;

        CurrentReservedNode = null;
// Representative: role = newRole;

{
        HasJob = node != null;
        CurrentNode = node;

        carriedAmount = 0;
        targetSite = null;
        targetStorage = null;

// Representative: HasJob = node != null;

{
        if (!IsAlive) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if (agent != null) agent.isStopped = true;
            Destroy(gameObject);
        }
// Representative: if (!IsAlive) return;

{
        return transform;
    }

```

### Comments & Documentation
- // Compatibility fields referenced elsewhere
- // Expose carried for UI
- // Legacy/compat fields (kept for external references)
- // IHasHealth / ITargetable style properties
- // Registration timing: teamID is often assigned right after Instantiate (before Start).
- // So we register with JobManager in Start, and only re-register on OnEnable if Start has run.
- // Only register here if Start has already run (prevents wrong-team registration on Instantiate)
- // / <summary>
- // / Call this if you change teamID at runtime and want the TaskBoard counts to move teams.
- // / </summary>
- /// <summary>
- /// Call this if you change teamID at runtime and want the TaskBoard counts to move teams.
- /// </summary>
- /// <summary>
- /// Explicit job assignment from AI/job system.
- /// Forces this civilian into gatherer behaviour on a specific node.
- /// </summary>

### Keywords
accept, after, agent, already, and, are, assigned, assignment, avoid, back, before, behaviour, Builder, Call, can, carried, carriedAmount, change, Civilian, compat, Compatibility, Construction, ConstructionSite, couldn, counts, CurrentAssignedSite, CurrentDeliverySite, currentHealth, CurrentNode, CurrentReservedNode, damage, deposit, Destroy, drop, dump, elsewhere, expects, Explicit, Expose, external, fallback, fields, for, Forces, free, from, full, gameObject, gatherer, get, has, HasJob, Hauler, have, Helpers, here, IHasHealth, Instantiate, interface, into, IsAlive, isStopped, ITargetable, job, JobManager, keep, kept, Legacy, like, MonoBehaviour, move, Must, newRole, next, node, null, often, OnEnable, only, phantom, physical, physically, pick, prevents, primary, properties, put, Reduce, referenced, references, register, registeredWithJobManager, RegisterWithJobManager, Registration, remaining, removed, Representative, reservation, reserved, ResourceNode, resources, return, right, role, run, runtime, simple, site, somehow, something, soon, specific, Start, started, storage, style, summary, Supply, system, targetNode, targetSite, targetStorage, TaskBoard, team, teamID, TeamResources, teams, the, this, tick, timing, transform, true, UnregisterFromJobManager, want, was, what, with, wrong, you, your

### Cross References
**Uses:** Building, CharacterStats, CivilianRole, ConstructionSite, IHasHealth, ITargetable, JobManager, ResourceCost, ResourceNode, ResourceStorageContainer, ResourceType, Team, TeamResources, TeamStorageManager
**Used By:** BuildPlacementManager, CivilianRole, CivilianSpawner, ConstructionSite, DefenseTurret, Dependencies, JobManager, NeonRing, ResourceStorageContainer, SciFiTeamStyler, SelectionRing, SixTeamBootstrap, TaskBoardUI, TeamStorageManager, TeamVisual, UnitInspectorUI


## Assets/Scripts/Runtime/Units/TeamVisual.cs
**Script:** TeamVisual
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamVisual appears to be a units script derived from MonoBehaviour. It appears to be related to: TeamVisual, MonoBehaviour, SciFiTeamStyler, Instance, Apply, gameObject, teamID, TeamColorManager.

### Public Methods (excerpts)
```csharp
{
        if (SciFiTeamStyler.Instance != null)
            SciFiTeamStyler.Instance.Apply(gameObject, teamID, kind);
        else if (TeamColorManager.Instance != null)
            TeamColorManager.Instance.ApplyTeamColor(gameObject, teamID); // fallback
    }// Representative: if (SciFiTeamStyler.Instance != null)

```

### Comments & Documentation
- // / <summary>
- // / TeamVisual
- // /
- // / PURPOSE:
- // / - Applies team-based visuals (colors, materials, decals, etc.)
- // /   to units, civilians, and buildings.
- // / - Delegates styling to SciFiTeamStyler if present, otherwise
- // /   falls back to simple color tinting via TeamColorManager.
- // /
- // / DEPENDENCIES:
- /// <summary>
- /// TeamVisual
- /// PURPOSE:
- /// - Applies team-based visuals (colors, materials, decals, etc.)
- /// to units, civilians, and buildings.
- /// - Delegates styling to SciFiTeamStyler if present, otherwise
- /// falls back to simple color tinting via TeamColorManager.
- /// DEPENDENCIES:
- /// - SciFiTeamStyler (optional):
- /// * Centralized styling system for meshes, materials, emissives,

### Keywords
accordingly, adapter, add, after, and, another, any, Applies, Apply, applyOnStart, ApplyTeamColor, ARCHITECTURE, assigned, away, back, based, Building, buildings, call, can, Centralized, Civilian, civilians, color, colors, components, decals, Delegates, deletes, deletion, DEPENDENCIES, else, emissives, etc, expose, extend, fallback, falls, for, from, FUTURE, gameObject, GameObjects, gameplay, global, here, inject, Instance, int, keep, kind, kinds, logic, MAINTENANCE, manually, material, materials, may, meshes, mirrored, MonoBehaviour, move, Must, needs, NEVER, new, NOTES, null, obj, objects, once, optional, otherwise, own, pooled, prefab, prefabs, present, Purely, PURPOSE, put, Renderer, Representative, resource, Runs, Safe, SciFiTeamStyler, script, set, should, simple, Start, StructureAddon, styling, summary, system, team, TeamColorManager, teamID, teams, TeamVisual, that, the, These, thin, This, tinting, true, typically, Unit, units, update, useful, using, usually, Vehicle, via, visual, VisualKind, visuals, void, want, you

### Cross References
**Uses:** Building, Civilian, SciFiTeamStyler, Team, TeamColorManager, Unit
**Used By:** BuildGridManager, Building, BuildPlacementManager, ConstructionSite, Dependencies, SixTeamBootstrap, TeamBootstrap, TeamColorManager


## Assets/Scripts/Runtime/Units/Unit.cs
**Script:** Unit
**System Classification:** Units
**Base Class:** -
**Interfaces:** -

### Summary
Unit appears to be a units script. It appears to be related to: agent, return, isStopped, false, SetDestination, Representative, IsAlive, CurrentHealth.

### Public Methods (excerpts)
```csharp
{
        if (agent == null) return;
        agent.isStopped = false;
        agent.SetDestination(pos);
    }// Representative: if (agent == null) return;

{
        if (!IsAlive) return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
// Representative: if (!IsAlive) return;

```

### Comments & Documentation
- // / <summary>
- // / Base class for all units in Cube Wars.
- // /
- // / DEPENDENCIES:
- // / - NavMeshAgent:
- // /     Movement and pathfinding.
- // / - UnitCombatController:
- // /     Handles combat behaviour.
- // / - AIMilitary:
- // /     Issues MoveTo commands.
- /// <summary>
- /// Base class for all units in Cube Wars.
- /// DEPENDENCIES:
- /// - NavMeshAgent:
- /// Movement and pathfinding.
- /// - UnitCombatController:
- /// Handles combat behaviour.
- /// - AIMilitary:
- /// Issues MoveTo commands.
- /// - IHasHealth / IAttackable:

### Keywords
agent, AIMilitary, all, amount, and, Base, behaviour, class, combat, commands, Cube, CurrentHealth, damage, death, delete, DEPENDENCIES, destroys, Die, Does, false, for, Handles, Health, IAttackable, IHasHealth, IMPORTANT, info, IsAlive, isStopped, Issues, Movement, MoveTo, NavMeshAgent, NOT, null, Only, pathfinding, pos, Representative, RESPONSIBILITIES, return, SetDestination, summary, systems, targeting, teams, THIS, Unit, UnitCombatController, units, Used, Wars

### Cross References
**Uses:** AIMilitary, IAttackable, IHasHealth, Team, UnitCombatController
**Used By:** AIMilitary, Barracks, BuildPlacementManager, CombatSystem, ConstructionSite, DefenseTurret, Dependencies, Minimap, NeonRing, ResourceStorageContainer, SelectionRing, SixTeamBootstrap, Spawner, Team, TeamBootstrap, TeamStorageManager, TeamVisual, UnitInspectorUI, UnitProductionDefinition


# Systems

## Assets/Scripts/Systems/AI/DecisionMaking/AIBuilder.cs
**Script:** AIBuilder
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIBuilder appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIBuilder, MonoBehaviour, buildTimer, return, BuildItemDefinition, choice, ChooseBuilding, Representative.

### Public Methods (excerpts)
```csharp
{
        if (buildTimer > 0f)
            return;

        BuildItemDefinition choice = ChooseBuilding();
        if (choice == null)
            return;

// Representative: if (buildTimer > 0f)

```

### Comments & Documentation
- // DEPENDENCIES:
- // - Requires BuildPlacementManager.PlaceBuild(...) and TeamResources.CanAfford/Spend.

### Keywords
AIBuilder, and, BuildItemDefinition, BuildPlacementManager, buildTimer, CanAfford, choice, ChooseBuilding, DEPENDENCIES, MonoBehaviour, null, PlaceBuild, Representative, Requires, return, Spend, TeamResources

### Cross References
**Uses:** AIBuildingPriority, AIPersonality, BuildItemDefinition, BuildPlacementManager, ResourceStorageContainer, TeamResources
**Used By:** AIPlayer, Barracks, BuildingsDatabase, BuildCellReservation, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, BuildTimeSettings, Dependencies, TeamBootstrap


## Assets/Scripts/Systems/AI/DecisionMaking/AIBuildingPriority.cs
**Script:** AIBuildingPriority
**System Classification:** Buildings
**Base Class:** -
**Interfaces:** -

### Summary
AIBuildingPriority appears to be a buildings script. It appears to be related to: AIBuildingPriority.

### Keywords
AIBuildingPriority

### Cross References
**Uses:** -
**Used By:** AIBuilder, BuildItemDefinition, Dependencies


## Assets/Scripts/Systems/AI/DecisionMaking/AIEconomy.cs
**Script:** AIEconomy
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIEconomy appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIEconomy, MonoBehaviour, barracks, FindBarracks, return, buildTimer, deltaTime, Representative.

### Public Methods (excerpts)
```csharp
{
        if (barracks == null)
        {
            FindBarracks();
            return;
        }

        buildTimer -= Time.deltaTime;
// Representative: if (barracks == null)

```

### Comments & Documentation
- // =============================================================
- // AIEconomy.cs
- // DEPENDENCIES:
- // - Barracks: uses QueueCount, EnqueueUnit(), buildTime proxy
- // - AIPersonality: affects delay between unit queues
- // NOTES FOR FUTURE MAINTENANCE:
- // - If Barracks changes how units are produced, keep this in sync.
- // - For more advanced AI, replace EnqueueUnit() with explicit unit selection.
- // =============================================================

### Keywords
advanced, affects, AIEconomy, AIPersonality, are, barracks, between, buildTime, buildTimer, changes, delay, deltaTime, DEPENDENCIES, EnqueueUnit, explicit, FindBarracks, FOR, FUTURE, how, keep, MAINTENANCE, MonoBehaviour, more, NOTES, null, produced, proxy, QueueCount, queues, replace, Representative, return, selection, sync, this, Time, unit, units, uses, with

### Cross References
**Uses:** AIPersonality, Barracks
**Used By:** AIPlayer, Barracks, Dependencies, TeamBootstrap


## Assets/Scripts/Systems/AI/DecisionMaking/AIMilitary.cs
**Script:** AIMilitary
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIMilitary appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIMilitary, MonoBehaviour, units, GameObject, FindObjectsOfType, foreach, teamID, continue.

### Public Methods (excerpts)
```csharp
{
        Unit[] units = GameObject.FindObjectsOfType<Unit>();

        foreach (var u in units)
        {
            if (u.teamID != teamID) continue;
            if (!u.combatEnabled) continue;

            u.MoveTo(pos);
            return; // send ONE unit
        }
// Representative: Unit[] units = GameObject.FindObjectsOfType<Unit>();

{
        UnitCombatController[] units = GameObject.FindObjectsOfType<UnitCombatController>();
        int myUnits = 0;

        foreach (var u in units)
            if (u.teamID == teamID)
                myUnits++;

// Representative: UnitCombatController[] units = GameObject.FindObjectsOfType<UnitCombatController>();

```

### Comments & Documentation
- // -----------------------------
- // DEPENDENCIES:
- // - Unit: must implement MoveTo(Vector3)
- // - UnitCombatController: used for combat targeting
- // - DiplomacyManager: used for war checks
- // - Attackable: enemy target interface
- // -----------------------------
- // send ONE unit
- // Personality bias

### Keywords
AIMilitary, Attackable, bias, checks, combat, combatEnabled, continue, DEPENDENCIES, DiplomacyManager, enemy, FindObjectsOfType, for, foreach, GameObject, implement, int, interface, MonoBehaviour, MoveTo, must, myUnits, ONE, Personality, pos, Representative, return, send, target, targeting, teamID, Unit, UnitCombatController, units, used, var, Vector3, war

### Cross References
**Uses:** AIPersonality, Attackable, DiplomacyManager, Unit, UnitCombatController
**Used By:** AIPlayer, AIResourceManager, Attackable, CombatSystem, Dependencies, DiplomacyManager, TeamBootstrap, Unit, UnitCombatController


## Assets/Scripts/Systems/AI/DecisionMaking/AIPlayer.cs
**Script:** AIPlayer
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIPlayer appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIPlayer, MonoBehaviour, summary, controller, single, DEPENDENCIES, AIEconomy, AIMilitary.

### Comments & Documentation
- // / <summary>
- // / Core AI controller for a single AI team.
- // /
- // / DEPENDENCIES:
- // / - AIEconomy
- // / - AIMilitary
- // / - AIBuilder
- // / - AIResourceManager
- // / - AIThreatDetector (optional)
- // / - AIRepairManager (optional)
- /// <summary>
- /// Core AI controller for a single AI team.
- /// DEPENDENCIES:
- /// - AIEconomy
- /// - AIMilitary
- /// - AIBuilder
- /// - AIResourceManager
- /// - AIThreatDetector (optional)
- /// - AIRepairManager (optional)
- /// - AIRebuildManager (optional)

### Keywords
AIBuilder, AIEconomy, AIMilitary, AIPlayer, AIRebuildManager, AIRepairManager, AIResourceManager, AIThreatDetector, all, and, Apply, attached, Cache, controller, Core, create, DEPENDENCIES, destroy, difficulty, does, for, Forward, IMPORTANT, logic, MonoBehaviour, NOT, only, optional, personality, references, RESPONSIBILITIES, runs, script, settings, single, subsystem, subsystems, summary, team, teamID, teams, the, This, Tick

### Cross References
**Uses:** AIBuilder, AIDifficulty, AIEconomy, AIMilitary, AIPersonality, AIRebuildManager, AIRepairManager, AIResourceManager, AIThreatDetector, Team
**Used By:** Dependencies, TeamBootstrap


## Assets/Scripts/Systems/AI/DecisionMaking/MultiTeamAIDirector.cs
**Script:** MultiTeamAIDirector
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
MultiTeamAIDirector appears to be a ai script derived from MonoBehaviour. It appears to be related to: MultiTeamAIDirector, MonoBehaviour, summary, level, director, multiple, teams, DEPENDENCIES.

### Comments & Documentation
- // / <summary>
- // / High-level AI director for multiple AI teams.
- // /
- // / DEPENDENCIES:
- // / - BuildGridCell
- // / - BuildPlacementManager
- // / - BuildItemDefinition
- // / - BuildItemInstance
- // / - ConstructionSite
- // / - TeamResources
- /// <summary>
- /// High-level AI director for multiple AI teams.
- /// DEPENDENCIES:
- /// - BuildGridCell
- /// - BuildPlacementManager
- /// - BuildItemDefinition
- /// - BuildItemInstance
- /// - ConstructionSite
- /// - TeamResources
- /// - Headquarters

### Keywords
aggression, and, based, build, BuildGridCell, buildings, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, Compute, construction, ConstructionSite, delete, DEPENDENCIES, director, does, for, Headquarters, High, IMPORTANT, level, MonoBehaviour, multiple, MultiTeamAIDirector, NOT, only, per, Place, positions, reads, Reserve, RESPONSIBILITIES, rest, script, sites, strategy, summary, team, teamID, TeamResources, teams, This, Track, unchanged, walkways, your

### Cross References
**Uses:** BuildGridCell, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, ConstructionSite, Headquarters, TeamResources
**Used By:** Dependencies


## Assets/Scripts/Systems/AI/DecisionMaking/TeamAIBuild.cs
**Script:** TeamAIBuild
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamAIBuild appears to be a ai script derived from MonoBehaviour. It appears to be related to: TeamAIBuild, MonoBehaviour, summary, Handles, building, placement, single, DEPENDENCIES.

### Comments & Documentation
- // / <summary>
- // / Handles AI building placement for a single team.
- // /
- // / DEPENDENCIES:
- // / - BuildGridCell
- // / - BuildPlacementManager
- // / - BuildingsDatabase
- // / - TeamResources
- // /
- // / RESPONSIBILITIES:
- /// <summary>
- /// Handles AI building placement for a single team.
- /// DEPENDENCIES:
- /// - BuildGridCell
- /// - BuildPlacementManager
- /// - BuildingsDatabase
- /// - TeamResources
- /// RESPONSIBILITIES:
- /// - Periodically attempt to place buildings
- /// - Choose affordable items

### Keywords
affordable, attempt, BuildingsDatabase, BuildGridCell, building, buildings, BuildPlacementManager, category, Choose, delete, DEPENDENCIES, Does, for, Handles, IMPORTANT, items, modify, MonoBehaviour, NOT, objects, Periodically, place, placement, priorities, Respect, RESPONSIBILITIES, single, summary, team, TeamAIBuild, TeamResources, teams

### Cross References
**Uses:** BuildingsDatabase, BuildGridCell, BuildItemDefinition, BuildPlacementManager, Team, TeamResources
**Used By:** BuildingsDatabase, BuildItemDefinition, Dependencies


## Assets/Scripts/Systems/AI/Jobs/AIRebuildManager.cs
**Script:** AIRebuildManager
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIRebuildManager appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIRebuildManager, MonoBehaviour, lostTeam, teamID, return, rebuildQueue, Enqueue, Representative.

### Public Methods (excerpts)
```csharp
{
        if (lostTeam != teamID)
            return;

        rebuildQueue.Enqueue((item, pos));
    }// Representative: if (lostTeam != teamID)

{
        if (rebuildQueue.Count == 0)
            return;

        var (item, pos) = rebuildQueue.Peek();

        // Check resources
        if (!TeamResources.Instance.CanAfford(teamID, item.costs))
// Representative: if (rebuildQueue.Count == 0)

```

### Comments & Documentation
- // Stores (building definition, rebuild position)
- // / <summary>
- // / Call this from your building destruction logic.
- // / Example:
- // / FindObjectOfType<AIRebuildManager>()?.NotifyBuildingDestroyed(teamID, item, position);
- // / </summary>
- // Check resources
- // Check placement
- // Rebuild
- /// <summary>
- /// Call this from your building destruction logic.
- /// Example:
- /// FindObjectOfType<AIRebuildManager>()?.NotifyBuildingDestroyed(teamID, item, position);
- /// </summary>

### Keywords
AIRebuildManager, building, Call, CanAfford, Check, costs, Count, definition, destruction, Enqueue, Example, FindObjectOfType, from, Instance, item, logic, lostTeam, MonoBehaviour, NotifyBuildingDestroyed, Peek, placement, pos, position, rebuild, rebuildQueue, Representative, resources, return, Stores, summary, teamID, TeamResources, this, var, your

### Cross References
**Uses:** BuildItemDefinition, BuildPlacementManager, TeamResources
**Used By:** AIPlayer, BuildCellReservation, BuildItemDefinition, BuildItemInstance, BuildPlacementManager, BuildTimeSettings, Dependencies


## Assets/Scripts/Systems/AI/Jobs/AIRepairManager.cs
**Script:** AIRepairManager
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIRepairManager appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIRepairManager, MonoBehaviour, timer, deltaTime, return, repairInterval, RepairNearbyBuildings, Representative.

### Public Methods (excerpts)
```csharp
{
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = repairInterval;

        RepairNearbyBuildings();
    }// Representative: timer -= Time.deltaTime;

```

### Comments & Documentation
- // repair ONE per tick

### Keywords
AIRepairManager, deltaTime, MonoBehaviour, ONE, per, repair, repairInterval, RepairNearbyBuildings, Representative, return, tick, Time, timer

### Cross References
**Uses:** Attackable
**Used By:** AIPlayer, Dependencies


## Assets/Scripts/Systems/AI/Jobs/AIResourceManager.cs
**Script:** AIResourceManager
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIResourceManager appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIResourceManager, MonoBehaviour, ClaimNearbyNodes, DefendNodes, Representative, Internal, nodes, claimed.

### Public Methods (excerpts)
```csharp
{
        ClaimNearbyNodes();
        DefendNodes();
    }// Representative: ClaimNearbyNodes();

```

### Comments & Documentation
- // Internal list of nodes this AI has claimed
- // Called by your AI tick system
- // ---------------------------------------------------------
- // CLAIM NODES
- // ---------------------------------------------------------
- // ---------------------------------------------------------
- // DEFEND CLAIMED NODES
- // ---------------------------------------------------------

### Keywords
AIResourceManager, Called, CLAIM, claimed, ClaimNearbyNodes, DEFEND, DefendNodes, has, Internal, list, MonoBehaviour, nodes, Representative, system, this, tick, your

### Cross References
**Uses:** AIMilitary, AIThreatDetector, Attackable, ResourceNode, Team
**Used By:** AIPlayer, Dependencies, TeamBootstrap


## Assets/Scripts/Systems/AI/Targeting/AIThreatDetector.cs
**Script:** AIThreatDetector
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIThreatDetector appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIThreatDetector, MonoBehaviour, Attackable, GameObject, FindObjectsOfType, foreach, IsAlive, continue.

### Public Methods (excerpts)
```csharp
{
        Attackable[] all = GameObject.FindObjectsOfType<Attackable>();

        foreach (var a in all)
        {
            if (!a.IsAlive) continue;
            if (a.teamID == teamID) continue;
            if (!DiplomacyManager.Instance.AreAtWar(teamID, a.teamID)) continue;

            if (Vector3.Distance(pos, a.transform.position) <= threatRadius)
                return a;
        }
// Representative: Attackable[] all = GameObject.FindObjectsOfType<Attackable>();

```

### Keywords
AIThreatDetector, all, AreAtWar, Attackable, continue, DiplomacyManager, Distance, FindObjectsOfType, foreach, GameObject, Instance, IsAlive, MonoBehaviour, pos, position, Representative, return, teamID, threatRadius, transform, var, Vector3

### Cross References
**Uses:** Attackable, DiplomacyManager
**Used By:** AIPlayer, AIResourceManager, Dependencies, TeamBootstrap


## Assets/Scripts/Systems/Buildings/BuildCellReservation.cs
**Script:** BuildCellReservation
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
BuildCellReservation appears to be a buildings script derived from MonoBehaviour. It appears to be related to: BuildCellReservation, MonoBehaviour, PURPOSE, Marks, BuildGridCell, special, placement, restrictions.

### Comments & Documentation
- // ============================================================================
- // BuildCellReservation.cs
- // PURPOSE:
- // - Marks a BuildGridCell with special placement restrictions.
- // - Used to prevent buildings from being placed in specific areas such as:
- // * Walkways
- // * Defensive borders
- // * Reserved terrain
- // DEPENDENCIES:
- // - BuildGridCell:

### Keywords
add, AIBuilder, AIRebuildManager, areas, assigns, attached, auto, based, being, blockBuildingPlacement, Blocks, borders, BuildCellReservation, BuildGridCell, BuildGridManager, building, buildings, BuildPlacementManager, but, cell, cells, check, Checks, component, create, Defensive, DEPENDENCIES, depending, designers, directly, Does, during, ensure, extend, extended, flexible, footprints, FOR, from, FUTURE, GameObject, generation, grid, here, industrial, INSPECTOR, integrate, larger, logic, MAINTENANCE, map, Marks, may, metadata, military, MonoBehaviour, more, multiple, must, new, NOT, NOTES, optional, placed, placement, preconfigured, prevent, PURPOSE, reject, REQUIREMENTS, reservation, reservations, Reserved, reservedForBorderDefense, ReservedForFarm, ReservedForRoad, reservedForWalkway, residential, respect, respectCellReservations, restrictions, same, script, Should, special, specific, such, system, tagging, terrain, than, the, This, true, types, use, Used, using, Walkways, when, will, with, you, zoning

### Cross References
**Uses:** AIBuilder, AIRebuildManager, BuildGridCell, BuildGridManager, BuildPlacementManager
**Used By:** BuildGridCell, BuildGridManager, BuildPlacementManager, Dependencies


## Assets/Scripts/Systems/Buildings/BuildPlacementManager.cs
**Script:** BuildPlacementManager
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
BuildPlacementManager appears to be a buildings script derived from MonoBehaviour. It appears to be related to: BuildPlacementManager, MonoBehaviour, selectedItem, ClearPreview, Debug, Selected, displayName, Representative.

### Public Methods (excerpts)
```csharp
{
        selectedItem = item;
        ClearPreview();
        Debug.Log($"[BuildPlacementManager] Selected: {(item != null ? item.displayName : "None")}");
    }// Representative: selectedItem = item;

{
        if (selectedItem == null) return false;
        return TryPlace(cell, selectedItem);
    }// Representative: if (selectedItem == null) return false;

{
        return !Physics.CheckSphere(pos, 1.5f);
    }// Representative: return !Physics.CheckSphere(pos, 1.5f);

{
        if (cell == null || item == null) return false;
        if (cell.isOccupied) return false;

        if (respectCellReservations)
        {
            var res = cell.GetComponent<BuildCellReservation>();
            if (res != null && res.blockBuildingPlacement)
                return false;
        }
// Representative: if (cell == null || item == null) return false;

{
        if (item == null || item.prefab == null)
            return false;

        if (!CanPlaceAt(pos))
            return false;

        // Cost check + spend
// Representative: if (item == null || item.prefab == null)

{
        if (!showPreview) return;
        if (cell == null) return;
        if (selectedItem == null) return;
        if (previewMaterial == null) return;

        if (respectCellReservations)
        {
            var res = cell.GetComponent<BuildCellReservation>();
            if (res != null && res.blockBuildingPlacement)
            {
                ClearPreview();
                return;
            }
        }
// Representative: if (!showPreview) return;

{
        if (previewObj != null)
            previewObj.SetActive(false);
    }// Representative: if (previewObj != null)

```

### Comments & Documentation
- // =============================================================
- // BuildPlacementManager.cs
- // PURPOSE:
- // - Handles ALL building placement logic (player + AI).
- // - Supports grid-based and world-space placement.
- // - Creates ConstructionSite objects or instantly places buildings.
- // DEPENDENCIES:
- // - BuildItemDefinition:
- // * Prefab, costs, offsets.
- // - BuildGridCell:

### Keywords
adaptation, Add, AIBuild, AIBuilder, AIRebuildManager, ALL, Always, and, applied, Applies, based, blockBuildingPlacement, Blocks, blueprint, BuildCellReservation, BuildGridCell, building, buildings, BuildItemDefinition, BuildPlacementManager, builds, cancellation, CanPlaceAt, cell, check, checks, CheckSphere, Civilian, ClearPreview, collision, colors, component, components, ConstructionSite, Cost, costs, Created, Creates, Debug, delete, deletes, DEPENDENCIES, deposited, displayName, does, duplicate, enforce, exists, false, footprints, for, from, FUTURE, GameObjects, GetComponent, grid, Handles, have, height, immediately, implement, IMPORTANT, Init, Instant, instantly, isOccupied, item, Log, logic, MAINTENANCE, manager, MaterialPropertyBlock, MonoBehaviour, multi, MUST, needed, None, NOT, NOTES, null, objects, offsets, ONLY, Optional, performance, Physics, placed, placement, places, player, pos, prefab, preview, previewMaterial, previewObj, PURPOSE, Representative, res, reserved, reserveResources, resource, ResourceDropoff, ResourceStorageProvider, respectCellReservations, return, rotation, script, Selected, selectedItem, SetActive, showPreview, space, spend, spending, storage, support, Supports, team, teamID, TeamResources, teams, TeamStorageManager, TeamVisual, terrain, This, tracking, true, TryPlace, Unit, useConstructionSites, Used, uses, var, when, world, yOffset

### Cross References
**Uses:** AIBuilder, AIRebuildManager, BuildCellReservation, BuildGridCell, Building, BuildItemDefinition, BuildItemInstance, Civilian, ConstructionSite, ResourceDropoff, ResourceStorageContainer, ResourceStorageProvider, Team, TeamResources, TeamStorageManager, TeamVisual, Unit
**Used By:** AIBuilder, AIRebuildManager, BuildCellReservation, BuildGridCell, BuildGridManager, Building, BuildItemDefinition, BuildItemInstance, BuildMenuUI, ConstructionSite, Dependencies, GameBootstrap, MultiTeamAIDirector, TeamAIBuild


## Assets/Scripts/Systems/Civilians/JobManager.cs
**Script:** JobManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
JobManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: JobManager, MonoBehaviour, return, civilians, Contains, Representative, Remove, HasJob.

### Public Methods (excerpts)
```csharp
{
        if (civ == null) return;
        if (!civilians.Contains(civ))
            civilians.Add(civ);
    }// Representative: if (civ == null) return;

{
        if (civ == null) return;
        civilians.Remove(civ);
    }// Representative: if (civ == null) return;

{
        return civ != null && civ.HasJob;
    }// Representative: return civ != null && civ.HasJob;

{
        int count = 0;
        for (int i = 0; i < civilians.Count; i++)
        {
            var civ = civilians[i];
            if (civ == null) continue;
            if (civ.CurrentAssignedSite == site)
                count++;
        }
// Representative: int count = 0;

{
        int count = 0;
        var sites = GameObject.FindObjectsOfType<ConstructionSite>();
        for (int i = 0; i < sites.Length; i++)
        {
            var s = sites[i];
            if (s == null) continue;
            if (s.teamID != teamID) continue;
            if (s.IsComplete) continue;
            count++;
        }
// Representative: int count = 0;

```

### Comments & Documentation
- // =============================================================
- // JobManager.cs
- // DEPENDENCIES:
- // - Civilian: registered/unregistered here, roles tracked
- // - CivilianRole: enum of roles (Gatherer, Builder, Hauler, Idle)
- // - ConstructionSite: used for CountBuildersOnSite + GetActiveConstructionSiteCount
- // - TaskBoardUI: calls GetRoleCounts() and GetActiveConstructionSiteCount()
- // NOTES FOR FUTURE MAINTENANCE:
- // - If you add new roles, CivilianRole and any UI must be updated.
- // - Keep civilians list in sync with Civilian.OnEnable/OnDisable.

### Keywords
Add, and, any, Builder, calls, civ, Civilian, CivilianRole, civilians, ConstructionSite, Contains, continue, count, CountBuildersOnSite, CurrentAssignedSite, DEPENDENCIES, enum, FindObjectsOfType, for, FUTURE, GameObject, Gatherer, GetActiveConstructionSiteCount, GetRoleCounts, HasJob, Hauler, here, Idle, int, IsComplete, JobManager, Keep, Length, list, MAINTENANCE, MonoBehaviour, must, new, NOTES, null, OnDisable, OnEnable, registered, Remove, Representative, return, roles, site, sites, sync, TaskBoardUI, teamID, tracked, unregistered, updated, used, var, with, you

### Cross References
**Uses:** Civilian, CivilianRole, ConstructionSite, TaskBoardUI
**Used By:** BuildTimeSettings, Civilian, CivilianRole, ConstructionSite, Dependencies, GameBootstrap, TaskBoardUI


## Assets/Scripts/Systems/Combat/IAttackable.cs
**Script:** IAttackable
**System Classification:** Combat
**Base Class:** -
**Interfaces:** -

### Summary
IAttackable appears to be a combat script. It appears to be related to: IAttackable, TeamID, IsAlive, Transform, AimPoint.

### Properties
```csharp
int TeamID { get;
bool IsAlive { get;
Transform AimPoint { get;
```

### Keywords
AimPoint, bool, get, IAttackable, int, IsAlive, TeamID, Transform

### Cross References
**Uses:** -
**Used By:** Building, ConstructionSite, Dependencies, Unit


## Assets/Scripts/Systems/Combat/ICommandable.cs
**Script:** ICommandable
**System Classification:** Combat
**Base Class:** -
**Interfaces:** -

### Summary
ICommandable appears to be a combat script. It appears to be related to: ICommandable.

### Keywords
ICommandable

### Cross References
**Uses:** -
**Used By:** Dependencies, UnitCommandController


## Assets/Scripts/Systems/Combat/IHasHealth.cs
**Script:** IHasHealth
**System Classification:** Combat
**Base Class:** -
**Interfaces:** -

### Summary
IHasHealth appears to be a combat script. It appears to be related to: IHasHealth, float, CurrentHealth, MaxHealth.

### Properties
```csharp
float CurrentHealth { get;
float MaxHealth { get;
```

### Keywords
CurrentHealth, float, get, IHasHealth, MaxHealth

### Cross References
**Uses:** -
**Used By:** Attackable, Building, Civilian, ConstructionSite, DefenseTurret, Dependencies, Unit, UnitInspectorUI, WorldHealthBar


## Assets/Scripts/Systems/Combat/ITargetable.cs
**Script:** ITargetable
**System Classification:** Combat
**Base Class:** -
**Interfaces:** -

### Summary
ITargetable appears to be a combat script. It appears to be related to: ITargetable, TeamID, IsAlive.

### Properties
```csharp
int TeamID { get;
bool IsAlive { get;
```

### Keywords
bool, get, int, IsAlive, ITargetable, TeamID

### Cross References
**Uses:** -
**Used By:** Building, Civilian, DefenseTurret, Dependencies


## Assets/Scripts/Systems/Combat/ProjectilePool.cs
**Script:** ProjectilePool
**System Classification:** Combat
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ProjectilePool appears to be a combat script derived from MonoBehaviour. It appears to be related to: ProjectilePool, MonoBehaviour, pools, TryGetValue, prefab, stack, Projectile, Count.

### Public Methods (excerpts)
```csharp
{
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<Projectile>();
            pools[prefab] = stack;
        }

        Projectile obj = (stack.Count > 0) ? stack.Pop() : Instantiate(prefab);
// Representative: if (!pools.TryGetValue(prefab, out var stack))

{
        if (obj == null) return;

        obj.gameObject.SetActive(false);

        if (obj.prefabKey != null)
        {
            if (!pools.TryGetValue(obj.prefabKey, out var stack))
            {
                stack = new Stack<Projectile>();
                pools[obj.prefabKey] = stack;
            }
            stack.Push(obj);
        }
// Representative: if (obj == null) return;

```

### Comments & Documentation
- // =============================================================
- // ProjectilePool.cs
- // PURPOSE:
- // - Unified projectile pooling for all weapons.
- // - Replaces TurretProjectilePool.
- // DEPENDENCIES:
- // - Projectile:
- // * Must have a prefabKey reference for pooling.
- // NOTES FOR FUTURE MAINTENANCE:
- // - Supports multiple projectile types.

### Keywords
add, all, can, Count, DEPENDENCIES, explosive, false, for, FUTURE, gameObject, have, Instantiate, MAINTENANCE, MonoBehaviour, multiple, Must, new, NOTES, null, obj, out, pool, pooling, pools, Pop, prefab, prefabKey, Projectile, ProjectilePool, projectiles, PURPOSE, Push, reference, Replaces, Representative, return, SetActive, stack, still, Supports, they, this, TryGetValue, TurretProjectilePool, types, Unified, use, var, weapons, you

### Cross References
**Uses:** Projectile, TurretProjectilePool
**Used By:** Attackable, CombatSystem, Dependencies, GameBootstrap, Projectile, WeaponComponent


## Assets/Scripts/Systems/Combat/TurretProjectilePool.cs
**Script:** TurretProjectilePool
**System Classification:** Buildings
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TurretProjectilePool appears to be a buildings script derived from MonoBehaviour. It appears to be related to: TurretProjectilePool, MonoBehaviour, pools, TryGetValue, prefab, stack, TurretProjectile, Count. It may share responsibilities with other defensive buildings such as Turret or DefenseTurret.

### Public Methods (excerpts)
```csharp
{
        if (!pools.TryGetValue(prefab, out var stack))
        {
            stack = new Stack<TurretProjectile>();
            pools[prefab] = stack;
        }

        TurretProjectile obj = (stack.Count > 0) ? stack.Pop() : Instantiate(prefab);
// Representative: if (!pools.TryGetValue(prefab, out var stack))

{
        if (obj == null) return;

        obj.gameObject.SetActive(false);

        if (obj.prefabKey != null)
        {
            if (!pools.TryGetValue(obj.prefabKey, out var stack))
            {
                stack = new Stack<TurretProjectile>();
                pools[obj.prefabKey] = stack;
            }
            stack.Push(obj);
        }
// Representative: if (obj == null) return;

```

### Comments & Documentation
- // =============================================================
- // TurretProjectilePool.cs
- // DEPENDENCIES:
- // - TurretProjectile:
- // * Projectiles spawned and despawned by this pool.
- // - Turret / any weapon system using TurretProjectile:
- // * Should call Spawn() and Despawn() instead of Instantiate/Destroy.
- // NOTES FOR FUTURE MAINTENANCE:
- // - This is a generic pool keyed by prefab.
- // - If you add different turret projectile types, they can all share this pool.

### Keywords
add, all, and, any, call, can, Count, DEPENDENCIES, Despawn, despawned, Destroy, different, exactly, false, FOR, FUTURE, gameObject, generic, Instantiate, instead, keyed, MAINTENANCE, Make, MonoBehaviour, new, NOTES, null, obj, one, out, pool, pools, Pop, prefab, prefabKey, projectile, Projectiles, Push, Representative, return, scene, SetActive, share, Should, Spawn, spawned, stack, sure, system, the, there, they, this, TryGetValue, Turret, TurretProjectile, TurretProjectilePool, types, using, var, weapon, you

### Cross References
**Uses:** Turret, TurretProjectile
**Used By:** DefenseTurret, Dependencies, ProjectilePool, TurretProjectile


## Assets/Scripts/Systems/Combat/WeaponComponent.cs
**Script:** WeaponComponent
**System Classification:** Combat
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
WeaponComponent appears to be a combat script derived from MonoBehaviour. It appears to be related to: WeaponComponent, MonoBehaviour, CanFire, target, IsAlive, return, fireTimer, fireCooldown.

### Public Methods (excerpts)
```csharp
{
        if (!CanFire || target == null || !target.IsAlive)
            return;

        fireTimer = fireCooldown;

        if (muzzleFlashFX != null)
            Instantiate(muzzleFlashFX, muzzle.position, muzzle.rotation);
// Representative: if (!CanFire || target == null || !target.IsAlive)

```

### Comments & Documentation
- // =============================================================
- // WeaponComponent.cs
- // PURPOSE:
- // - Shared firing logic for Units, Turrets, and any future ranged entity.
- // - Handles projectile pooling, muzzle FX, fire cooldowns.
- // DEPENDENCIES:
- // - ProjectilePool:
- // * Spawns/despawns projectiles.
- // - Projectile:
- // * Unified projectile logic.

### Keywords
add, ammo, and, any, AoE, application, Attackable, beam, before, burst, Calls, CanFire, checks, class, cooldowns, damage, DEPENDENCIES, despawns, different, entity, extend, fire, FireAtTarget, fireCooldown, fireTimer, firing, for, future, Handles, here, Instantiate, interface, IsAlive, logic, MAINTENANCE, MonoBehaviour, muzzle, muzzleFlashFX, NOTES, null, overheating, pooling, position, projectile, ProjectilePool, projectiles, PURPOSE, ranged, ready, Representative, return, rotation, Shared, shoot, sound, Spawns, target, them, this, trigger, Turrets, types, Unified, UnitCombatController, Units, weapon, WeaponComponent, when, you

### Cross References
**Uses:** Attackable, Projectile, ProjectilePool, UnitCombatController
**Used By:** Attackable, CombatSystem, Dependencies, Projectile, Turret, UnitCombatController


## Assets/Scripts/Systems/Economy/CraftingSystem.cs
**Script:** CraftingSystem
**System Classification:** Unclassified
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
CraftingSystem appears to be a script derived from MonoBehaviour. It appears to be related to: CraftingSystem, MonoBehaviour, amount, return, false, TeamResources, Instance, TeamInventory.

### Public Methods (excerpts)
```csharp
{
        if (item == null || amount <= 0) return false;
        if (TeamResources.Instance == null || TeamInventory.Instance == null) return false;

        // Check resource availability
        if (item.craftCost != null)
        {
            for (int i = 0; i < item.craftCost.Length; i++)
            {
                int need = item.craftCost[i].amount * amount;

                // NEW: use TeamStorageManager for stored resources
                int stored = TeamStorageManager.Instance.GetTotalStored(teamID, item.craftCost[i].type);
                if (stored < need)
                    return false;
            }

            // Spend resources
            for (int i = 0; i < item.craftCost.Length; i++)
            {
                int need = item.craftCost[i].amount * amount;
                TeamResources.Instance.SpendResource(teamID, item.craftCost[i].type, need);
            }
        }
// Representative: if (item == null || amount <= 0) return false;

```

### Comments & Documentation
- // ---------------------------------------------------------
- // DEPENDENCIES:
- // - TeamResources: must implement SpendResource(teamID, type, amount)
- // - TeamStorageManager: used for GetTotalStored(teamID, type)
- // - TeamInventory: must implement AddTool(teamID, item, amount)
- // - ToolItem: must contain craftCost[]
- // ---------------------------------------------------------
- // Check resource availability
- // NEW: use TeamStorageManager for stored resources
- // Spend resources

### Keywords
AddTool, amount, availability, Check, contain, craftCost, CraftingSystem, DEPENDENCIES, false, for, GetTotalStored, implement, Instance, int, item, Length, MonoBehaviour, must, need, NEW, null, Representative, resource, resources, return, Spend, SpendResource, stored, teamID, TeamInventory, TeamResources, TeamStorageManager, ToolItem, type, use, used

### Cross References
**Uses:** TeamInventory, TeamResources, TeamStorageManager, ToolItem
**Used By:** Dependencies, WeaponsFactory


## Assets/Scripts/Systems/Economy/ResourceRegistry.cs
**Script:** ResourceRegistry
**System Classification:** Resources
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceRegistry appears to be a resources script derived from MonoBehaviour. It appears to be related to: ResourceRegistry, MonoBehaviour, return, nodesByType, TryGetValue, ResourceNode, Contains, Representative.

### Public Methods (excerpts)
```csharp
{
        if (node == null) return;
        if (!nodesByType.TryGetValue(node.type, out var list))
        {
            list = new List<ResourceNode>();
            nodesByType[node.type] = list;
        }
        if (!list.Contains(node)) list.Add(node);
// Representative: if (node == null) return;

{
        if (node == null) return;
        if (nodesByType.TryGetValue(node.type, out var list))
            list.Remove(node);
    }// Representative: if (node == null) return;

{
        if (!nodesByType.TryGetValue(type, out var list))
        {
            list = new List<ResourceNode>();
            nodesByType[type] = list;
        }
        return list;
    }// Representative: if (!nodesByType.TryGetValue(type, out var list))

```

### Comments & Documentation
- // Pre-create lists for all enum values so lookups are cheap

### Keywords
Add, all, are, cheap, Contains, create, enum, for, list, lists, lookups, MonoBehaviour, new, node, nodesByType, null, out, Pre, Remove, Representative, ResourceNode, ResourceRegistry, return, TryGetValue, type, values, var

### Cross References
**Uses:** ResourceNode, ResourceType
**Used By:** Dependencies


## Assets/Scripts/Systems/Grid/BuildGridCell.cs
**Script:** BuildGridCell
**System Classification:** UI
**Base Class:** -
**Interfaces:** -

### Summary
BuildGridCell appears to be a ui script. It appears to be related to: BuildGridCell, manager, teamID, gridCoord, coord, worldCenter, center, Representative.

### Public Methods (excerpts)
```csharp
{
        manager = mgr;
        teamID = t;
        gridCoord = coord;
        worldCenter = center;
    }// Representative: manager = mgr;

```

### Comments & Documentation
- // ============================================================================
- // BuildGridCell.cs
- // PURPOSE:
- // - Represents a single tile in the build grid.
- // - Stores team ownership, world position, occupancy state, and placed object.
- // - Handles click interaction for building placement.
- // DEPENDENCIES:
- // - BuildGridManager:
- // * Calls Init() to assign teamID, coordinates, and worldCenter.
- // * Calls OnCellClicked() when this cell is clicked.

### Keywords
add, adjusted, alignment, and, area, assign, assigned, attached, Attempts, build, BuildCellReservation, BuildGridCell, BuildGridManager, building, buildings, BuildMenuUI, BuildPlacementManager, Calls, cell, center, class, click, clicked, clicks, component, ConstructionSite, coord, coordinates, created, DEPENDENCIES, drag, dynamically, ensure, EventSystem, expansion, fog, footprint, for, FUTURE, GameObject, grid, gridCell, gridCoord, Handles, height, here, Init, INSPECTOR, integrated, interaction, logic, MAINTENANCE, manager, May, menu, metadata, mgr, multi, must, need, None, NOTES, object, occupancy, OnCellClicked, OnMouseDown, optional, ownership, place, placed, placedObject, placement, position, prevent, PURPOSE, Representative, Represents, REQUIREMENTS, respects, restrict, same, selection, should, single, state, Stores, support, Syncs, team, teamID, terrain, the, this, through, tile, Used, visibility, war, when, with, world, worldCenter, you

### Cross References
**Uses:** BuildCellReservation, BuildGridManager, Building, BuildMenuUI, BuildPlacementManager, ConstructionSite
**Used By:** BuildCellReservation, BuildGridManager, BuildPlacementManager, ConstructionSite, Dependencies, MultiTeamAIDirector, TeamAIBuild


## Assets/Scripts/Systems/Grid/BuildGridManager.cs
**Script:** BuildGridManager
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
BuildGridManager appears to be a ui script derived from MonoBehaviour. It appears to be related to: BuildGridManager, MonoBehaviour, Destroy, cells, allCells, Count, Clear, selectedCell.

### Public Methods (excerpts)
```csharp
{
        // Destroy old cells
        for (int i = 0; i < allCells.Count; i++)
            if (allCells[i] != null) Destroy(allCells[i]);

        allCells.Clear();
        selectedCell = null;

// Representative: for (int i = 0; i < allCells.Count; i++)

{
        if (!isVisible) return;
        if (cell == null) return;

        if (cell.teamID != playerTeamID) return;

        if (cell.isOccupied) return;

// Representative: if (!isVisible) return;

```

### Comments & Documentation
- // ============================================================================
- // BuildGridManager.cs
- // PURPOSE:
- // - Generates and manages the build grid around each Headquarters.
- // - Controls visibility of the grid for the player.
- // - Handles cell selection, highlighting, and click routing.
- // - Ensures non-player grids are hidden and unclickable.
- // - Acts as the central controller for grid-based building placement.
- // DEPENDENCIES:
- // - BuildGridCell:

### Keywords
Acts, add, adjust, allCells, and, are, around, Attached, attempt, based, between, blue, build, BuildCellReservation, BuildGridCell, BuildGridManager, building, buildings, BuildMenuUI, BuildPlacementManager, but, Called, cell, cellMaterial, cells, cellSize, central, checks, Clear, click, clicked, component, controller, Controls, Count, Created, default, DEPENDENCIES, Destroy, determine, directly, each, ensure, Ensures, EventSystem, exist, fog, footprint, for, frame, from, FUTURE, generated, Generates, generation, grid, grids, halfExtent, Handles, Headquarters, height, here, hidden, highlight, highlighting, HQs, indirectly, INSPECTOR, int, integrate, isOccupied, isVisible, logic, MAINTENANCE, manages, Mat_GridCell, Mat_GridCell_Selected, match, material, menu, MonoBehaviour, multi, must, non, Not, NOTES, null, occupancy, old, optional, orientation, ownership, placed, placedObject, placement, player, playerTeamID, positions, prevent, PURPOSE, radius, read, reflect, rely, Representative, REQUIREMENTS, reservation, respects, restrict, return, roads, rotation, routing, rules, selectedCell, selectedMaterial, selection, set, should, snapping, spacing, Stores, support, sync, Syncs, system, team, teamID, TeamVisual, terrain, the, this, through, tied, tile, tiles, unclickable, Used, variation, via, visibility, vision, wait, war, when, with, worldCenter, you, zones

### Cross References
**Uses:** BuildCellReservation, BuildGridCell, Building, BuildMenuUI, BuildPlacementManager, Headquarters, TeamVisual
**Used By:** BuildCellReservation, BuildGridCell, BuildMenuUI, Dependencies, GameBootstrap


## Assets/Scripts/Systems/UI/BuildMenuUI.cs
**Script:** BuildMenuUI
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
BuildMenuUI appears to be a ui script derived from MonoBehaviour. It appears to be related to: BuildMenuUI, MonoBehaviour, visible, BuildPlacementManager, Instance, SetSelected, Representative, autoDiscover.

### Public Methods (excerpts)
```csharp
{
        show = visible;

        if (!show)
        {
            if (BuildPlacementManager.Instance != null)
                BuildPlacementManager.Instance.SetSelected(null);
        }
// Representative: show = visible;

{
        if (autoDiscover)
        {
            categories = DiscoverFromResources(resourcesPath);
            selectedCategoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, Mathf.Max(0, categories.Length - 1));
            scroll = Vector2.zero;
        }
    }// Representative: if (autoDiscover)

```

### Comments & Documentation
- // ============================================================================
- // BuildMenuUI.cs
- // PURPOSE:
- // - Provides the entire in-game Build Menu UI.
- // - Displays categories, items, costs, search, and affordability.
- // - Syncs visibility with the build grid.
- // - Allows the player to select a BuildItemDefinition for placement.
- // DEPENDENCIES:
- // - BuildItemDefinition:
- // * Provides displayName, icon, costs, category.

### Keywords
add, affordability, Allows, and, assets, Auto, autoDiscover, based, Build, BuildingsDatabase, BuildGridManager, building, BuildItemDefinition, BuildItems, BuildMenuUI, BuildPlacementManager, button, CanAfford, CanAffordAvailable, categories, category, check, checks, Clamp, Clears, clicks, close, closes, costs, debugging, DEPENDENCIES, DiscoverFromResources, discovery, displayName, Displays, entire, expose, extend, false, filter, folder, for, from, FUTURE, game, grid, hotkeys, icon, IMGUIInputBlocker, indirect, INSPECTOR, Instance, integrate, item, items, key, Length, lists, loads, MAINTENANCE, manually, Mathf, Max, may, Menu, MonoBehaviour, multi, new, NOTES, null, off, open, optional, page, placement, player, playerTeamID, populated, Prevents, previews, Provides, PURPOSE, Receives, replaced, Representative, requirements, Resources, resourcesPath, script, scroll, search, select, selectedCategoryIndex, selection, SetSelected, show, showAffordability, showCosts, ShowPreviewAt, state, sync, Syncs, system, tab, TeamResources, TeamStorageManager, tech, the, them, this, through, toggleKey, toggles, Toolkit, tools, tree, trySyncBuildGridVisibility, TrySyncGrid, uGUI, Unity, unlock, Update, Used, Vector2, via, visibility, visible, when, with, you, zero

### Cross References
**Uses:** BuildingsDatabase, BuildGridManager, BuildItemDefinition, BuildPlacementManager, IMGUIInputBlocker, ResourceCost, TeamResources, TeamStorageManager
**Used By:** BuildingsDatabase, BuildGridCell, BuildGridManager, BuildItemDefinition, Dependencies


## Assets/Scripts/Systems/UI/ConstructionManager.cs
**Script:** ConstructionManager
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ConstructionManager appears to be a ui script derived from MonoBehaviour. It appears to be related to: ConstructionManager, MonoBehaviour, eventually, track, active, construction, sites, update.

### Comments & Documentation
- // This will eventually track all active construction sites
- // and update their progress each frame.

### Keywords
active, all, and, construction, ConstructionManager, each, eventually, frame, MonoBehaviour, progress, sites, their, This, track, update, will

### Cross References
**Uses:** -
**Used By:** Dependencies, GameBootstrap


## Assets/Scripts/Systems/UI/EconomyUI.cs
**Script:** EconomyUI
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
EconomyUI appears to be a ui script derived from MonoBehaviour. It appears to be related to: EconomyUI, MonoBehaviour.

### Keywords
EconomyUI, MonoBehaviour

### Cross References
**Uses:** ResourceType, Team, TeamResources
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/IMGUIInputBlocker.cs
**Script:** IMGUIInputBlocker
**System Classification:** UI
**Base Class:** -
**Interfaces:** -

### Summary
IMGUIInputBlocker appears to be a ui script. It appears to be related to: IMGUIInputBlocker, summary, Register, panel, frame, OnGUI, Returns, mouse.

### Comments & Documentation
- // / <summary>
- // / Register a panel rect each frame from OnGUI.
- // / </summary>
- // / <summary>
- // / Returns true if the mouse is over any registered IMGUI panel.
- // / Works in Update by using last frame's registered rects.
- // / </summary>
- // If no panels have registered recently, don't block anything.
- // During Update, lastRegisterFrame will usually be (Time.frameCount - 1) when visible.
- // Convert screen coords (bottom-left) -> IMGUI coords (top-left)
- /// <summary>
- /// Register a panel rect each frame from OnGUI.
- /// </summary>
- /// <summary>
- /// Returns true if the mouse is over any registered IMGUI panel.
- /// Works in Update by using last frame's registered rects.
- /// </summary>

### Keywords
any, anything, block, bottom, Convert, coords, don, During, each, frame, frameCount, from, have, IMGUI, IMGUIInputBlocker, last, lastRegisterFrame, left, mouse, OnGUI, over, panel, panels, recently, rect, rects, Register, registered, Returns, screen, summary, the, Time, top, true, Update, using, usually, visible, when, will, Works

### Cross References
**Uses:** -
**Used By:** BuildMenuUI, Dependencies, TaskBoardUI


## Assets/Scripts/Systems/UI/MainMenuUI.cs
**Script:** MainMenuUI
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
MainMenuUI appears to be a ui script derived from MonoBehaviour. It appears to be related to: MainMenuUI, MonoBehaviour, buttonStyle, GUIStyle, button, fontSize, alignment, TextAnchor.

### Private Methods (excerpts)
```csharp
{
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

// Representative: buttonStyle = new GUIStyle(GUI.skin.button)

{
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "MAIN MENU", boxStyle);

        float x = panelRect.x + 20;
// Representative: float left = (Screen.width - panelWidth) / 2f;

{
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "LOAD GAME", boxStyle);

        float x = panelRect.x + 20;
// Representative: float left = (Screen.width - panelWidth) / 2f;

{
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "CREDITS", boxStyle);

        float x = panelRect.x + 20;
// Representative: float left = (Screen.width - panelWidth) / 2f;

```

### Comments & Documentation
- // Lazy initialization — safe in Unity 2019

### Keywords
2019, alignment, Bold, Box, boxStyle, button, buttonStyle, CREDITS, float, fontSize, fontStyle, GAME, GUI, GUIStyle, height, initialization, Lazy, left, LOAD, MAIN, MainMenuUI, MENU, MiddleCenter, MonoBehaviour, new, panelHeight, panelRect, panelWidth, Rect, Representative, safe, Screen, skin, TextAnchor, top, Unity, width

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/MapVisualsBootstrap.cs
**Script:** MapVisualsBootstrap
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
MapVisualsBootstrap appears to be a ui script derived from MonoBehaviour. It appears to be related to: MapVisualsBootstrap, MonoBehaviour, Transparency, Standard, shader, finicky, renders, opaque.

### Comments & Documentation
- // Transparency hack in Standard shader can be finicky in 2019;
- // if it renders opaque, set Rendering Mode = Transparent in the material manually.

### Keywords
2019, can, finicky, hack, manually, MapVisualsBootstrap, material, Mode, MonoBehaviour, opaque, Rendering, renders, set, shader, Standard, the, Transparency, Transparent

### Cross References
**Uses:** TeamColorManager
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/Minimap.cs
**Script:** Minimap
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Minimap appears to be a ui script derived from MonoBehaviour. It appears to be related to: Minimap, MonoBehaviour.

### Keywords
Minimap, MonoBehaviour

### Cross References
**Uses:** ResourceNode, TeamColorUtils, Turret, Unit
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/NeonRing.cs
**Script:** NeonRing
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
NeonRing appears to be a ui script derived from MonoBehaviour. It appears to be related to: NeonRing, MonoBehaviour.

### Keywords
MonoBehaviour, NeonRing

### Cross References
**Uses:** Building, Civilian, TeamColorManager, Unit
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/ResourceVisualBuilder.cs
**Script:** ResourceVisualBuilder
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ResourceVisualBuilder appears to be a ui script derived from MonoBehaviour. It appears to be related to: ResourceVisualBuilder, MonoBehaviour, Clear, children.

### Comments & Documentation
- // Clear children

### Keywords
children, Clear, MonoBehaviour, ResourceVisualBuilder

### Cross References
**Uses:** ResourceType
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/SciFiTeamStyler.cs
**Script:** SciFiTeamStyler
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SciFiTeamStyler appears to be a ui script derived from MonoBehaviour. It appears to be related to: SciFiTeamStyler, MonoBehaviour, TeamColorManager, Instance, return, Color, GetTeamColor, teamID.

### Public Methods (excerpts)
```csharp
{
        if (TeamColorManager.Instance == null) return;

        Color team = TeamColorManager.Instance.GetTeamColor(teamID);
        Color baseColor = team;

        // Sci-fi type grading
        if (kind == VisualKind.Civilian) baseColor = Color.Lerp(team, Color.white, civilianLighten);
// Representative: if (TeamColorManager.Instance == null) return;

```

### Comments & Documentation
- // Sci-fi type grading
- // Emission for bloom/glow

### Keywords
baseColor, bloom, Civilian, civilianLighten, Color, Emission, for, GetTeamColor, glow, grading, Instance, kind, Lerp, MonoBehaviour, null, Representative, return, Sci, SciFiTeamStyler, team, TeamColorManager, teamID, type, VisualKind, white

### Cross References
**Uses:** Building, Civilian, TeamColorManager
**Used By:** Dependencies, TeamVisual


## Assets/Scripts/Systems/UI/SelectionManager.cs
**Script:** SelectionManager
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SelectionManager appears to be a ui script derived from MonoBehaviour. It appears to be related to: SelectionManager, MonoBehaviour, SELECTION, ATTACK, ORDERS, Right, clicked, ground.

### Comments & Documentation
- // -------------------------------------------------
- // SELECTION
- // -------------------------------------------------
- // -------------------------------------------------
- // ATTACK ORDERS
- // -------------------------------------------------
- // Right-clicked ground → clear order
- // -------------------------------------------------
- // INTERNAL
- // -------------------------------------------------

### Keywords
ATTACK, clear, clicked, ground, INTERNAL, MonoBehaviour, order, ORDERS, Right, SELECTION, SelectionManager

### Cross References
**Uses:** Attackable, Selectable, UnitCombatController, UnitInspectorUI
**Used By:** Dependencies, UnitInspectorUI


## Assets/Scripts/Systems/UI/SelectionRing.cs
**Script:** SelectionRing
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SelectionRing appears to be a ui script derived from MonoBehaviour. It appears to be related to: SelectionRing, MonoBehaviour, Remove, collider, important, Color.

### Comments & Documentation
- // Remove collider (important)
- // Color by team + type

### Keywords
collider, Color, important, MonoBehaviour, Remove, SelectionRing, team, type

### Cross References
**Uses:** Building, Civilian, TeamColorManager, Unit
**Used By:** Dependencies


## Assets/Scripts/Systems/UI/TaskBoardUI.cs
**Script:** TaskBoardUI
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TaskBoardUI appears to be a ui script derived from MonoBehaviour. It appears to be related to: TaskBoardUI, MonoBehaviour, DEPENDENCIES, JobManager, GetRoleCounts, GetActiveConstructionSiteCount, CivilianRole, expects.

### Comments & Documentation
- // =============================================================
- // TaskBoardUI.cs
- // DEPENDENCIES:
- // - JobManager: GetRoleCounts(), GetActiveConstructionSiteCount()
- // - CivilianRole: expects Gatherer, Builder, Hauler, Idle
- // - TeamStorageManager: building-only stored/capacity/reserved totals
- // - IMGUIInputBlocker: prevents clicks through the panel
- // NOTES FOR FUTURE MAINTENANCE:
- // - If you add new roles, update the display line for civilians.
- // - If storage logic changes, update the resource lines accordingly.

### Keywords
accordingly, add, Builder, building, capacity, changes, CivilianRole, civilians, clicks, DEPENDENCIES, display, expects, FOR, FUTURE, Gatherer, GetActiveConstructionSiteCount, GetRoleCounts, Hauler, Idle, IMGUIInputBlocker, JobManager, line, lines, logic, MAINTENANCE, MonoBehaviour, new, NOTES, only, panel, prevents, reserved, resource, roles, storage, stored, TaskBoardUI, TeamStorageManager, the, through, totals, update, you

### Cross References
**Uses:** Civilian, CivilianRole, IMGUIInputBlocker, JobManager, ResourceType, Team, TeamStorageManager
**Used By:** CivilianRole, Dependencies, JobManager


## Assets/Scripts/Systems/UI/TeamBootstrap.cs
**Script:** TeamBootstrap
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamBootstrap appears to be a ui script derived from MonoBehaviour. It appears to be related to: TeamBootstrap, MonoBehaviour, hqRoot, Debug, LogWarning, teamID, Expected, Team_X.

### Private Methods (excerpts)
```csharp
{
        if (team.hqRoot == null)
        {
            Debug.LogWarning($"Team {team.teamID} has no HQ root. Expected: Team_X -> HQ.");
            return;
        }

        // Skip if HQ already exists
// Representative: if (team.hqRoot == null)

{
        if (workerPrefab == null)
            return;

        if (team.unitsRoot == null)
        {
            Debug.LogWarning($"Team {team.teamID} has no Units root. Expected: Team_X -> Units.");
            return;
        }
// Representative: if (workerPrefab == null)

{
        if (team.teamType != TeamType.AI)
            return;

        // Add AI components ONLY if missing
        AddIfMissing<AIPlayer>(team.gameObject);
        AddIfMissing<AIEconomy>(team.gameObject);
        AddIfMissing<AIMilitary>(team.gameObject);
// Representative: if (team.teamType != TeamType.AI)

```

### Comments & Documentation
- // / <summary>
- // / TeamBootstrap
- // /
- // / PURPOSE:
- // / - Initialize each Team object at runtime.
- // / - Spawn HQs for teams missing one.
- // / - Spawn starting workers.
- // / - Attach AI subsystems to AI teams.
- // /
- // / DEPENDENCIES:
- /// <summary>
- /// TeamBootstrap
- /// PURPOSE:
- /// - Initialize each Team object at runtime.
- /// - Spawn HQs for teams missing one.
- /// - Spawn starting workers.
- /// - Attach AI subsystems to AI teams.
- /// DEPENDENCIES:
- /// - Team.cs:
- /// * Provides teamID, teamType, hqRoot, unitsRoot.

### Keywords
Add, Added, AddIfMissing, after, AIBuilder, AIEconomy, AIMilitary, AIPlayer, AIResourceManager, AIThreatDetector, already, and, any, apply, ARCHITECTURE, Assign, assignment, Attach, automatically, base, bootstrap, Building, camera, class, colors, component, components, contain, correct, count, Debug, deletes, DEPENDENCIES, Designed, Detects, each, enforces, exists, Expected, extend, for, FUTURE, GameManager, gameObject, global, has, have, hqRoot, HQs, include, Initialize, LogWarning, MAINTENANCE, maximum, may, missing, modifies, MonoBehaviour, multi, must, near, NEVER, new, NOTES, null, number, object, objects, once, one, ONLY, optional, ownership, prefab, prefabs, present, Provides, PURPOSE, Representative, return, root, roots, RTS, runs, runtime, Safe, scene, script, SETUP, SetupAI, SetupWorkers, Skip, Spawn, spawns, start, starting, subsystems, summary, systems, team, Team_X, TeamBootstrap, TeamColorManager, teamID, teamIDs, teams, teamType, TeamVisual, them, This, Unit, Units, unitsRoot, use, Vehicles, via, will, with, work, Worker, workerPrefab, workers, you, your

### Cross References
**Uses:** AIBuilder, AIEconomy, AIMilitary, AIPlayer, AIResourceManager, AIThreatDetector, Building, GameManager, Team, TeamColorManager, TeamVisual, Unit
**Used By:** Dependencies, Team


## Assets/Scripts/Systems/UI/UnitInspectorUI.cs
**Script:** UnitInspectorUI
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
UnitInspectorUI appears to be a units script derived from MonoBehaviour. It appears to be related to: UnitInspectorUI, MonoBehaviour, selected, Debug, Selection, cleared, Representative, float.

### Properties
```csharp
float CurrentHealth { get;
float MaxHealth { get;
```

### Public Methods (excerpts)
```csharp
{
        selected = obj;

        if (selected != null)
            Debug.Log("UnitInspectorUI: Selected " + selected.name);
        else
            Debug.Log("UnitInspectorUI: Selection cleared");
    }// Representative: selected = obj;

```

### Comments & Documentation
- // =============================================================
- // UnitInspectorUI.cs
- // PURPOSE:
- // - On-screen inspector for the currently selected object.
- // - Shows health, team, civilian/unit info, barracks queue, combat stance,
- // construction progress, storage contents, and turret info.
- // DEPENDENCIES:
- // - SelectionManager (or equivalent):
- // * Must call SetSelected(GameObject) when selection changes.
- // - Barracks:

### Keywords
add, and, any, API, APIs, Attack, attackRange, barracks, bool, call, Called, canAttackCivilians, CancelLast, CanQueue, change, changes, civilian, Civilians, cleared, combat, combatEnabled, CombatStance, component, construction, ConstructionSite, contents, CurrentBuildTime, CurrentHealth, currently, CurrentProgress, damage, Debug, def, DEPENDENCIES, dependent, display, DrawX, else, equivalent, etc, existing, Expected, Exposes, float, for, FUTURE, game, GameObject, get, GetCapacity, GetDeliveredAmount, GetRequiredCosts, GetStatusLine, GetStored, GetTargetStatus, health, here, IHasHealth, implement, info, inspector, int, Interface, Keep, List, Log, logic, MAINTENANCE, Marker, match, MaxHealth, MonoBehaviour, Must, name, new, newStance, not, NOTES, null, obj, object, ones, only, overloading, producibleUnits, progress, PURPOSE, put, queue, QueueUnit, rather, Representative, ResourceCost, ResourceStorageContainer, ResourceType, screen, script, sections, selected, Selection, SelectionManager, SetSelected, SetStance, Shows, specific, stance, storage, string, systems, target, team, teamID, than, the, this, toggle, ToggleAttackCivilians, turret, type, types, unit, UnitCombatController, UnitInspectorUI, UnitProductionDefinition, update, Used, void, when, with, you

### Cross References
**Uses:** Barracks, Civilian, CombatStance, ConstructionSite, IHasHealth, ResourceCost, ResourceStorageContainer, ResourceType, SelectionManager, Team, Turret, Unit, UnitCombatController, UnitProductionDefinition
**Used By:** Attackable, Barracks, CombatSystem, Dependencies, SelectionManager, UnitCombatController, UnitProductionDefinition, UnitProductionQueue


## Assets/Scripts/Systems/UI/WorldHealthBar.cs
**Script:** WorldHealthBar
**System Classification:** UI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
WorldHealthBar appears to be a ui script derived from MonoBehaviour. It appears to be related to: WorldHealthBar, MonoBehaviour, Smooth, displayed, value, prevents, shimmer, Update.

### Comments & Documentation
- // Smooth the displayed value (prevents shimmer)
- // Update fill width + pivot so it shrinks from left-to-right
- // Color shifts Green -> Yellow -> Red
- // Stable billboard (no LookRotation jitter)
- // Background (slightly behind fill to prevent z-fighting)
- // Fill (slightly in front)
- // Remove colliders
- // Dark sci-fi plate
- // Start fill color

### Keywords
Background, behind, billboard, colliders, Color, Dark, displayed, fighting, fill, from, front, Green, jitter, left, LookRotation, MonoBehaviour, pivot, plate, prevent, prevents, Red, Remove, right, sci, shifts, shimmer, shrinks, slightly, Smooth, Stable, Start, the, Update, value, width, WorldHealthBar, Yellow

### Cross References
**Uses:** Building, IHasHealth
**Used By:** Dependencies


## Assets/Scripts/Systems/Units/CivilianSpawner.cs
**Script:** CivilianSpawner
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
CivilianSpawner appears to be a units script derived from MonoBehaviour. It appears to be related to: CivilianSpawner, MonoBehaviour.

### Keywords
CivilianSpawner, MonoBehaviour

### Cross References
**Uses:** Civilian, TeamColorManager
**Used By:** Dependencies


## Assets/Scripts/Systems/Units/Selectable.cs
**Script:** Selectable
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
Selectable appears to be a units script derived from MonoBehaviour. It appears to be related to: Selectable, MonoBehaviour, selectionRing, SetActive, selected, Representative.

### Public Methods (excerpts)
```csharp
{
        if (selectionRing != null)
            selectionRing.SetActive(selected);
    }// Representative: if (selectionRing != null)

```

### Keywords
MonoBehaviour, null, Representative, Selectable, selected, selectionRing, SetActive

### Cross References
**Uses:** -
**Used By:** Dependencies, SelectionManager


## Assets/Scripts/Systems/Units/UnitCombatController.cs
**Script:** UnitCombatController
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
UnitCombatController appears to be a units script derived from MonoBehaviour. It appears to be related to: UnitCombatController, MonoBehaviour, IsValidTarget, target, return, currentTarget, hasManualTarget, Representative.

### Public Methods (excerpts)
```csharp
{
        if (!IsValidTarget(target)) return;
        currentTarget = target;
        hasManualTarget = true;
    }// Representative: if (!IsValidTarget(target)) return;

{
        hasManualTarget = false;
        currentTarget = null;
    }// Representative: hasManualTarget = false;

{
        // (You can expand this later to change behaviour based on stance.)
        // For now, stance is informational + used by UI.
        // If you add stance-specific logic, keep UI + AI in sync.
        // e.g., Aggressive could increase search radius, Hold could disable AcquireTarget().
        // This is left simple for now.
        // NOTE: UnitInspectorUI relies on this existing.
        // (No extra logic required yet.)

{
        if (currentTarget == null)
            return "None";

        return currentTarget.name + (hasManualTarget ? " (Ordered)" : "");
    }// Representative: if (currentTarget == null)

{
        canAttackCivilians = !canAttackCivilians;
    }// Representative: canAttackCivilians = !canAttackCivilians;

```

### Comments & Documentation
- // =============================================================
- // UnitCombatController.cs (Updated for WeaponComponent + UI)
- // DEPENDENCIES:
- // - WeaponComponent:
- // * Handles firing, cooldowns, projectile spawning.
- // - Attackable:
- // * Target interface (teamID, IsAlive, isCivilian).
- // - DiplomacyManager:
- // * War/peace filtering via AreAtWar(teamA, teamB).
- // - AIMilitary:

### Keywords
AcquireTarget, add, Aggressive, AIMilitary, and, any, AreAtWar, Attackable, based, behaviour, break, But, Calls, can, canAttackCivilians, change, ClearManualTarget, CombatStance, cooldowns, could, currentTarget, DEPENDENCIES, DiplomacyManager, disable, doesn, etc, existing, expand, extra, false, filtering, fire, firing, focus, For, FUTURE, GetTargetStatus, Handles, hasManualTarget, Hold, how, increase, informational, interface, IsAlive, isCivilian, IsValidTarget, keep, Keeping, later, left, logic, MAINTENANCE, manipulates, might, minimal, MonoBehaviour, name, new, newStance, None, NOTE, NOTES, now, null, Ordered, peace, projectile, radius, relies, Representative, required, return, search, SetManualTarget, SetStance, silently, simple, spawning, specific, stance, stances, still, store, sync, target, targeting, teamA, teamB, teamID, that, this, ToggleAttackCivilians, true, UnitCombatController, UnitInspectorUI, update, Updated, used, Uses, via, want, War, WeaponComponent, works, yet, You

### Cross References
**Uses:** AIMilitary, Attackable, CombatStance, DiplomacyManager, UnitInspectorUI, WeaponComponent
**Used By:** AIMilitary, Attackable, CombatSystem, Dependencies, DiplomacyManager, SelectionManager, Turret, TurretProjectile, Unit, UnitCommandController, UnitInspectorUI, UnitProductionDefinition, WeaponComponent


## Assets/Scripts/Systems/Units/UnitCommandController.cs
**Script:** UnitCommandController
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** ICommandable

### Summary
UnitCommandController appears to be a units script derived from MonoBehaviour. It appears to be related to: UnitCommandController, MonoBehaviour, ICommandable, stance, CombatStance, TakePoint, followTarget, hasDefendAnchor.

### Public Methods (excerpts)
```csharp
{
        stance = CombatStance.TakePoint;
        followTarget = null;
        hasDefendAnchor = false;
        agent.SetDestination(worldPos);
    }// Representative: stance = CombatStance.TakePoint;

{
        stance = CombatStance.Defend;
        followTarget = null;
        defendAnchor = transform.position;
        hasDefendAnchor = true;
    }// Representative: stance = CombatStance.Defend;

{
        stance = CombatStance.FollowTarget;
        followTarget = target;
        hasDefendAnchor = false;
    }// Representative: stance = CombatStance.FollowTarget;

{
        if (stance == CombatStance.FollowTarget)
            stance = CombatStance.Defend;

        followTarget = null;
        hasDefendAnchor = false;
    }// Representative: if (stance == CombatStance.FollowTarget)

```

### Comments & Documentation
- // =============================================================
- // UnitCommandController.cs
- // DEPENDENCIES:
- // - NavMeshAgent:
- // * Handles movement for move/defend/follow behaviours.
- // - ICommandable (interface):
- // * Allows external systems (e.g., player input) to issue commands.
- // - CombatStance (enum):
- // * This is a separate stance enum from UnitCombatController.CombatStance,
- // used for movement/command behaviour.

### Keywords
agent, Allows, and, behaviour, behaviours, between, can, combat, CombatStance, command, commands, consider, creating, current, Defend, defendAnchor, definitions, DEPENDENCIES, enum, external, false, follow, followTarget, for, from, FUTURE, Handles, hasDefendAnchor, ICommandable, input, interface, issue, MAINTENANCE, merging, MonoBehaviour, move, movement, NavMeshAgent, not, NOTES, null, player, position, remove, Representative, safely, separate, SetDestination, shared, stance, stances, system, systems, TakePoint, target, the, This, transform, true, unify, UnitCombatController, UnitCommandController, used, worldPos, you, your

### Cross References
**Uses:** CombatStance, ICommandable, UnitCombatController
**Used By:** Dependencies


## Assets/Scripts/Systems/Units/UnitProductionQueue.cs
**Script:** UnitProductionQueue
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
UnitProductionQueue appears to be a units script derived from MonoBehaviour. It appears to be related to: UnitProductionQueue, MonoBehaviour, return, queue, QueueItem, timeRemaining, buildTime, Representative.

### Public Methods (excerpts)
```csharp
{
        if (def == null) return;

        queue.Add(new QueueItem
        {
            def = def,
            timeRemaining = def.buildTime
        });
// Representative: if (def == null) return;

{
        if (queue.Count == 0) return;

        if (queue[0] == current)
            current = null;

        queue.RemoveAt(queue.Count - 1);
    }// Representative: if (queue.Count == 0) return;

```

### Comments & Documentation
- // ============================================================================
- // UnitProductionQueue.cs
- // PURPOSE:
- // - Handles timed production of units inside Barracks or other production buildings.
- // - Maintains a FIFO queue of production tasks.
- // - Notifies Barracks when a unit is finished.
- // DEPENDENCIES:
- // - UnitProductionDefinition:
- // * Provides buildTime, unitPrefab, costs.
- // - Barracks:

### Keywords
Add, apply, are, automatically, Barracks, BEFORE, buffs, buildings, buildTime, Calls, cancel, CancelLast, categories, component, consider, contents, costs, Count, created, current, CurrentBuildTime, def, DEPENDENCIES, Enqueue, enqueueing, expose, FIFO, finished, FOR, full, FUTURE, handled, Handles, infantry, inside, INSPECTOR, integrate, into, lanes, Maintains, MAINTENANCE, missing, modifiers, MonoBehaviour, multiple, new, None, NOTES, Notifies, null, OnUnitCompleted, other, Owns, parallel, production, Progress01, Provides, PURPOSE, queue, QueueCount, QueueItem, queues, Reads, refunds, RemoveAt, Representative, REQUIREMENTS, return, safely, separate, showing, spawn, speed, spent, split, Subscribes, tasks, TeamResources, TeamStorageManager, tech, the, them, this, timed, timeRemaining, unit, UnitInspectorUI, unitPrefab, UnitProductionDefinition, UnitProductionQueue, units, upgrades, vehicles, when, with, you

### Cross References
**Uses:** Barracks, TeamResources, TeamStorageManager, UnitInspectorUI, UnitProductionDefinition
**Used By:** Barracks, Dependencies, UnitProductionDefinition


# Ungrouped

## Assets/Scripts/Managers/Core/AlertManager.cs
**Script:** AlertManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AlertManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: AlertManager, MonoBehaviour, alerts, Alert, timer, duration, Representative.

### Public Methods (excerpts)
```csharp
{
        alerts.Add(new Alert { text = text, timer = duration });
    }// Representative: alerts.Add(new Alert { text = text, timer = duration });

```

### Keywords
Add, Alert, AlertManager, alerts, duration, MonoBehaviour, new, Representative, text, timer

### Cross References
**Uses:** -
**Used By:** Attackable, CombatSystem, Dependencies


## Assets/Scripts/Managers/Core/GameManager.cs
**Script:** GameManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
GameManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: GameManager, MonoBehaviour, allTeams, FindObjectsOfType, Identify, player, foreach, teamType.

### Private Methods (excerpts)
```csharp
{
        allTeams = FindObjectsOfType<Team>();

        // Identify player team
        foreach (var team in allTeams)
        {
            if (team.teamType == TeamType.Player)
            {
                playerTeam = team;
                break;
            }
        }
// Representative: allTeams = FindObjectsOfType<Team>();

```

### Comments & Documentation
- // / <summary>
- // / Central orchestrator for Cube Wars.
- // /
- // / Responsibilities:
- // / - Detect all teams in the scene
- // / - Track player team and AI teams
- // / - Provide global access to team data
- // / - Handle win/loss conditions (later)
- // / - Coordinate high-level game state
- // /
- /// <summary>
- /// Central orchestrator for Cube Wars.
- /// Responsibilities:
- /// - Detect all teams in the scene
- /// - Track player team and AI teams
- /// - Provide global access to team data
- /// - Handle win/loss conditions (later)
- /// - Coordinate high-level game state
- /// IMPORTANT:
- /// This script does NOT spawn, delete, or modify teams.

### Keywords
access, all, allTeams, and, break, categorizes, Central, components, conditions, Coordinate, Cube, data, delete, destructive, Detect, does, FindObjectsOfType, Finds, for, foreach, game, GameManager, global, Handle, high, Identify, IMPORTANT, intentionally, later, level, loss, modify, MonoBehaviour, non, NOT, orchestrator, player, playerTeam, Provide, Representative, Responsibilities, scene, script, spawn, state, summary, Team, teams, teamType, the, them, This, Track, var, Wars, win

### Cross References
**Uses:** Team
**Used By:** Dependencies, GameBootstrap, Team, TeamBootstrap


## Assets/Scripts/Managers/Core/SaveLoadManager.cs
**Script:** SaveLoadManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SaveLoadManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: SaveLoadManager, MonoBehaviour, Placeholder, future, system.

### Comments & Documentation
- // Placeholder for future save/load system.

### Keywords
for, future, load, MonoBehaviour, Placeholder, save, SaveLoadManager, system

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Managers/Systems/AIManager.cs
**Script:** AIManager
**System Classification:** AI
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
AIManager appears to be a ai script derived from MonoBehaviour. It appears to be related to: AIManager, MonoBehaviour, summary, Placeholder, future, orchestration, DEPENDENCIES, RESPONSIBILITIES.

### Comments & Documentation
- // / <summary>
- // / Placeholder for future AI orchestration.
- // /
- // / DEPENDENCIES:
- // / - None yet
- // /
- // / RESPONSIBILITIES:
- // / - Will eventually coordinate AI players
- // /
- // / IMPORTANT:
- /// <summary>
- /// Placeholder for future AI orchestration.
- /// DEPENDENCIES:
- /// - None yet
- /// RESPONSIBILITIES:
- /// - Will eventually coordinate AI players
- /// IMPORTANT:
- /// - Does NOT delete teams
- /// </summary>

### Keywords
AIManager, coordinate, delete, DEPENDENCIES, Does, eventually, for, future, IMPORTANT, MonoBehaviour, None, NOT, orchestration, Placeholder, players, RESPONSIBILITIES, summary, teams, Will, yet

### Cross References
**Uses:** -
**Used By:** Dependencies, GameBootstrap


## Assets/Scripts/Managers/Systems/CombatManager.cs
**Script:** CombatManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
CombatManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: CombatManager, MonoBehaviour, eventually, coordinate, global, combat, logic, threat.

### Comments & Documentation
- // This will eventually coordinate global combat logic,
- // threat detection, and shared combat utilities.

### Keywords
and, combat, CombatManager, coordinate, detection, eventually, global, logic, MonoBehaviour, shared, This, threat, utilities, will

### Cross References
**Uses:** -
**Used By:** Dependencies, GameBootstrap


## Assets/Scripts/Managers/Systems/DiplomacyManager.cs
**Script:** DiplomacyManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
DiplomacyManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: DiplomacyManager, MonoBehaviour, return, false, warMatrix, ContainsKey, Contains, Representative.

### Public Methods (excerpts)
```csharp
{
        if (a == b) return false;
        return warMatrix.ContainsKey(a) && warMatrix[a].Contains(b);
    }// Representative: if (a == b) return false;

{
        Ensure(a);
        Ensure(b);
        warMatrix[a].Add(b);
        warMatrix[b].Add(a);
    }// Representative: Ensure(a);

{
        if (warMatrix.ContainsKey(a)) warMatrix[a].Remove(b);
        if (warMatrix.ContainsKey(b)) warMatrix[b].Remove(a);
    }// Representative: if (warMatrix.ContainsKey(a)) warMatrix[a].Remove(b);

```

### Private Methods (excerpts)
```csharp
{
        if (!warMatrix.ContainsKey(team))
            warMatrix[team] = new HashSet<int>();
    }// Representative: if (!warMatrix.ContainsKey(team))

```

### Comments & Documentation
- // ============================================================================
- // DiplomacyManager.cs
- // PURPOSE:
- // - Central authority for team relationships (war/peace).
- // - Determines whether two teams are hostile.
- // - Used by combat, AI, and future faction systems.
- // DEPENDENCIES:
- // - UnitCombatController:
- // * Calls AreAtWar(teamA, teamB) to validate targets.
- // - Attackable:
- /// <summary>
- /// Returns true if teams a and b are hostile.
- /// </summary>
- /// <summary>
- /// Declares symmetric war between two teams.
- /// </summary>
- /// <summary>
- /// Ends war between two teams.
- /// </summary>

### Keywords
across, Add, adding, aggression, AIMilitary, all, Alliance, Alliances, and, are, AreAtWar, Attackable, authority, between, buildings, but, Calls, Ceasefire, Central, civilians, combat, consider, consistent, Contains, ContainsKey, Currently, data, decide, Declares, defense, DEPENDENCIES, determine, Determines, diplomacy, DiplomacyManager, diplomatic, display, Ends, Ensure, event, events, exists, expand, faction, factions, false, for, future, HashSet, hates, hostile, hostility, IDs, ignores, int, integration, Load, MAINTENANCE, May, model, MonoBehaviour, Must, need, Neutral, new, not, NOTES, One, only, OnPeaceMade, OnWarDeclared, peace, persist, persisted, priorities, PURPOSE, reactions, relationships, Remove, Representative, return, Returns, Save, serialize, sessions, should, states, strategic, summary, supports, symmetric, System, systems, targets, team, teamA, teamB, teamID, teams, that, the, Truce, true, turrets, two, UnitCombatController, units, Used, Uses, validate, war, warMatrix, way, whether, will, with, yet, you

### Cross References
**Uses:** AIMilitary, Attackable, UnitCombatController
**Used By:** AIMilitary, AIThreatDetector, Attackable, Dependencies, UnitCombatController


## Assets/Scripts/Managers/Systems/TeamColorManager.cs
**Script:** TeamColorManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamColorManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: TeamColorManager, MonoBehaviour, teamColors, Length, return, Color, white, teamID.

### Public Methods (excerpts)
```csharp
{
        if (teamColors == null || teamColors.Length == 0) return Color.white;
        if (teamID < 0) teamID = 0;
        if (teamID >= teamColors.Length) teamID = teamID % teamColors.Length;
        return teamColors[teamID];
    }// Representative: if (teamColors == null || teamColors.Length == 0) return Color.white;

{
        Color c = GetTeamColor(teamID);

        var renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r.material != null)
                r.material.color = c;
        }
// Representative: Color c = GetTeamColor(teamID);

```

### Comments & Documentation
- // =============================================================
- // TeamColorManager.cs
- // PURPOSE:
- // - Provides color lookup for all teams.
- // - Applies team colors to units, buildings, UI, and minimap icons.
- // DEPENDENCIES:
- // - Renderer (Unity):
- // * Used to apply color to all materials on an object.
- // - Team.cs:
- // * teamID determines which color to use.

### Keywords
all, and, Applies, apply, array, attach, Blue, buildings, Color, colors, Consider, count, Cyan, delete, DEPENDENCIES, determines, does, Ensure, exist, first, for, foreach, FUTURE, GetComponentsInChildren, GetTeamColor, global, Green, icons, IMPORTANT, instances, instead, Length, lookup, MAINTENANCE, matches, material, MaterialPropertyBlock, materials, maximum, minimap, MonoBehaviour, multiple, NOT, NOTES, null, obj, object, objects, only, optional, pattern, performance, Provides, Purple, PURPOSE, Red, Renderer, renderers, Representative, return, script, singleton, Some, survives, switching, team, TeamColorManager, teamColors, teamID, teams, TeamVisual, the, This, true, units, Unity, use, Used, uses, var, which, white, Yellow

### Cross References
**Uses:** Team, TeamVisual
**Used By:** CivilianSpawner, Dependencies, MapVisualsBootstrap, NeonRing, SciFiMapBootstrap, SciFiTeamStyler, SelectionRing, Spawner, Team, TeamBootstrap, TeamVisual


## Assets/Scripts/Managers/Systems/TeamStorageManager.cs
**Script:** TeamStorageManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamStorageManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: TeamStorageManager, MonoBehaviour, return, RemoveFromAllTeams, EnsureTeam, teamID, storages, Contains.

### Public Methods (excerpts)
```csharp
{
        if (c == null) return;

        RemoveFromAllTeams(c);
        EnsureTeam(c.teamID);

        if (!storages[c.teamID].Contains(c))
            storages[c.teamID].Add(c);
// Representative: if (c == null) return;

{
        if (c == null) return;
        RemoveFromAllTeams(c);
    }// Representative: if (c == null) return;

{
        EnsureTeam(teamID);

        if (primaryStorage.TryGetValue(teamID, out var p) && p != null)
            return p;

        var list = storages[teamID];
        if (list.Count > 0)
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetStored(type);

// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetCapacity(type);

// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
            if (list[i] != null) sum += list[i].GetFree(type);

// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetStored(type);
        }
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetCapacity(type);
        }
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        int sum = 0;
        var list = storages[teamID];

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            if (!IsBuildingStorage(s)) continue;
            sum += s.GetFree(type);
        }
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        if (amount == 0) return;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            s.AddCapacity(type, amount);
        }
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        if (amount == 0) return;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s == null) continue;
            s.AddCapacity(type, -amount);
        }
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        return reservedTotals[teamID][type];
    }// Representative: EnsureTeam(teamID);

{
        if (!siteReserved.TryGetValue(siteKey, out var dict) || dict == null)
            return 0;

        return dict.TryGetValue(type, out var v) ? Mathf.Max(0, v) : 0;
    }// Representative: if (!siteReserved.TryGetValue(siteKey, out var dict) || dict == null)

{
        int stored = GetTotalStored(teamID, type);
        int reserved = GetReservedTotal(teamID, type);
        return Mathf.Max(0, stored - reserved);
    }// Representative: int stored = GetTotalStored(teamID, type);

{
        if (costs == null) return true;

        for (int i = 0; i < costs.Length; i++)
        {
            var c = costs[i];
            if (GetAvailable(teamID, c.type) < c.amount)
                return false;
        }
// Representative: if (costs == null) return true;

{
        if (!CanAffordAvailable(teamID, costs)) return false;

        EnsureTeam(teamID);

        if (!siteReserved.TryGetValue(siteKey, out var dict))
        {
            dict = new Dictionary<ResourceType, int>();
            foreach (ResourceType t in System.Enum.GetValues(typeof(ResourceType)))
                dict[t] = 0;
            siteReserved[siteKey] = dict;
        }
// Representative: if (!CanAffordAvailable(teamID, costs)) return false;

{
        EnsureTeam(teamID);

        if (!siteReserved.TryGetValue(siteKey, out var dict))
            return;

        foreach (var kv in dict)
            reservedTotals[teamID][kv.Key] = Mathf.Max(0, reservedTotals[teamID][kv.Key] - kv.Value);
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        if (!siteReserved.TryGetValue(siteKey, out var dict))
            return 0;

        int can = Mathf.Min(amount, dict[type]);
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        int remaining = amount;
        int takenTotal = 0;

        var list = storages[teamID];
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);
        if (amount <= 0) return 0;

        int remaining = amount;
        int acceptedTotal = 0;

        var list = storages[teamID];
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);

        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
// Representative: EnsureTeam(teamID);

{
        EnsureTeam(teamID);

        ResourceStorageContainer best = null;
        float bestD = float.MaxValue;

        var list = storages[teamID];
        for (int i = 0; i < list.Count; i++)
// Representative: EnsureTeam(teamID);

```

### Comments & Documentation
- // =============================================================
- // TeamStorageManager.cs
- // PURPOSE:
- // - Central storage backend for all teams.
- // - Tracks capacity, stored amounts, reservations, and withdrawals.
- // DEPENDENCIES:
- // - ResourceStorageContainer:
- // * Actual per-building storage.
- // - ResourceType / ResourceCost:
- // * Defines resource kinds and costs.

### Keywords
acceptedTotal, Actual, Add, AddCapacity, all, amount, amounts, and, APIs, attach, backend, best, bestD, bucket, building, Calls, can, CanAffordAvailable, capacity, Central, Civilian, components, consistent, ConstructionSite, Contains, continue, costs, Count, Defines, delete, deletes, DEPENDENCIES, Deposit, determines, dict, Dictionary, does, duplicate, EnsureTeam, Enum, Excluded, false, float, for, foreach, from, FUTURE, GameObjects, GetAvailable, GetCapacity, GetFree, GetReservedTotal, GetStored, GetTotalStored, GetValues, global, IMPORTANT, int, into, IsBuildingStorage, Key, kinds, Length, list, logic, MAINTENANCE, Management, Mathf, Max, MaxValue, Min, MonoBehaviour, multiple, must, new, NOT, NOTES, null, objects, only, out, pattern, per, primaryStorage, PURPOSE, queries, Registration, remain, remaining, RemoveFromAllTeams, Representative, reservation, reservations, reserved, reservedTotals, resource, ResourceCost, ResourceStorageContainer, ResourceType, return, script, singleton, siteKey, siteReserved, storage, storages, stored, sum, System, takenTotal, Team, teamID, TeamResources, teams, TeamStorageManager, this, Totals, Tracks, true, TryGetValue, type, typeof, types, Unit, update, use, Uses, Value, var, which, with, Withdraw, withdrawals, you

### Cross References
**Uses:** Building, Civilian, ConstructionSite, ResourceCost, ResourceStorageContainer, ResourceType, Team, TeamResources, Unit
**Used By:** Barracks, BuildMenuUI, BuildPlacementManager, Civilian, ConstructionSite, CraftingSystem, Dependencies, ResourceStorageContainer, ResourceStorageProvider, TaskBoardUI, Team, TeamResources, UnitProductionQueue


## Assets/Scripts/Managers/Systems/UnitManager.cs
**Script:** UnitManager
**System Classification:** Units
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
UnitManager appears to be a units script derived from MonoBehaviour. It appears to be related to: UnitManager, MonoBehaviour, eventually, track, units, handle, spawning, despawning.

### Comments & Documentation
- // This will eventually track all units,
- // handle spawning, despawning, and provide lists to AI/UI.

### Keywords
all, and, despawning, eventually, handle, lists, MonoBehaviour, provide, spawning, This, track, UnitManager, units, will

### Cross References
**Uses:** -
**Used By:** Dependencies, GameBootstrap


## Assets/Scripts/Managers/Systems/WinConditionManager.cs
**Script:** WinConditionManager
**System Classification:** Managers
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
WinConditionManager appears to be a managers script derived from MonoBehaviour. It appears to be related to: WinConditionManager, MonoBehaviour.

### Keywords
MonoBehaviour, WinConditionManager

### Cross References
**Uses:** Headquarters
**Used By:** Dependencies, GameBootstrap


# Utilities

## Assets/Scripts/Utilities/Debug/FindTeamDuplicates.cs
**Script:** FindTeamDuplicates
**System Classification:** Utilities
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
FindTeamDuplicates appears to be a utilities script derived from MonoBehaviour. It appears to be related to: FindTeamDuplicates, MonoBehaviour.

### Keywords
FindTeamDuplicates, MonoBehaviour

### Cross References
**Uses:** Team
**Used By:** Dependencies


## Assets/Scripts/Utilities/Debug/TeamWatchdog.cs
**Script:** TeamWatchdog
**System Classification:** Utilities
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
TeamWatchdog appears to be a utilities script derived from MonoBehaviour. It appears to be related to: TeamWatchdog, MonoBehaviour.

### Keywords
MonoBehaviour, TeamWatchdog

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Utilities/Helpers/ParentRenamer.cs
**Script:** ParentRenamer
**System Classification:** Utilities
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
ParentRenamer appears to be a utilities script derived from MonoBehaviour. It appears to be related to: ParentRenamer, MonoBehaviour, Transform, parentTransform, parent, random, words, firstNameIndex.

### Private Methods (excerpts)
```csharp
{
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            // Pick two random words (can be the same)
            int firstNameIndex = Random.Range(0, offensiveWords.Length);
            int lastNameIndex = Random.Range(0, offensiveWords.Length);

            string firstName = offensiveWords[firstNameIndex];
            string lastName = offensiveWords[lastNameIndex];

            // Set parent's name to "First Last"
            parentTransform.name = firstName + " " + lastName;

            // Optional: Log for debugging
            Debug.Log($"Renamed parent '{parentTransform.name}' (was '{parentTransform.gameObject.name}') from child '{gameObject.name}'");
        }
// Representative: Transform parentTransform = transform.parent;

```

### Comments & Documentation
- // The list of offensive words as a string array
- // Ensure this runs early, before Start() on other scripts
- // Pick two random words (can be the same)
- // Set parent's name to "First Last"
- // Optional: Log for debugging

### Keywords
array, before, can, child, Debug, debugging, early, Ensure, First, firstName, firstNameIndex, for, from, gameObject, int, Last, lastName, lastNameIndex, Length, list, Log, MonoBehaviour, name, null, offensive, offensiveWords, Optional, other, parent, ParentRenamer, parentTransform, Pick, random, Range, Renamed, Representative, runs, same, scripts, Set, Start, string, the, this, Transform, two, was, words

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Utilities/Helpers/SelfRenamer.cs
**Script:** SelfRenamer
**System Classification:** Utilities
**Base Class:** MonoBehaviour
**Interfaces:** -

### Summary
SelfRenamer appears to be a utilities script derived from MonoBehaviour. It appears to be related to: SelfRenamer, MonoBehaviour, random, words, firstNameIndex, Range, offensiveWords, Length.

### Private Methods (excerpts)
```csharp
{
        // Pick two random words (can be the same)
        int firstNameIndex = Random.Range(0, offensiveWords.Length);
        int lastNameIndex = Random.Range(0, offensiveWords.Length);

        string firstName = offensiveWords[firstNameIndex];
        string lastName = offensiveWords[lastNameIndex];

// Representative: int firstNameIndex = Random.Range(0, offensiveWords.Length);

```

### Comments & Documentation
- // The list of offensive words as a string array
- // Runs early, before Start() on other scripts
- // Pick two random words (can be the same)
- // Set this GameObject's name to "First Last"
- // Optional: Log for debugging

### Keywords
array, before, can, debugging, early, First, firstName, firstNameIndex, for, GameObject, int, Last, lastName, lastNameIndex, Length, list, Log, MonoBehaviour, name, offensive, offensiveWords, Optional, other, Pick, random, Range, Representative, Runs, same, scripts, SelfRenamer, Set, Start, string, the, this, two, words

### Cross References
**Uses:** -
**Used By:** Dependencies


## Assets/Scripts/Utilities/Helpers/TeamColorUtils.cs
**Script:** TeamColorUtils
**System Classification:** Utilities
**Base Class:** -
**Interfaces:** -

### Summary
TeamColorUtils appears to be a utilities script. It appears to be related to: TeamColorUtils.

### Keywords
TeamColorUtils

### Cross References
**Uses:** -
**Used By:** Dependencies, Minimap




## Data-driven Resource Migration Note
- ResourceType enum references are deprecated and removed from runtime resource logic.
- Runtime systems now pass ResourceDefinition references and/or normalized resource IDs.
- Food lookup is provided by FoodDatabase entries mapped by ResourceDefinition.


## 2026-02 Added Runtime Scripts
- `Assets/Scripts/Runtime/Buildings/InteractionPoint.cs`
- `Assets/Scripts/Runtime/Buildings/BuildingInteractionPointController.cs`
- `Assets/Scripts/Registry/HousingRegistry.cs`
- `Assets/Scripts/Runtime/Units/HousingController.cs`

### Migration Notes
- `Civilian` housing assignment/search responsibilities are delegated to `HousingController` + `HousingRegistry`.
- `StorageFacility.interactionPoint` legacy single-point field is replaced by `InteractionPoint` components managed through `BuildingInteractionPointController`.
- Crafting worker task generation caps queued craft jobs by available work points.
