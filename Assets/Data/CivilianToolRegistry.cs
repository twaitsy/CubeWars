using System.Collections.Generic;
using UnityEngine;

public enum CivilianToolType
{
    Pickaxe,
    Backpack,
    Spanner,
    Trowel,
    RunningShoes
}

public static class CivilianToolRegistry
{
    public struct ToolProfile
    {
        public CivilianToolType toolType;
        public CivilianJobType requiredJob;
        public float gatherSpeedMultiplier;
        public float carryCapacityMultiplier;
        public float craftingSpeedMultiplier;
        public float buildSpeedMultiplier;
        public float moveSpeedMultiplier;

        public bool IsUniversal => requiredJob == CivilianJobType.Generalist;
    }

    static readonly ToolProfile[] allTools =
    {
        new ToolProfile { toolType = CivilianToolType.Pickaxe, requiredJob = CivilianJobType.Gatherer, gatherSpeedMultiplier = 1.5f },
        new ToolProfile { toolType = CivilianToolType.Backpack, requiredJob = CivilianJobType.Hauler, carryCapacityMultiplier = 1.5f },
        new ToolProfile { toolType = CivilianToolType.Spanner, requiredJob = CivilianJobType.Engineer, craftingSpeedMultiplier = 1.5f },
        new ToolProfile { toolType = CivilianToolType.Trowel, requiredJob = CivilianJobType.Builder, buildSpeedMultiplier = 1.5f },
        new ToolProfile { toolType = CivilianToolType.RunningShoes, requiredJob = CivilianJobType.Generalist, moveSpeedMultiplier = 1.5f }
    };

    public static IReadOnlyList<ToolProfile> AllTools => allTools;

    public static ToolProfile Get(CivilianToolType toolType)
    {
        for (int i = 0; i < allTools.Length; i++)
            if (allTools[i].toolType == toolType)
                return allTools[i];

        return default;
    }

    public static bool MatchesJob(ToolProfile profile, CivilianJobType jobType)
    {
        return profile.requiredJob == CivilianJobType.Generalist || profile.requiredJob == jobType;
    }

    public static bool TryGetPreferredStartingTool(CivilianJobType jobType, out CivilianToolType toolType)
    {
        toolType = CivilianToolType.RunningShoes;

        switch (jobType)
        {
            case CivilianJobType.Gatherer:
            case CivilianJobType.Farmer:
                toolType = CivilianToolType.Pickaxe;
                return true;
            case CivilianJobType.Builder:
            case CivilianJobType.Carpenter:
                toolType = CivilianToolType.Trowel;
                return true;
            case CivilianJobType.Hauler:
                toolType = CivilianToolType.Backpack;
                return true;
            case CivilianJobType.Technician:
            case CivilianJobType.Engineer:
            case CivilianJobType.Scientist:
            case CivilianJobType.Crafter:
            case CivilianJobType.Blacksmith:
            case CivilianJobType.Cook:
                toolType = CivilianToolType.Spanner;
                return true;
            default:
                return false;
        }
    }
}
