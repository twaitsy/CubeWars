using System;
using UnityEngine;

[Serializable]
public class FoodDefinition
{
    public ResourceDefinition resource;

    [Min(1)] public int hungerRestore = 1;
    [Min(0.1f)] public float eatTimeSeconds = 1f;

    [Header("Cooking")]
    public bool requiresCooking;
    [Min(0f)] public float cookingTimeSeconds;
    public ResourceDefinition cookedResult;

    [Header("Spoilage")]
    public bool canSpoil;
    [Min(0f)] public float spoilAfterSeconds;
    public ResourceDefinition spoiledResult;
}
