using System;
using System.Collections.Generic;
using UnityEngine;

public enum NeedDrivenActionType
{
    None,
    SeekFood,
    SeekRest,
    SeekWater
}

[Serializable]
public class NeedActionEntry
{
    public string needId;
    public NeedDrivenActionType actionType = NeedDrivenActionType.None;
    [Range(0f, 1f)] public float triggerThreshold01 = 0.6f;
    [Min(0)] public int priority = 0;
}

[CreateAssetMenu(menuName = "CubeWars/Database/Needs Action", fileName = "NeedsActionDatabase")]
public class NeedsActionDatabase : ScriptableObject
{
    [SerializeField] private List<NeedActionEntry> entries = new();

    public IReadOnlyList<NeedActionEntry> Entries => entries;

    public bool TryGetBestAction(Func<string, float> normalizedNeedAccessor, out NeedDrivenActionType actionType)
    {
        actionType = NeedDrivenActionType.None;
        if (normalizedNeedAccessor == null || entries == null || entries.Count == 0)
            return false;

        int bestPriority = int.MinValue;
        float bestPressure = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            NeedActionEntry entry = entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.needId) || entry.actionType == NeedDrivenActionType.None)
                continue;

            float normalizedNeed = Mathf.Clamp01(normalizedNeedAccessor(entry.needId));
            float threshold = Mathf.Clamp01(entry.triggerThreshold01);
            if (normalizedNeed < threshold)
                continue;

            float pressure = normalizedNeed - threshold;
            if (entry.priority > bestPriority || (entry.priority == bestPriority && pressure > bestPressure))
            {
                bestPriority = entry.priority;
                bestPressure = pressure;
                actionType = entry.actionType;
            }
        }

        return actionType != NeedDrivenActionType.None;
    }
}
