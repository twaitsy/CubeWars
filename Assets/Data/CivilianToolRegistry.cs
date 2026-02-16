using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines all tool types a civilian can equip.
/// These are used as identifiers throughout the tool system.
/// </summary>
public enum CivilianToolType
{
    Pickaxe,
    Backpack,
    Spanner,
    Trowel,
    RunningShoes
}

/// <summary>
/// Central registry for all civilian tools in Cube Wars.
/// 
/// Responsibilities:
/// - Stores all tool definitions (multipliers, job requirements)
/// - Provides lookup helpers
/// - Provides job-matching logic
/// - Provides preferred starting tool logic
///
/// Dependencies:
/// - CivilianJobType enum (must include all jobs referenced below)
/// - Any system that assigns tools to civilians
/// - Any system that applies tool multipliers to civilian stats
///
/// This registry is intentionally static, lightweight, and terminal.
/// </summary>
public static class CivilianToolRegistry
{
    /// <summary>
    /// Describes a tool's gameplay effects and job restrictions.
    /// 
    /// Notes:
    /// - Multipliers default to 0 unless explicitly set.
    /// - Only the relevant multiplier is set for each tool.
    /// - Tools with requiredJob = Generalist are considered universal.
    /// </summary>
    public struct ToolProfile
    {
        public CivilianToolType toolType;          // Which tool this profile represents
        public CivilianJobType requiredJob;        // Which job is allowed to use it

        // Stat multipliers applied when the tool is equipped
        public float gatherSpeedMultiplier;
        public float carryCapacityMultiplier;
        public float craftingSpeedMultiplier;
        public float buildSpeedMultiplier;
        public float moveSpeedMultiplier;

        /// <summary>
        /// True if the tool can be used by any job.
        /// </summary>
        public bool IsUniversal => requiredJob == CivilianJobType.Generalist;
    }

    /// <summary>
    /// The authoritative list of all tools in the game.
    /// Each entry defines:
    /// - the tool type
    /// - the job required to use it
    /// - the stat multiplier it affects
    ///
    /// Only the relevant multiplier is set; all others remain 0.
    /// </summary>
    static readonly ToolProfile[] allTools =
    {
        new() { toolType = CivilianToolType.Pickaxe,      requiredJob = CivilianJobType.Gatherer,   gatherSpeedMultiplier = 1.5f },
        new() { toolType = CivilianToolType.Backpack,     requiredJob = CivilianJobType.Hauler,     carryCapacityMultiplier = 1.5f },
        new() { toolType = CivilianToolType.Spanner,      requiredJob = CivilianJobType.Engineer,   craftingSpeedMultiplier = 1.5f },
        new() { toolType = CivilianToolType.Trowel,       requiredJob = CivilianJobType.Builder,    buildSpeedMultiplier = 1.5f },
        new() { toolType = CivilianToolType.RunningShoes, requiredJob = CivilianJobType.Generalist, moveSpeedMultiplier = 1.5f }
    };

    /// <summary>
    /// Exposes the tool list as a read-only collection.
    /// Useful for UI, debugging, or iteration.
    /// </summary>
    public static IReadOnlyList<ToolProfile> AllTools => allTools;

    /// <summary>
    /// Returns the ToolProfile for a given tool type.
    /// Performs a simple linear search (fast enough for small arrays).
    /// Returns default(ToolProfile) if not found.
    /// </summary>
    public static ToolProfile Get(CivilianToolType toolType)
    {
        for (int i = 0; i < allTools.Length; i++)
            if (allTools[i].toolType == toolType)
                return allTools[i];

        return default;
    }

    /// <summary>
    /// Returns true if the tool is allowed for the given job.
    /// Universal tools always match.
    /// </summary>
    public static bool MatchesJob(ToolProfile profile, CivilianJobType jobType)
    {
        return profile.requiredJob == CivilianJobType.Generalist || profile.requiredJob == jobType;
    }

    /// <summary>
    /// Determines the preferred starting tool for a given job.
    /// Returns true if a tool was found, false otherwise.
    ///
    /// This is used when spawning civilians or assigning initial equipment.
    /// </summary>
    public static bool TryGetPreferredStartingTool(CivilianJobType jobType, out CivilianToolType toolType)
    {
        // Default fallback if no job-specific tool exists
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