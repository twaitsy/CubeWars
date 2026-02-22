using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RecipeResourceAmount
{
    [Header("Resource")]
    public ResourceDefinition resource;

    [Tooltip("Optional: allow any resource with matching category or tags.")]
    public ResourceCategory categoryFallback;
    public List<string> tagFallbacks = new(); // e.g. "wood", "metal"

    [Header("Quantity")]
    [Min(1)] public int amount = 1;

    [Tooltip("0 = normal, 1 = refined, 2 = high-quality, etc.")]
    public int qualityTier = 0;

    [Tooltip("Affects output quality, yield, or value.")]
    public float qualityMultiplier = 1f;

    [Header("Chance & Variance")]
    [Tooltip("1.0 = guaranteed, <1 = probabilistic input.")]
    public float chance = 1f;

    [Tooltip("Optional +/- variance in input amount.")]
    public Vector2Int randomRange = new(0, 0);

    [Header("Batch Scaling")]
    public bool scaleWithBatch = true;
    public float batchMultiplier = 1f;

    [Header("Consumption Behaviour")]
    [Tooltip("If true, only part of the resource is consumed (e.g., catalyst).")]
    public bool partialConsumption = false;

    [Tooltip("0–1: fraction consumed if partialConsumption is true.")]
    public float consumptionFraction = 1f;

    [Header("Spoilage & Decay")]
    public bool canSpoil = false;
    public float spoilRate = 0f;

    [Tooltip("Environmental sensitivity (heat, cold, humidity).")]
    public float environmentSensitivity = 1f;

    [Header("Byproducts")]
    public List<ResourceAmount> byproducts = new();
    public float byproductChance = 1f;

    [Header("Tech & Efficiency Modifiers")]
    [Tooltip("Tech nodes that reduce cost or increase efficiency.")]
    public List<TechNodeDefinition> techCostModifiers = new();

    [Tooltip("Multiplier applied after tech and bonuses.")]
    public float efficiencyMultiplier = 1f;

    [Header("AI & Economy")]
    public float aiPriority = 1f;
    public float economicWeight = 1f;

    public int GetFinalAmount(int batchSize)
    {
        int baseAmount = amount;

        if (scaleWithBatch)
            baseAmount = Mathf.RoundToInt(baseAmount * batchSize * batchMultiplier);

        if (randomRange.x != 0 || randomRange.y != 0)
            baseAmount += UnityEngine.Random.Range(randomRange.x, randomRange.y + 1);

        if (partialConsumption)
            baseAmount = Mathf.Max(1, Mathf.RoundToInt(baseAmount * consumptionFraction));

        return Mathf.Max(1, baseAmount);
    }
}