using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Buildings")]
public class BuildingsDatabase : ScriptableObject
{
    public List<BuildingDefinition> buildings = new List<BuildingDefinition>();
}