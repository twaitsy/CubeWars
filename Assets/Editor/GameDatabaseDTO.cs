// GameDatabaseDTO.cs
using System.Collections.Generic;

public class GameDatabaseDTO
{
    public JobsDatabaseDTO jobs = new JobsDatabaseDTO();
    public ToolsDatabaseDTO tools = new ToolsDatabaseDTO();
    public BuildingsDatabaseDTO buildings = new BuildingsDatabaseDTO();
    public ResourcesDatabaseDTO resources = new ResourcesDatabaseDTO();
    public UnitsDatabaseDTO units = new UnitsDatabaseDTO();
    public RecipesDatabaseDTO recipes = new RecipesDatabaseDTO();
    public FoodDatabaseDTO foods = new FoodDatabaseDTO();
    public TechTreeDatabaseDTO techTree = new TechTreeDatabaseDTO();
}

// ---------------- TECH TREE ----------------

public class TechTreeDatabaseDTO
{
    public List<TechNodeDTO> techNodes = new List<TechNodeDTO>();
}

public class TechNodeDTO
{
    public string id;
    public string displayName;
    public string description;

    public List<ResourceAmountDTO> researchCost = new List<ResourceAmountDTO>();
    public float researchTime;

    public List<string> prerequisiteIds = new List<string>();
    public string requiredBuildingId;

    public List<string> unlockBuildingIds = new List<string>();
    public List<string> unlockUnitIds = new List<string>();
    public List<string> unlockRecipeIds = new List<string>();

    public float globalProductionSpeedMultiplier;
    public float globalCombatDamageMultiplier;
    public float workerEfficiencyMultiplier;
}

// ---------------- RESOURCES ----------------

public class ResourcesDatabaseDTO
{
    public List<ResourceDefinitionDTO> resources = new List<ResourceDefinitionDTO>();
}

public class ResourceDefinitionDTO
{
    public string id;
    public string displayName;
    public string category;
    public float weight;
    public int baseValue;
}

public class ResourceAmountDTO
{
    public string resourceId;
    public int amount;
}

// ---------------- UNITS ----------------

public class UnitsDatabaseDTO
{
    public List<UnitDefinitionDTO> units = new List<UnitDefinitionDTO>();
}

public class UnitDefinitionDTO
{
    public string id;
    public string displayName;

    public string jobType;
    public bool isCombatUnit;

    public int maxHealth;
    public float moveSpeed;

    public int attackDamage;
    public float attackRange;
    public float attackCooldown;
    public int armor;

    public int carryCapacity;
    public float gatherSpeed;
    public float buildSpeed;

    public List<ResourceAmountDTO> trainingCost = new List<ResourceAmountDTO>();
    public float trainingTime;
    public string trainedAtBuildingId;

    public string upgradeToUnitId;
}

// ---------------- BUILDINGS ----------------

public class BuildingsDatabaseDTO
{
    public List<BuildingDefinitionDTO> buildings = new List<BuildingDefinitionDTO>();
}

public class BuildingDefinitionDTO
{
    public string id;
    public string displayName;

    public string category;
    public string subCategory;

    public List<ResourceAmountDTO> constructionCost = new List<ResourceAmountDTO>();
    public float buildTime;
    public int maxHealth;

    public List<ResourceAmountDTO> upkeepCost = new List<ResourceAmountDTO>();
    public float upkeepInterval;

    public int maxResidents;
    public int comfortLevel;

    public bool isProducer;
    public List<string> recipeIds = new List<string>();
    public int workerSlots;

    public bool isStorage;
    public int storageCapacity;
    public List<string> allowedStorageCategories = new List<string>();

    public bool isMilitary;
    public int attackDamage;
    public float attackRange;
    public int garrisonSlots;

    public int powerConsumption;
    public int powerGeneration;
    public int waterConsumption;

    public string upgradeToBuildingId;
}

// ---------------- JOBS ----------------

public class JobsDatabaseDTO
{
    public List<JobDefinitionDTO> jobs = new List<JobDefinitionDTO>();
}

public class JobDefinitionDTO
{
    public string id;
    public string displayName;
    public List<string> requiredToolIds = new List<string>();
}

// ---------------- TOOLS ----------------

public class ToolsDatabaseDTO
{
    public List<ToolDefinitionDTO> tools = new List<ToolDefinitionDTO>();
}

public class ToolDefinitionDTO
{
    public string id;
    public string displayName;
    public int durability;
    public float efficiency;
}

// ---------------- RECIPES ----------------

public class RecipesDatabaseDTO
{
    public List<ProductionRecipeDTO> recipes = new List<ProductionRecipeDTO>();
}

public class ProductionRecipeDTO
{
    public string id;
    public string recipeName;

    public List<ResourceAmountDTO> inputs = new List<ResourceAmountDTO>();
    public List<ResourceAmountDTO> outputs = new List<ResourceAmountDTO>();

    public float craftTimeSeconds;
    public int batchSize;

    public string requiredJobType;

    public float outputEfficiencyMultiplier;
    public float inputEfficiencyMultiplier;

    public bool requiresPower;
    public bool requiresFuel;
}

// ---------------- FOOD ----------------

public class FoodDatabaseDTO
{
    public List<FoodDefinitionDTO> foods = new List<FoodDefinitionDTO>();
}

public class FoodDefinitionDTO
{
    public string resourceId;
    public int hungerRestore;
    public float eatTime;
    public bool requiresCooking;
    public bool perishable;
    public float spoilTime;
}