using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Needs/NeedDefinition")]
public class NeedDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;

    public float maxValue = 100f;
    public float decayRatePerSecond = 1f;

    public float seekThreshold01 = 0.5f; // When to satisfy this need
    public float criticalThreshold01 = 0.1f; // When bad things happen

    public NeedBehaviour behaviour; // Optional: custom logic
}