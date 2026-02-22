using System;
using System.Collections.Generic;
using UnityEngine;

public enum ToolCategory
{
    None,
    Axe,
    Pickaxe,
    Hammer,
    Knife,
    Sickle,
    Wrench,
    Saw,
    Shovel,
    Tongs,
    Brush,
    Custom
}

[Serializable]
public class ToolDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;

    public ToolCategory category = ToolCategory.None;
    public int tier = 1;
    public RarityType rarity = RarityType.Common;

    public int maxDurability = 100;
    public int durabilityLossPerUse = 1;
    public float breakChance = 0f;
    public ResourceAmount[] repairCost;

    public float baseEfficiency = 1f;
    public float qualityMultiplier = 1f;
    public float workerSkillMultiplier = 1f;

    public List<CivilianJobType> jobTypeRestrictions = new();
    public List<BonusDefinition> jobTypeBonuses = new();

    public List<BonusDefinition> bonuses = new();          // resource/category
    public List<BonusDefinition> recipeBonuses = new();    // recipe-specific
    public List<BonusDefinition> stationBonuses = new();   // station-specific

    public int attackDamage = 0;
    public float attackSpeed = 1f;
    public float attackRange = 1f;

    public float aiEquipWeight = 1f;
    public float aiCraftPriority = 1f;
    public int marketValue = 1;
    public float maintenanceCost = 0f;
}