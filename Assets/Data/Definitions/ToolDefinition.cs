using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ToolDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;

    public int durability;
    public float baseEfficiency = 1f;

    // Bonuses this tool provides
    public List<BonusDefinition> bonuses = new();
}