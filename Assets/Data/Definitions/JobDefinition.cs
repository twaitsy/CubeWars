using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Data/Job Definition", fileName = "JobDefinition")]
public class JobDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    [Header("Capability Profile")]
    public CivilianJobType defaultJobType = CivilianJobType.Generalist;
    public CivilianRole legacyRole = CivilianRole.Gatherer;
    public float baseSkill = 1f;
    public List<WorkerCapability> capabilities = new List<WorkerCapability>();
    public List<ToolDefinition> allowedTools = new List<ToolDefinition>();

    public bool HasCapability(WorkerCapability capability)
    {
        return capabilities != null && capabilities.Contains(capability);
    }
}
