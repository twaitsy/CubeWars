// ============================================================================
// BuildItemDefinition.cs
//
// PURPOSE:
// - Defines a buildable structure in Cube Wars.
// - Acts as the metadata container for placement, cost, category, AI priority,
//   and prefab reference.
// - Used by UI, AI, construction, and placement systems.
//
// DEPENDENCIES:
// - BuildPlacementManager:
//      * Uses prefab, yOffset, placementOffset, costs.
//      * Uses category + aiPriority for AI building logic.
// - ConstructionSite:
//      * Reads costs and buildTime.
//      * Spawns final building prefab.
// - BuildMenuUI:
//      * Displays displayName, icon, costs, category.
//      * Uses category for grouping.
// - BuildCatalog:
//      * Stores BuildItemDefinition assets.
// - AIBuilder / TeamAIBuild / AIRebuildManager:
//      * Uses category + aiPriority to decide what to build.
// - ResourceCost / ResourceType:
//      * Defines the cost of constructing this building.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new AI building logic, ensure category and aiPriority match AI expectations.
// - If you add building upgrades, consider linking BuildItemDefinition → UpgradeDefinition.
// - If you add multi-cell buildings, add footprint metadata here.
// - If you add rotation-based placement, add rotation rules here.
// - If you add tech-tree requirements, add prerequisites here.
//
// INSPECTOR REQUIREMENTS:
// - displayName: shown in UI.
// - icon: shown in UI.
// - prefab: MUST contain a Building component or ConstructionSite will fail.
// - costs: resource requirements.
// - buildTime: override for ConstructionSite.
// - category: used by UI + AI.
// - aiPriority: used by AI building logic.
// ============================================================================

using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Build Item", fileName = "BuildItem_")]
public class BuildItemDefinition : ScriptableObject
{
    [Header("UI")]
    public string displayName;
    public Sprite icon;

    [Header("Placement")]
    public GameObject prefab;

    [Tooltip("Vertical offset when placing the building.")]
    public float yOffset = 0f;

    [Tooltip("Optional extra placement offset (X/Z). Y is usually driven by yOffset.")]
    public Vector3 placementOffset = Vector3.zero;

    [Header("Cost")]
    public ResourceCost[] costs;

    [Header("Build Time")]
    [Tooltip("Build time in seconds for ConstructionSite. If <= 0, site uses its own baseBuildTime.")]
    public float buildTime = 0f;

    [Header("AI / Category")]
    [Tooltip("High-level category used by AI (e.g. Economy, Industry, Housing, Tech).")]
    public string category = "Economy";

    public AIBuildingPriority aiPriority = AIBuildingPriority.Economy;
}