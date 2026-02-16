using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Jobs")]
public class JobsDatabase : ScriptableObject
{
    public List<JobDefinition> jobs = new();
}
