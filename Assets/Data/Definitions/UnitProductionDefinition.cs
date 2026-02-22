using System;
using System.Collections.Generic;
using UnityEngine;

public enum UnitCategory
{
    Worker,
    Infantry,
    Ranged,
    Cavalry,
    Siege,
    Support,
    Specialist,
    Hero,
    Other
}

public enum UnitTrainingTiming
{
    Early,
    Mid,
    Late
}

[CreateAssetMenu(menuName = "CubeWars/Unit Production Definition")]
public class UnitProductionDefinition : ScriptableObject
{
    // Identity
    public string id;
    public string displayName;
    public Sprite icon;

    [TextArea]
    public string description;

    public UnitCategory category = UnitCategory.Infantry;
    public int techTier = 1;

    // Unit prefab
    public GameObject unitPrefab;

    // Production cost
    public ResourceCost[] costs;
    public float buildTime = 5f;

    // Additional costs
    public int populationCost = 1;
    public ResourceAmount[] upkeepCost;
    public float upkeepInterval = 10f;

    // Requirements
    public TechNodeDefinition requiredTech;
    public BuildingDefinition requiredBuilding;
    public int requiredFactionLevel = 0;

    // Production behavior
    public int queueSizeOverride = 0; // 0 = use building default
    public int parallelTrainingSlots = 1;
    public Vector3 spawnOffset = Vector3.zero;
    public AudioClip spawnSound;
    public AnimationClip spawnAnimation;

    // Starting equipment
    public List<ToolDefinition> startingTools = new();
    public List<ResourceAmount> startingInventory = new();
    public int startingVeterancy = 0;

    // AI logic
    public float aiProductionWeight = 1f;
    public UnitTrainingTiming aiTiming = UnitTrainingTiming.Early;
    public List<string> aiRoleTags = new(); // e.g. "frontline", "ranged", "siege"
    public int aiMaxCount = 0; // 0 = unlimited
}