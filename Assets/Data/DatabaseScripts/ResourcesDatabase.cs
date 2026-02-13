using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Resources")]
public class ResourcesDatabase : ScriptableObject
{
    public List<ResourceDefinition> resources = new List<ResourceDefinition>();
}