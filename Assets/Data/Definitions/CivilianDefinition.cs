using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CivilianDefinition
{
    [Header("1. Core Identity & Classification")]
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
    public int faction;
    public UnitClassType unitClass = UnitClassType.Civilian;
    public int tier = 1;
    public bool isCombatUnit;
    public bool isWorker = true;
    public bool isCivilian = true;
    public bool isMechanical;
    public bool isBiological = true;
    public UnitSizeCategory sizeCategory = UnitSizeCategory.Medium;
    public int populationCost = 1;
    public List<ResourceAmount> upkeepCost = new();

    [Header("2. Movement & Navigation Stats")]
    public float moveSpeed = 2.8f;
    public float acceleration = 6f;
    public float deceleration = 6f;
    public float turnSpeed = 300f;
    public float rotationAcceleration = 450f;
    public float pathRecalculationRate = 0.35f;
    public int avoidancePriority = 45;
    public List<StringFloatStat> terrainSpeedMultipliers = new();
    public float waterMovementPenalty = 0.7f;
    public float slopePenalty = 0.85f;
    public float stuckDetectionTime = 4f;
    public float fleeSpeedMultiplier = 1.35f;
    public float carryingSpeedPenalty = 0.85f;

    [Header("3. Health, Survival & Resistances")]
    public int maxHealth = 60;
    public float healthRegenRate = 0.2f;
    public int armor = 1;
    public ArmorType armorType = ArmorType.Light;
    public List<StringFloatStat> damageResistance = new();
    public List<StringFloatStat> vulnerability = new();
    public float fireResistance;
    public float coldResistance;
    public float poisonResistance;
    public float radiationResistance;
    public float diseaseResistance;
    public float bleedingRate;
    public float reviveTime = 6f;
    public float deathDelay = 0.25f;

    [Header("4. Combat Stats")]
    public int attackDamage = 2;
    public AttackType attackType = AttackType.Melee;
    public float attackRange = 1.2f;
    public float attackWindup = 0.2f;
    public float attackRecovery = 0.2f;
    public float attackCooldown = 1.2f;
    public float projectileSpeed;
    public float projectileArc;
    public float splashRadius;
    public float splashFalloff = 1f;
    public float accuracy = 0.85f;
    public float critChance;
    public float critMultiplier = 1.5f;
    public float knockbackForce;
    public int threatPriority = 1;
    public float aggroRange = 5f;
    public float chaseRange = 7f;
    public float fleeThreshold = 0.3f;
    public float morale = 1f;
    public float moraleBreakChance = 0.05f;

    [Header("5. Worker / Economy Stats")]
    public int carryCapacity = 30;
    public float gatherSpeed = 1f;
    public float gatherYieldMultiplier = 1f;
    public float gatherRange = 1.5f;
    public float buildSpeed = 1f;
    public float repairSpeed = 1f;
    public float haulSpeedMultiplier = 1f;
    public float refineEfficiency = 1f;
    public float craftingSpeed = 1f;
    public float craftingQuality = 1f;
    public float taskSwitchPenalty = 0.1f;
    public float workStaminaCost = 1f;
    public float resourceDetectionRange = 14f;
    public List<StringFloatStat> resourcePreference = new();
    public float toolEffectivenessMultiplier = 1f;
    public float workAccuracy = 1f;

    [Header("6. Needs & Life Simulation Stats")]
    public float needDecayMultiplier = 1f;
    public float needRecoveryMultiplier = 1f;
    public float hungerRate = 1f;
    public float thirstRate = 1f;
    public float sleepRate = 1f;
    public float entertainmentRate = 1f;
    public float socialRate = 1f;
    public float hygieneRate = 1f;
    public float toiletRate = 1f;
    public float coffeeDependence;
    public float stressResistance = 1f;
    public float comfortPreference = 0.5f;
    public float lonelinessThreshold = 0.6f;
    public float boredomThreshold = 0.5f;
    public float panicThreshold = 0.8f;

    [Header("7. Personality & Behaviour Stats")]
    public float bravery = 0.5f;
    public float discipline = 0.5f;
    public float sociability = 0.5f;
    public float curiosity = 0.5f;
    public float aggression = 0.2f;
    public float riskTolerance = 0.4f;
    public float stubbornness = 0.3f;
    public float leadershipScore = 0.2f;
    public float loyalty = 0.8f;
    public float conflictLikelihood = 0.2f;
    public IdleBehaviourType idleBehaviourType = IdleBehaviourType.Wander;
    public List<string> preferredJobs = new();
    public List<string> dislikedJobs = new();
    public List<string> preferredNeeds = new();

    [Header("8. Job / Profession Stats")]
    public CivilianJobType jobType = CivilianJobType.Generalist;
    public string jobTypeId;
    public List<string> allowedJobs = new();
    public List<StringFloatStat> jobSpeedMultipliers = new();
    public JobTrainingLevel trainingLevel = JobTrainingLevel.Novice;
    public float xpGainRate = 1f;
    public float xpRequiredForNextLevel = 100f;
    public float jobSuccessChance = 1f;
    public float jobFailurePenalty;
    public float multitaskEfficiency = 1f;

    [Header("9. Environmental Interaction Stats")]
    public float heatTolerance = 1f;
    public float coldTolerance = 1f;
    public float wetnessTolerance = 1f;
    public float windResistance = 1f;
    public float radiationTolerance = 1f;
    public float oxygenConsumption = 1f;
    public float drowningTime = 20f;
    public float sunExposureRate = 1f;
    public float shadeRecoveryRate = 1f;
    public List<StringFloatStat> biomeAffinity = new();

    [Header("10. Social Interaction Stats")]
    public float friendshipGainRate = 1f;
    public float romanceChance = 0.05f;
    public float conflictChance = 0.08f;
    public float socialNeedMultiplier = 1f;
    public float groupCohesion = 0.5f;
    public float leadershipInfluence = 0.3f;
    public float gossipLikelihood = 0.2f;
    public float mentoringEffectiveness = 0.4f;

    [Header("11. Training, Upgrades & Progression")]
    public List<ResourceAmount> trainingCost = new();
    public float trainingTime;
    public BuildingDefinition trainedAt;
    public string upgradeTo;
    public List<ResourceAmount> upgradeCost = new();
    public float upgradeTime;
    public float experience;
    public int level = 1;
    public List<LevelBonusDefinition> levelUpBonuses = new();
    public float veterancyDamageBonus;
    public float veterancySpeedBonus;

    [Header("12. Animation & Visual Stats")]
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

    [Header("13. AI Control & State Machine Tuning")]
    public float decisionFrequency = 0.3f;
    public float retargetFrequency = 0.25f;
    public int maxConcurrentTasks = 2;
    public float taskRetryDelay = 1f;
    public float panicDuration = 4f;
    public float searchRadius = 18f;
    public float searchRetrySeconds = 2f;
    public float stuckTimeout = 6f;
    public Vector3 priorityBias = new(0.4f, 0.45f, 0.15f);
    public float fleeDecisionCooldown = 2f;
    public float targetMemoryDuration = 5f;

    [Header("14. Meta / Game Design Stats")]
    public float costMultiplier = 1f;
    public float upkeepMultiplier = 1f;
    public float difficultyScalingFactor = 1f;
    public float spawnWeight = 1f;
    public RarityType rarity = RarityType.Common;
    public float aiRecruitmentWeight = 1f;
    public float playerRecruitmentWeight = 1f;

    [Header("Compatibility")]
    public ToolDefinition[] startingTools;
    public List<NeedDefinition> needs;
}
