using System;
using UnityEngine;

[Serializable]
public class FoodDefinition
{
    public ResourceDefinition resource;
    [Min(1)] public int hungerRestore = 1;
    [Min(0.1f)] public float eatTime = 1f;
    public bool requiresCooking;
    public bool perishable;
    [Min(0f)] public float spoilTime;
}
