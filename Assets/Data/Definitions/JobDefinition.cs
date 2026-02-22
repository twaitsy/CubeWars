using System;
using System.Collections.Generic;
using UnityEngine;

public enum JobCategory
{
    Gathering,
    Crafting,
    Farming,
    Building,
    Logistics,
    CombatSupport,
    Research,
    Utility,
    Other
}

[Serializable]
public class JobDefinition
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;
    [TextArea] public string description;

    public CivilianJobType jobType;
    public JobCategory category = JobCategory.Gathering;
    public int tier = 1;
    public List<string> roleTags = new(); // e.g. "woodcutting", "mining", "smithing"

    [Header("Skill & XP")]
    public float baseSkill = 1f;
    public float skillPerLevel = 0.1f;
    public float xpGainRate = 1f;
    public AnimationCurve xpCurve = AnimationCurve.Linear(0, 1, 1, 2);
    public float jobDifficulty = 1f;

    [Header("Performance Multipliers")]
    public float jobSpeedMultiplier = 1f;
    public float jobQualityMultiplier = 1f;
    public float fatigueMultiplier = 1f;
    public float accuracyMultiplier = 1f;
    public float failureChance = 0f;

    [Header("Tools & Resources")]
    public List<ToolDefinition> allowedTools = new();
    public List<ToolDefinition> preferredTools = new();
    public List<ToolDefinition> forbiddenTools = new();
    public List<ResourceCategory> resourceCategoriesWorked = new();
    public List<BonusDefinition> jobBonuses = new(); // job-specific bonuses

    [Header("Buildings & Recipes")]
    public List<BuildingDefinition> requiredBuildings = new();
    public List<BuildingDefinition> allowedBuildings = new();
    public List<ProductionRecipeDefinition> allowedRecipes = new();
    public List<string> jobStations = new(); // e.g. "Anvil", "Workbench"

    [Header("AI Behaviour")]
    public float aiPriority = 1f;
    public List<string> aiRoleTags = new(); // e.g. "eco", "industry", "military"
    public int aiMaxWorkers = 0; // 0 = unlimited

    [Header("Animation & Audio")]
    public string animationSet;
    public float workAnimationSpeed = 1f;
    public string soundSet;
}