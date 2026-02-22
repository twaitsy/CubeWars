using System;
using System.Collections.Generic;
using UnityEngine;

public enum CivilianCategory
{
    Worker,
    Specialist,
    Scholar,
    Noble,
    Soldier,
    Other
}

public enum AIPersonalityType
{
    Balanced,
    Industrious,
    Social,
    Cautious,
    Brave,
    Curious,
    Stubborn
}

[Serializable]
public class CivilianDefinition
{
    // ---------------------------------------------------------
    // Identity
    // ---------------------------------------------------------
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    public CivilianCategory category = CivilianCategory.Worker;
    public int faction = -1; // -1 = any faction
    public List<string> roleTags = new(); // e.g. "farmer", "miner", "scholar"

    // ---------------------------------------------------------
    // Personality
    // ---------------------------------------------------------
    public AIPersonalityType personalityType = AIPersonalityType.Balanced;

    [Range(0f, 1f)] public float discipline = 0.5f;
    [Range(0f, 1f)] public float curiosity = 0.5f;
    [Range(0f, 1f)] public float stubbornness = 0.5f;
    [Range(0f, 1f)] public float bravery = 0.5f;
    [Range(0f, 1f)] public float sociability = 0.5f;

    // ---------------------------------------------------------
    // Core Stats
    // ---------------------------------------------------------
    public float maxHealth = 100f;
    public float moveSpeed = 3.5f;
    public int carryCapacity = 10;
    public float gatherSpeed = 1f;
    public float buildSpeed = 1f;

    public float morale = 1f;
    public float moraleRecoveryRate = 0.1f;
    public float fatigue = 0f;
    public float fatigueRecoveryRate = 0.2f;

    // ---------------------------------------------------------
    // Needs Integration
    // ---------------------------------------------------------
    public List<NeedDefinition> needs = new();
    public List<float> needDecayMultipliers = new(); // parallel to needs list
    public List<float> needPriorityOverrides = new(); // optional

    public List<FoodDefinition> preferredFoods = new();
    public List<BuildingDefinition> preferredBuildings = new();

    // ---------------------------------------------------------
    // Jobs & Skills
    // ---------------------------------------------------------
    public CivilianJobType jobType = CivilianJobType.Generalist;
    public JobTrainingLevel trainingLevel = JobTrainingLevel.Novice;

    public List<JobDefinition> allowedJobs = new();
    public List<JobDefinition> preferredJobs = new();
    public List<JobDefinition> forbiddenJobs = new();

    public float jobAffinityMultiplier = 1f;
    public float jobXPMultiplier = 1f;

    public float baseWorkSpeed = 1f;
    public float jobSpeedMultiplier = 1f;
    public float jobQualityMultiplier = 1f;

    public AnimationCurve fatiguePenaltyCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);
    public AnimationCurve moraleEffectCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);

    // ---------------------------------------------------------
    // Tools & Equipment
    // ---------------------------------------------------------
    public float toolEffectivenessMultiplier = 1f;
    public List<ToolDefinition> startingTools = new();
    public List<ToolDefinition> allowedTools = new();
    public List<ToolDefinition> preferredTools = new();

    // ---------------------------------------------------------
    // Work Behaviour
    // ---------------------------------------------------------
    public float workAccuracy = 1f;
    public float multitaskEfficiency = 1f;

    // ---------------------------------------------------------
    // AI Behaviour
    // ---------------------------------------------------------
    public float aiDecisionFrequency = 0.25f;
    public float aiWanderRadius = 10f;
    public float aiFleeThreshold = 0.2f;
    public float aiMoraleBreakChance = 0.1f;
    public float aiSearchRetrySeconds = 2f;
}
