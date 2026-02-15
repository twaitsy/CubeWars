using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JobDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;

    // Tools this job is allowed to use
    public List<ToolDefinition> allowedTools = new List<ToolDefinition>();

    // Base skill for this job (gathering, building, mining, etc.)
    public float baseSkill = 1f;
}