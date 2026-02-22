using System;
using System.Collections.Generic;
using UnityEngine;

public enum TechCategory
{
    Economy,
    Military,
    Industry,
    Farming,
    Research,
    Utility,
    Logistics,
    Social,
    Other
}

public enum TechTiming
{
    Early,
    Mid,
    Late
}

[Serializable]
public class TechNodeDefinition
{
    // Identity
    public string id;
    public string displayName;
    public Sprite icon;

    [TextArea]
    public string description;

    public TechCategory category = TechCategory.Economy;
    public int techTier = 1;
    public RarityType rarity = RarityType.Common;

    // Research cost
    public List<ResourceAmount> researchCost = new();
    public float researchTime = 10f;
    public float researchSpeedMultiplier = 1f;

    // Requirements
    public List<string> prerequisiteIds = new();
    public List<string> softPrerequisiteIds = new();
    public BuildingDefinition requiredBuilding;
    public int requiredPopulation = 0;
    public int requiredFactionLevel = 0;

    // Unlocks
    public List<BuildingDefinition> unlockBuildings = new();
    public List<UnitDefinition> unlockUnits = new();
    public List<ProductionRecipeDefinition> unlockRecipes = new();
    public List<ToolDefinition> unlockTools = new();
    public List<JobDefinition> unlockJobs = new();
    public List<BonusDefinition> unlockBonuses = new();

    // Global modifiers
    public float globalProductionSpeedMultiplier = 1f;
    public float globalCombatDamageMultiplier = 1f;
    public float globalWorkerEfficiencyMultiplier = 1f;
    public float globalGatherSpeedMultiplier = 1f;
    public float globalRefineEfficiencyMultiplier = 1f;
    public float globalBuildSpeedMultiplier = 1f;
    public float globalResearchSpeedMultiplier = 1f;
    public float globalMoraleMultiplier = 1f;
    public float globalMovementSpeedMultiplier = 1f;
    public float globalHealthMultiplier = 1f;
    public float globalArmorMultiplier = 1f;

    // AI behaviour
    public float aiResearchWeight = 1f;
    public TechTiming aiTiming = TechTiming.Early;
    public List<string> aiRoleTags = new();
}