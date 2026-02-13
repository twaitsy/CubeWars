using System;
using UnityEngine;

[Serializable]
public class JobDefinition
{
    public string id;
    public string displayName;
    public Sprite icon;

    // Optional: tools required for this job
    public ToolDefinition[] requiredTools;
}