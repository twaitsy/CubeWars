using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Raw,
    Refined,
    Crafted,
    Organic,
    Fuel,
    Luxury,
    Utility,
    Other
}

[Serializable]
public class ResourceDefinition
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public Sprite icon;

    public ResourceCategory category;
    public string subCategory;
    public ResourceType resourceType = ResourceType.Raw;
    public int tier = 1;
    public int techTier = 1;
    public int qualityTier = 1;
    public RarityType rarity = RarityType.Common;
    public List<string> tags = new(); // e.g. "metal", "wood", "food"

    [Header("Physical Properties")]
    public float weight = 1f;
    public float volume = 1f;
    public int stackSize = 100;
    public bool isLiquid = false;
    public bool isGas = false;

    [Header("Perishability & Storage")]
    public bool isPerishable = false;
    public float decayRate = 0f;
    public float temperatureRequirement = 0f;
    public float spoilageModifier = 1f;
    public float flammability = 0f;

    [Header("Gathering")]
    public float gatherDifficulty = 1f;
    public ToolDefinition gatherToolRequired;
    public ToolCategory gatherToolCategory;
    public float gatherTime = 1f;
    public float gatherYieldMultiplier = 1f;
    public int gatherXpReward = 1;
    public List<string> biomeAvailability = new(); // e.g. "Forest", "Mountain"
    public float hazardLevel = 0f;

    [Header("Refining & Crafting")]
    public List<ResourceAmount> refinesInto = new();
    public float refineEfficiency = 1f;
    public float refineTime = 1f;
    public BuildingDefinition refineStationRequired;
    public int refineXpReward = 1;
    public float refineDifficulty = 1f;
    public List<ResourceAmount> refineByproducts = new();

    [Header("Fuel Properties")]
    public bool isFuel = false;
    public float burnValue = 0f;
    public float burnTemperature = 0f;

    [Header("Economy")]
    public int baseValue = 1;
    public int marketValue = 1;
    public string tradeCategory = "General";
    public float aiDesirability = 1f;
    public int aiStockpileTarget = 0;

    [Header("Consumption")]
    public bool isConsumable = false;
    public float nutritionValue = 0f;
    public float hydrationValue = 0f;
    public float populationConsumptionRate = 0f;
}