using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UnitDefinition
{
    // Identity
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;

    // Classification
    public CivilianJobType jobType = CivilianJobType.Generalist;
    public bool isCombatUnit = false;

    // Core Stats
    public int maxHealth = 100;
    public float moveSpeed = 3.5f;

    // Combat Stats
    public int attackDamage = 0;
    public float attackRange = 0f;
    public float attackCooldown = 1f;
    public int armor = 0;

    // Economy / Worker Stats
    public int carryCapacity = 10;
    public float gatherSpeed = 1f;
    public float buildSpeed = 1f;

    // Training
    public List<ResourceAmount> trainingCost = new();
    public float trainingTime = 5f;
    public BuildingDefinition trainedAt;

    // Tools
    public ToolDefinition[] startingTools;

    // Upgrades
    public UnitDefinition upgradeTo;
    public List<NeedDefinition> needs;
}