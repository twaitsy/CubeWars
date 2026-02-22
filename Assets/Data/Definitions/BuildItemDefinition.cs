using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildMenuCategory
{
    Economy,
    Industry,
    Housing,
    Military,
    Defense,
    Farming,
    Logistics,
    Research,
    Utility,
    Decoration,
    Other
}

public enum BuildPlacementStrategy
{
    Default,
    ClusterNearSimilar,
    SpreadOut,
    PerimeterDefense,
    NearResources,
    NearHQ,
    RoadNetwork
}

public enum AIBuildTiming
{
    Early,
    Mid,
    Late
}

[CreateAssetMenu(menuName = "CubeWars/Build Item", fileName = "BuildItem_")]
public class BuildItemDefinition : ScriptableObject
{
    // ---------------------------------------------------------
    // Identity
    // ---------------------------------------------------------
    [Header("Identity")]
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    public BuildMenuCategory menuCategory = BuildMenuCategory.Economy;
    public int techTier = 1;
    public List<string> tags = new(); // e.g. "eco", "defense", "housing"
    public KeyCode hotkey = KeyCode.None;

    // -1 = any faction, otherwise restrict to specific faction
    public int factionRestriction = -1;

    // ---------------------------------------------------------
    // Tech Requirements
    // ---------------------------------------------------------
    [Header("Tech Requirements")]
    public List<TechNodeDefinition> requiredTech = new();
    public string[] prerequisites; // legacy string-based prereqs

    // ---------------------------------------------------------
    // Placement Rules
    // ---------------------------------------------------------
    [Header("Placement")]
    public GameObject prefab;
    public BuildingDefinition buildingDefinition;

    [Tooltip("Vertical offset when placing the building.")]
    public float yOffset = 0f;

    [Tooltip("Optional extra placement offset (X/Z). Y is usually driven by yOffset.")]
    public Vector3 placementOffset = Vector3.zero;

    [Tooltip("Footprint size in grid cells.")]
    public Vector2Int footprintSize = new(1, 1);

    [Tooltip("Whether the building must be placed on flat ground.")]
    public bool requiresFlatGround = true;

    [Tooltip("Whether the building can be placed on water.")]
    public bool canPlaceOnWater = false;

    [Tooltip("Whether the building must be adjacent to a road.")]
    public bool requiresRoadConnection = false;

    [Tooltip("Whether the building snaps to the grid.")]
    public bool snapToGrid = true;

    [Tooltip("Maximum allowed terrain slope for placement.")]
    public float slopeLimit = 30f;

    [Tooltip("Allowed terrain types (e.g. Grass, Sand, Rock). Leave empty for any.")]
    public List<string> terrainTypeRestrictions = new();

    [Tooltip("Minimum distance from another building of the same type.")]
    public float minDistanceFromSameType = 0f;

    [Tooltip("Maximum distance from a resource node (0 = ignore).")]
    public float maxDistanceFromResource = 0f;

    [Tooltip("Optional build radius for zoning or adjacency bonuses.")]
    public float buildRadius = 0f;

    [Tooltip("Zoning category (e.g. Residential, Industrial, Agricultural).")]
    public string zoningCategory = "General";

    // ---------------------------------------------------------
    // Construction
    // ---------------------------------------------------------
    [Header("Construction")]
    public ResourceCost[] costs;
    public float buildTime = 0f;

    [Tooltip("Percentage of resources refunded if construction is cancelled.")]
    [Range(0f, 1f)] public float cancelRefundPercent = 0.5f;

    [Tooltip("Percentage of resources refunded when demolishing the building.")]
    [Range(0f, 1f)] public float demolitionRefundPercent = 0.25f;

    [Tooltip("Multiplier applied to repair cost.")]
    public float repairCostMultiplier = 1f;

    [Tooltip("Job type required for construction.")]
    public CivilianJobType requiredJobType = CivilianJobType.Generalist;

    [Tooltip("Tools required for construction.")]
    public List<ToolDefinition> requiredTools = new();

    [Tooltip("How many workers can build this at once.")]
    public int workerCapacity = 1;

    [Tooltip("Optional construction VFX.")]
    public GameObject constructionVFX;

    [Tooltip("Optional construction sound.")]
    public AudioClip constructionSound;

    [Tooltip("Optional multi-stage construction visuals.")]
    public List<GameObject> constructionStages = new();

    // ---------------------------------------------------------
    // Power & Upkeep
    // ---------------------------------------------------------
    [Header("Power & Upkeep")]
    public float powerCost = 0f;
    public ResourceAmount[] fuelCost;
    public float upkeepInterval = 10f;

    // ---------------------------------------------------------
    // AI Logic
    // ---------------------------------------------------------
    [Header("AI Logic")]
    public AIBuildingPriority aiPriority = AIBuildingPriority.Economy;

    [Tooltip("Additional weighting for AI build decisions.")]
    public float aiWeight = 1f;

    [Tooltip("Tags describing the building's strategic role (e.g. 'eco', 'defense', 'tech').")]
    public string[] aiRoleTags;

    [Tooltip("Maximum number of this building the AI is allowed to build. 0 = unlimited.")]
    public int aiBuildLimit = 0;

    [Tooltip("When the AI should consider building this (Early, Mid, Late).")]
    public AIBuildTiming aiTiming = AIBuildTiming.Early;

    [Tooltip("How the AI should place this building.")]
    public BuildPlacementStrategy aiPlacementStrategy = BuildPlacementStrategy.Default;

    [Tooltip("Minimum distance from HQ for AI placement.")]
    public float aiMinDistanceFromHQ = 0f;

    [Tooltip("Maximum distance from HQ for AI placement.")]
    public float aiMaxDistanceFromHQ = 999f;
}