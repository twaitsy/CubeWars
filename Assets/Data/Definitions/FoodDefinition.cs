using System;
using System.Collections.Generic;
using UnityEngine;

public enum FoodCategory
{
    Raw,
    Cooked,
    Luxury,
    Drink,
    Preserved,
    Ingredient,
    Other
}

[Serializable]
public class FoodDefinition
{
    [Header("Identity")]
    public ResourceDefinition resource;
    public FoodCategory category = FoodCategory.Raw;
    public int qualityTier = 1;
    public List<string> tags = new(); // e.g. "meat", "vegetable", "grain"

    [Header("Nutrition & Needs")]
    [Min(1)] public int hungerRestore = 1;
    public float hydrationRestore = 0f;
    public float moraleBonus = 0f;
    public float comfortBonus = 0f;
    public float energyRestore = 0f; // affects fatigue
    public float diseaseRisk = 0f;   // raw meat, spoiled food

    [Header("Consumption")]
    [Min(0.1f)] public float eatTime = 1f;
    public bool requiresCooking;
    public bool requiresSeasoning;
    public ProductionRecipeDefinition cookingRecipe;

    [Header("Spoilage & Storage")]
    public bool perishable = false;
    [Min(0f)] public float spoilTime = 0f;
    public float spoilRate = 1f;
    public float temperatureSensitivity = 1f;
    public string storageCategory = "Ambient"; // Cold, Dry, Ambient
    public ResourceDefinition spoilByproduct;  // e.g. spoiled food → compost

    [Header("Buffs & Bonuses")]
    public List<BonusDefinition> temporaryBonuses = new();
    public float bonusDurationSeconds = 0f;

    [Header("AI Behaviour")]
    public float aiFoodPriority = 1f;
    public int aiStockpileTarget = 0;
    public List<CivilianJobType> preferredByJobs = new();
}