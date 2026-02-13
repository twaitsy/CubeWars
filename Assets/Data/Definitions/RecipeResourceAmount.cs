using System;
using UnityEngine;

[Serializable]
public class RecipeResourceAmount
{
    public ResourceType resourceType;
    [Min(1)] public int amount = 1;
}