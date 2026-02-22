using System;
using UnityEngine;

public enum BonusTargetType
{
    Resource,
    Category,
    QualityTier,
    JobType,
    Recipe,
    Station
}

public enum BonusStackingMode
{
    Multiply,
    Add,
    Override
}

[Serializable]
public class BonusDefinition
{
    [Header("Identity")]
    public string id;

    [Header("Target Type")]
    public BonusTargetType targetType;

    [Header("Targets")]
    public ResourceDefinition resource;
    public ResourceCategory category;
    public int qualityTier;
    public CivilianJobType jobType;
    public ProductionRecipeDefinition recipe;
    public string stationId;

    [Header("Bonus Value")]
    public float multiplier = 1f;
    public float additiveBonus = 0f;
    public BonusStackingMode stackingMode = BonusStackingMode.Multiply;

    public bool Matches(ResourceDefinition targetResource,
                        CivilianJobType workerJob,
                        ProductionRecipeDefinition targetRecipe,
                        string activeStationId)
    {
        switch (targetType)
        {
            case BonusTargetType.Resource:
                if (resource == null || targetResource == null)
                    return false;
                if (ReferenceEquals(resource, targetResource))
                    return true;
                return string.Equals(resource.id, targetResource.id, StringComparison.OrdinalIgnoreCase);

            case BonusTargetType.Category:
                return targetResource != null && targetResource.category == category;

            case BonusTargetType.QualityTier:
                return targetResource != null && targetResource.tier == qualityTier;

            case BonusTargetType.JobType:
                return workerJob == jobType;

            case BonusTargetType.Recipe:
                return targetRecipe != null && recipe != null &&
                       string.Equals(recipe.recipeId, targetRecipe.recipeId, StringComparison.OrdinalIgnoreCase);

            case BonusTargetType.Station:
                return !string.IsNullOrWhiteSpace(activeStationId) &&
                       string.Equals(stationId, activeStationId, StringComparison.OrdinalIgnoreCase);

            default:
                return false;
        }
    }
}