using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitDefinition
{
    [Header("1. Core Identity")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public int faction;
    public UnitClassType unitClass = UnitClassType.Soldier;
    public int tier = 1;
    public int techTier = 1;
    public bool isMechanical;
    public bool isBiological = true;
    public UnitSizeCategory sizeCategory = UnitSizeCategory.Medium;
    public int populationCost = 1;
    public List<ResourceAmount> upkeepCost = new();
    public List<string> roleTags = new(); // e.g. "frontline", "ranged", "siege"

    [Header("2. Movement")]
    public float moveSpeed = 3.5f;
    public float acceleration = 8f;
    public float deceleration = 8f;
    public float turnSpeed = 360f;
    public float rotationAcceleration = 540f;
    public float pathRecalculationRate = 0.3f;
    public int avoidancePriority = 50;
    public List<StringFloatStat> terrainSpeedMultipliers = new();
    public float waterMovementPenalty = 1f;
    public float slopePenalty = 1f;
    public float stuckDetectionTime = 3f;
    public bool canSwim;
    public bool canClimb;
    public bool canFly;
    public bool canBurrow;
    public float formationSpacing = 1f;

    [Header("3. Health & Resistances")]
    public int maxHealth = 100;
    public float healthRegenRate = 0f;
    public int armor = 0;
    public ArmorType armorType = ArmorType.Light;
    public List<StringFloatStat> damageResistance = new();
    public List<StringFloatStat> vulnerability = new();
    public float fireResistance;
    public float coldResistance;
    public float poisonResistance;
    public float radiationResistance;
    public float diseaseResistance;
    public float bleedingRate;
    public float reviveTime = 8f;
    public float deathDelay = 0f;
    public int shieldValue;
    public float shieldRegenRate;

    [Header("4. Combat Stats")]
    public int attackDamage = 10;
    public AttackType attackType = AttackType.Melee;
    public float attackRange = 1.5f;
    public float attackWindup = 0.1f;
    public float attackRecovery = 0.1f;
    public float attackCooldown = 1f;
    public float projectileSpeed;
    public float projectileArc;
    public float splashRadius;
    public float splashFalloff = 1f;
    public float accuracy = 1f;
    public float critChance;
    public float critMultiplier = 1.5f;
    public float knockbackForce;
    public int threatPriority = 1;
    public float aggroRange = 8f;
    public float chaseRange = 12f;
    public float fleeThreshold = 0.15f;
    public float morale = 1f;
    public float moraleBreakChance;
    public List<UnitClassType> preferredTargets = new();
    public List<BonusDefinition> attackModifiers = new();

    [Header("5. Combat AI")]
    public float decisionFrequency = 0.25f;
    public float retargetFrequency = 0.2f;
    public int maxConcurrentTasks = 1;
    public float taskRetryDelay = 1f;
    public float panicDuration = 4f;
    public float searchRadius = 15f;
    public float searchRetrySeconds = 2f;
    public float stuckTimeout = 5f;
    public Vector3 priorityBias = new(0.2f, 0.4f, 0.4f);
    public float fleeDecisionCooldown = 2f;
    public float targetMemoryDuration = 5f;
    public string stance = "Aggressive"; // Aggressive, Defensive, Hold
    public List<string> aiTargetPriorityTags = new();

    [Header("6. Training & Upgrades")]
    public List<ResourceAmount> trainingCost = new();
    public float trainingTime = 5f;
    public BuildingDefinition trainedAt;
    public TechNodeDefinition requiredTech;
    public List<ResourceAmount> upgradeCost = new();
    public float upgradeTime = 5f;
    public UnitDefinition upgradeTo;
    public float experience;
    public int level = 1;
    public List<LevelBonusDefinition> levelUpBonuses = new();
    public float veterancyDamageBonus;
    public float veterancySpeedBonus;
    public List<ToolDefinition> startingTools = new();
    public List<ResourceAmount> startingInventory = new();

    [Header("7. Animation & Visuals")]
    public string animationSet;
    public float modelScale = 1f;
    public List<string> idleAnimationVariants = new();
    public float walkAnimationSpeed = 1f;
    public float attackAnimationSpeed = 1f;
    public string footstepType;
    public string voiceSet;
    public Sprite portrait;
    public List<Color> colorPalette = new();
    public List<string> accessorySet = new();
    public AnimationClip deathAnimation;
    public AnimationClip hitAnimation;

    [Header("8. Meta")]
    public float costMultiplier = 1f;
    public float upkeepMultiplier = 1f;
    public float difficultyScalingFactor = 1f;
    public float spawnWeight = 1f;
    public RarityType rarity = RarityType.Common;
    public float aiRecruitmentWeight = 1f;
    public float playerRecruitmentWeight = 1f;
    public List<string> aiRoleTags = new(); // e.g. "frontline", "ranged", "siege"
    public List<string> aiWeaknessTags = new();
    public List<string> aiCounterTags = new();
}