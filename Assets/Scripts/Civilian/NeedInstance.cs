using UnityEngine;

[System.Serializable]
public class NeedInstance
{
    public NeedDefinition definition;
    public float currentValue;

    public float Max => definition.maxValue;

    public bool IsCritical =>
        currentValue <= definition.maxValue * definition.criticalThreshold01;

    public bool ShouldSeek =>
        currentValue <= definition.maxValue * definition.seekThreshold01;

    public void Tick(float deltaTime)
    {
        currentValue -= definition.decayRatePerSecond * deltaTime;
        currentValue = Mathf.Clamp(currentValue, 0f, definition.maxValue);
    }
}