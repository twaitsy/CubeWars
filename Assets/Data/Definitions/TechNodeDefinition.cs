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
    public List<ResourceAmount> researchCost = new List<ResourceAmount>();
    public float researchTime = 10f;

    // Requirements
    public List<string> prerequisiteIds = new List<string>();
    public BuildingDefinition requiredBuilding;

    // Unlocks
    public List<BuildingDefinition> unlockBuildings = new List<BuildingDefinition>();
    public List<UnitDefinition> unlockUnits = new List<UnitDefinition>();
    public List<ProductionRecipeDefinition> unlockRecipes = new List<ProductionRecipeDefinition>();

    // Stat modifiers (optional)
    public float globalProductionSpeedMultiplier = 1f;
    public float globalCombatDamageMultiplier = 1f;
    public float workerEfficiencyMultiplier = 1f;
}