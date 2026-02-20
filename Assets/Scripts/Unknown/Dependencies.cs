// ============================================================================
// Cube Wars — Global Dependency Map (Alphabetical + Category-Based)
// This file is documentation-only. It compiles, but contains no runtime logic.
// ============================================================================

using UnityEngine;

public class dependencies : MonoBehaviour
{
    /*
    ========================================================================
    = MASTER INDEX (A–Z) — Script Name + Purpose + Category
    ========================================================================

    AIBuildingPriority.cs — AI building priority weights (AI System)
    AIBuilder.cs — AI building placement logic (AI System)
    AIDifficulty.cs — Difficulty presets for AI behavior (AI System)
    AIEconomy.cs — AI economic management (AI System)
    AIManager.cs — Global AI coordinator (AI System)
    AIMilitary.cs — AI combat and army logic (AI System)
    AIPersonality.cs — Defines AI personality traits (AI System)
    AIPlayer.cs — Root AI brain per team (AI System)
    AIRebuildManager.cs — AI rebuilding destroyed structures (AI System)
    AIRepairManager.cs — AI repairing damaged buildings (AI System)
    AIResourceManager.cs — AI resource flow management (AI System)
    AIThreatDetector.cs — AI threat scanning and alerts (AI System)
    AlertManager.cs — Global alert/event notifications (Core)
    Attackable.cs — Base attackable component (Combat)
    AutoDestroyFX.cs — Auto-destroy particle effects (Combat)
    Barracks.cs — Produces infantry units (Buildings)
    BuildingsDatabase.cs — Catalog of buildable items (Buildings)
    BuildCellReservation.cs — Blocks grid cells for placement (Buildings)
    BuildGridCell.cs — Represents a buildable grid tile (Buildings)
    BuildGridManager.cs — Generates and manages build grid (Buildings)
    BuildItemDefinition.cs — Metadata for buildings (Buildings)
    BuildItemInstance.cs — Runtime instance of a building type (Buildings)
    BuildMenuUI.cs — UI for selecting buildings (UI)
    BuildPlacementManager.cs — Handles all building placement (Buildings)
    BuildTimeSettings.cs — Global build time modifiers (Buildings)
    Building.cs — Base class for all buildings (Buildings)
    CharacterStats.cs — Health, speed, combat stats (Core)
    Civilian.cs — Worker unit with jobs and skills (Civilians)
    CivilianRole.cs — Enum of civilian job roles (Civilians)
    CivilianSpawner.cs — Spawns civilians (Civilians)
    CombatManager.cs — Global combat coordinator (Combat)
    CombatStance.cs — Unit stance definitions (Combat)
    ConstructionManager.cs — UI for construction progress (UI)
    ConstructionSite.cs — In-progress building logic (Buildings)
    CraftingSystem.cs — Building crafting/refinement logic (Buildings)
    DefenseTurret.cs — Automated turret logic (Buildings)
    DiplomacyManager.cs — Handles alliances and hostility (Combat)
    EventManager.cs — Global event dispatcher (Core)
    Farm.cs — Produces food resources (Buildings)
    FindTeamDuplicates.cs — Debug tool for detecting duplicate teams (Debug)
    GameBootstrap.cs — High-level scene initialization (Core)
    GameManager.cs — Global game state manager (Core)
    Headquarters.cs — HQ building logic (Buildings)
    HQSpawner.cs — Documentation placeholder (Documents)
    IAttackable.cs — Interface for attackable objects (Combat)
    ICommandable.cs — Interface for commandable units (Combat)
    IHasHealth.cs — Interface for health-bearing objects (Combat)
    ITargetable.cs — Interface for targetable objects (Combat)
    IMGUIInputBlocker.cs — Blocks IMGUI input (UI)
    JobManager.cs — Manages civilian jobs and assignments (Civilians)
    MainMenuUI.cs — Main menu logic (UI)
    MapVisualsBootstrap.cs — Initializes map visuals (UI)
    Minimap.cs — Minimap rendering and tracking (UI)
    NeonRing.cs — Visual effect for selection rings (UI)
    ParentRenamer.cs — Utility for renaming parents (Core)
    Projectile.cs — Projectile behavior (Combat)
    ProjectilePool.cs — Pooling for projectiles (Combat)
    ResourceCapacityEntry.cs — Defines storage capacity (Buildings)
    ResourceCost.cs — Defines resource costs (Buildings)
    ResourceDropoff.cs — Dropoff point for resources (Buildings)
    ResourceNode.cs — Resource node logic (Buildings)
    ResourceRegistry.cs — Registry of resource types (Buildings)
    ResourceSpawner.cs — Spawns resource nodes (Buildings)
    ResourceStorageContainer.cs — Per-building storage (Buildings)
    ResourceStorageProvider.cs — Provides storage to workers (Buildings)
    ResourceDefinition.cs — Enum of resource types (Core)
    ResourceVisualBuilder.cs — Builds resource visuals (Buildings)
    RTSCamera.cs — RTS camera controller (Core)
    SaveLoadManager.cs — Save/load system (Core)
    SciFiMapBootstrap.cs — Map initialization (Core)
    SciFiTeamStyler.cs — Advanced team styling system (Core)
    Selectable.cs — Selectable object logic (Core)
    SelfRenamer.cs — Auto-renames objects (Core)
    SixTeamBootstrap.cs — Legacy bootstrap for 6 teams (Core)
    SkirmishConfig.cs — Skirmish mode configuration (Combat)
    Spawner.cs — Generic object spawner (Core)
    TaskBoardUI.cs — UI for job/task management (UI)
    Team.cs — Represents a team/faction (Core)
    TeamAIBuild.cs — Legacy/simple AI builder (AI System)
    TeamBootstrap.cs — UI-based team bootstrap (UI)
    TeamColorManager.cs — Team color lookup (Core)
    TeamColorUtils.cs — Utility for team color operations (Core)
    TeamInventory.cs — Per-team inventory (Core)
    TeamResources.cs — Resource façade for teams (Core)
    TeamStorageManager.cs — Storage backend for teams (Core)
    TeamVisual.cs — Applies team visuals (Core)
    TeamWatchdog.cs — Debug tool for detecting team deletion (Debug)
    TurretProjectile.cs — Turret projectile logic (Combat)
    TurretProjectilePool.cs — Pooling for turret projectiles (Combat)
    Unit.cs — Base unit logic (Combat)
    UnitCombatController.cs — Unit combat behavior (Combat)
    UnitCommandController.cs — Unit command handling (Combat)
    UnitInspectorUI.cs — UI for unit inspection (UI)
    UnitManager.cs — Global unit manager (Combat)
    UnitProductionDefinition.cs — Defines unit production data (Combat)
    UnitProductionQueue.cs — Production queue for units (Combat)
    VehicleFactory.cs — Produces vehicles (Buildings)
    WeaponComponent.cs — Weapon firing logic (Combat)
    WeaponsFactory.cs — Produces weapons (Buildings)
    WinConditionManager.cs — Win/loss evaluation (Core)
    WorldHealthBar.cs — World-space health bar UI (UI)


    ========================================================================
    = CATEGORY-BASED DEPENDENCY SECTIONS
    ========================================================================

    ------------------------------------------------------------------------
    AI SYSTEM
    ------------------------------------------------------------------------
    AIBuilder.cs
    AIBuildingPriority.cs
    AIDifficulty.cs
    AIEconomy.cs
    AIManager.cs
    AIMilitary.cs
    AIPersonality.cs
    AIPlayer.cs
    AIRebuildManager.cs
    AIRepairManager.cs
    AIResourceManager.cs
    AIThreatDetector.cs
    MultiTeamAIDirector.cs
    TeamAIBuild.cs

    ------------------------------------------------------------------------
    BUILDING SYSTEM
    ------------------------------------------------------------------------
    Barracks.cs
    BuildingsDatabase.cs
    BuildCellReservation.cs
    BuildGridCell.cs
    BuildGridManager.cs
    BuildItemDefinition.cs
    BuildItemInstance.cs
    BuildPlacementManager.cs
    BuildTimeSettings.cs
    Building.cs
    ConstructionSite.cs
    CraftingSystem.cs
    DefenseTurret.cs
    Farm.cs
    Headquarters.cs
    ResourceCapacityEntry.cs
    ResourceCost.cs
    ResourceDropoff.cs
    ResourceNode.cs
    ResourceRegistry.cs
    ResourceSpawner.cs
    ResourceStorageContainer.cs
    ResourceStorageProvider.cs
    ResourceVisualBuilder.cs
    VehicleFactory.cs
    WeaponsFactory.cs

    ------------------------------------------------------------------------
    CIVILIAN SYSTEM
    ------------------------------------------------------------------------
    Civilian.cs
    CivilianRole.cs
    CivilianSpawner.cs
    JobManager.cs
    SkillType.cs

    ------------------------------------------------------------------------
    COMBAT SYSTEM
    ------------------------------------------------------------------------
    Attackable.cs
    AutoDestroyFX.cs
    CombatManager.cs
    CombatStance.cs
    DiplomacyManager.cs
    IAttackable.cs
    ICommandable.cs
    IHasHealth.cs
    ITargetable.cs
    Projectile.cs
    ProjectilePool.cs
    SkirmishConfig.cs
    TurretProjectile.cs
    TurretProjectilePool.cs
    Unit.cs
    UnitCombatController.cs
    UnitCommandController.cs
    UnitManager.cs
    UnitProductionDefinition.cs
    UnitProductionQueue.cs
    WeaponComponent.cs

    ------------------------------------------------------------------------
    CORE SYSTEM
    ------------------------------------------------------------------------
    AlertManager.cs
    CharacterStats.cs
    EventManager.cs
    GameBootstrap.cs
    GameManager.cs
    ParentRenamer.cs
    ResourceDefinition.cs
    RTSCamera.cs
    SaveLoadManager.cs
    SciFiMapBootstrap.cs
    SciFiTeamStyler.cs
    Selectable.cs
    SelfRenamer.cs
    SixTeamBootstrap.cs
    Spawner.cs
    Team.cs
    TeamColorManager.cs
    TeamColorUtils.cs
    TeamInventory.cs
    TeamResources.cs
    TeamStorageManager.cs
    TeamVisual.cs
    WinConditionManager.cs

    ------------------------------------------------------------------------
    DEBUG
    ------------------------------------------------------------------------
    FindTeamDuplicates.cs
    TeamWatchdog.cs

    ------------------------------------------------------------------------
    DOCUMENTS (Design-Only)
    ------------------------------------------------------------------------
    CombatSystem.cs
    HQSpawner.cs

    ------------------------------------------------------------------------
    UI SYSTEM
    ------------------------------------------------------------------------
    BuildMenuUI.cs
    ConstructionManager.cs
    EconomyUI.cs
    IMGUIInputBlocker.cs
    MainMenuUI.cs
    MapVisualsBootstrap.cs
    Minimap.cs
    NeonRing.cs
    SelectionManager.cs
    SelectionRing.cs
    TaskBoardUI.cs
    TeamBootstrap.cs
    UnitInspectorUI.cs
    WorldHealthBar.cs

    */
}