using System;
using UnityEngine;

[Serializable]
public class RecipeResourceAmount
{
    public ResourceType resourceType;
    [Min(1)] public int amount = 1;
}

[Serializable]
public class ProductionRecipe
{
    public string recipeName = "New Recipe";
    public RecipeResourceAmount[] inputs;
    public RecipeResourceAmount[] outputs;

    [Min(0.1f)] public float craftTimeSeconds = 5f;
    [Min(1)] public int batchSize = 1;

    public CivilianJobType requiredJobType = CivilianJobType.Generalist;

    [Tooltip("Multiplier to produced output. 1.0 = base output.")]
    [Min(0.1f)] public float outputEfficiencyMultiplier = 1f;

    [Tooltip("Multiplier to required input. 1.0 = base input.")]
    [Min(0.1f)] public float inputEfficiencyMultiplier = 1f;

    public bool requiresPower;
    public bool requiresFuel;

    public int GetScaledInputAmount(int baseAmount)
    {
        return Mathf.Max(1, Mathf.CeilToInt(baseAmount * inputEfficiencyMultiplier));
    }

    public int GetScaledOutputAmount(int baseAmount)
    {
        return Mathf.Max(1, Mathf.RoundToInt(baseAmount * outputEfficiencyMultiplier));
    }
}
