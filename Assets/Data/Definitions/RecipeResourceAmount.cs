using System;
using UnityEngine;

[Serializable]
public class RecipeResourceAmount
{
    public ResourceDefinition resource;
    [Min(1)] public int amount = 1;
}
