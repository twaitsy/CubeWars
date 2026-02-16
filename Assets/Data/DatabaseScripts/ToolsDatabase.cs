using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Tools")]
public class ToolsDatabase : ScriptableObject
{
    public List<ToolDefinition> tools = new();
}