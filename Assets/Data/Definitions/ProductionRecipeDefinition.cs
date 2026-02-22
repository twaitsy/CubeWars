using System;
using System.Collections.Generic;
using UnityEngine;

public enum RecipeCategory
{
    Crafting,
    Refining,
    Cooking,
    Smelting,
    Construction,
    Alchemy,
    Utility,
    Other
}

public enum BatchScalingMode
{
    Linear,
    Exponential,
    Fixed
}

[Serializable]
public class ProductionRecipeDefinition
{
    [Header("Identity")]
    public string recipeId;
    public string recipeName = "New Recipe";
    [TextArea] public string description;
    public RecipeCategory category = RecipeCategory.Crafting;
    public List<string> tags = new(); // e.g. "metalworking", "cooking"
    public int techTier = 1;

    [Header("Inputs & Outputs")]
    public RecipeResourceAmount[] inputs;
    public RecipeResourceAmount[] outputs;
    public List<ResourceAmount> byproducts = new();
    public float byproductChance = 1f;

    [Header("Batching")]
    public float craftTimeSeconds = 5f;
    public int batchSize = 1;
    public BatchScalingMode batchScalingMode = BatchScalingMode.Linear;
    public float batchEfficiency = 1f;

    [Header("Job Requirements")]
    public CivilianJobType requiredJobType = CivilianJobType.Generalist;
    public JobTrainingLevel requiredSkillLevel = JobTrainingLevel.Novice;
    public JobDefinition requiredJob;
    public float jobDifficulty = 1f;
    public int xpReward = 1;

    [Header("Tool Requirements")]
    public List<ToolDefinition> requiredTools = new();
    public List<ToolDefinition> forbiddenTools = new();
    public int minimumToolTier = 0;

    [Header("Building & Station Requirements")]
    public BuildingDefinition requiredBuilding;
    public string requiredStationId; // e.g. "Anvil", "Forge", "Kitchen"
    public float stationEfficiencyMultiplier = 1f;

    [Header("Power & Fuel")]
    public bool requiresPower;
    public float powerCost = 0f;
    public bool requiresFuel;
    public ResourceAmount fuelCost;
    public float heatRequirement = 0f;

    [Header("Environment")]
    public float temperatureSensitivity = 1f;
    public float weatherPenaltyMultiplier = 1f;

    [Header("Quality & Efficiency")]
    public float inputEfficiencyMultiplier = 1f;
    public float outputEfficiencyMultiplier = 1f;
    public bool inputQualityAffectsOutput = false;
    public float outputQualityMultiplier = 1f;
    public float workerSpeedMultiplier = 1f;
    public float toolEfficiencyMultiplier = 1f;
    public float buildingEfficiencyMultiplier = 1f;

    [Header("Tech Modifiers")]
    public List<TechNodeDefinition> techCostModifiers = new();
    public List<TechNodeDefinition> techSpeedModifiers = new();

    [Header("AI Behaviour")]
    public float aiPriority = 1f;
    public float aiEconomicWeight = 1f;
    public string aiProductionRole = "general"; // e.g. "military", "economy"
    public int aiBatchPreference = 1;

    [Header("Animation & Audio")]
    public string animationSet;
    public string soundSet;
    public GameObject vfxPrefab;
}
