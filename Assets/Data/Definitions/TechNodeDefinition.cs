using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TechNodeDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;

    [TextArea]
    public string description;

    // Research cost
    public List<ResourceAmount> researchCost = new();
    public float researchTime = 10f;

    // Requirements
    public List<string> prerequisiteIds = new();
    public BuildingDefinition requiredBuilding;

    // Unlocks
    public List<BuildingDefinition> unlockBuildings = new();
    public List<UnitDefinition> unlockUnits = new();
    public List<ProductionRecipeDefinition> unlockRecipes = new();

    // Stat modifiers (optional)
    public float globalProductionSpeedMultiplier = 1f;
    public float globalCombatDamageMultiplier = 1f;
    public float workerEfficiencyMultiplier = 1f;
}