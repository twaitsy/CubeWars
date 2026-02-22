using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingCategory
{
    Housing,
    Production,
    Storage,
    Military,
    Utility,
    Research,
    Farming,
    Mining,
    Logistics,
    Decoration,
    Other
}

[Serializable]
public class ResourceAmount
{
    public ResourceDefinition resource;
    public int amount;
}

[Serializable]
public class StorageSettings
{
    public int capacity = 0;
    public List<ResourceCategory> allowedCategories = new();
}

[Serializable]
public class BuildingDefinition
{
    // Identity
    public string id;                // <-- added for stationId lookups
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    // Classification
    public BuildingCategory category;
    public string subCategory;

    // Construction
    public List<ResourceAmount> constructionCost = new();
    public float buildTime = 5f;
    public int maxHealth = 100;

    // Upkeep
    public List<ResourceAmount> upkeepCost = new();
    public float upkeepInterval = 10f;

    // Housing
    public int maxResidents = 0;
    public int comfortLevel = 0;

    // Production (now references recipe definitions)
    public bool isProducer = false;
    public List<ProductionRecipeDefinition> recipes = new();
    public int workerSlots = 0;

    // Added for BonusCalculator / production systems
    public int productionQueueSize = 1;
    public float productionSpeedMultiplier = 1f;

    // Storage
    public bool isStorage = false;
    public StorageSettings storageSettings;

    // Military
    public bool isMilitary = false;
    public int attackDamage = 0;
    public float attackRange = 0f;
    public int garrisonSlots = 0;

    // Utility
    public int powerConsumption = 0;
    public int powerGeneration = 0;
    public int waterConsumption = 0;

    // Upgrades
    public BuildingDefinition upgradeTo;
}