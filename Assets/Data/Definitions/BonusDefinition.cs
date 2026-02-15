using System;
using UnityEngine;

public enum BonusTargetType
{
    Resource,
    Category
}

[Serializable]
public class BonusDefinition
{
    public string id; // optional, for debugging or lookup

    public BonusTargetType targetType;

    // Only one of these is used depending on targetType
    public ResourceDefinition resource;
    public ResourceCategory category;

    // Multiplier applied to speed/efficiency/etc.
    public float multiplier = 1f;
}