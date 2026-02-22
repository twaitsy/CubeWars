using UnityEngine;

[CreateAssetMenu(fileName = "CivilianStateSettings", menuName = "CubeWars/Civilian/State Settings")]
public class CivilianStateSettings : ScriptableObject
{
    [Header("Search")]
    [Min(0.1f)] public float searchRetrySeconds = 1.5f;

    [Header("Idle")]
    [Min(0f)] public float idleWanderRadius = 3f;
    [Min(0.1f)] public float idleWanderIntervalMin = 2f;
    [Min(0.1f)] public float idleWanderIntervalMax = 5f;
    [Min(0f)] public float idleNoTaskReturnToHouseDelay = 10f;

    [Header("Crafting")]
    [Min(0f)] public float workPointStallRepathSeconds = 10f;
}
