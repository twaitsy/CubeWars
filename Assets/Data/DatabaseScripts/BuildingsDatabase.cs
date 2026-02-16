using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Buildings")]
public class BuildingsDatabase : ScriptableObject
{
    public List<string> categoryOrder = new() { "Economy", "Industry", "Housing", "Defense", "Tech" };
    public List<BuildItemDefinition> items = new();
    public List<BuildingDefinition> buildings = new();
}
