using UnityEngine;

/// <summary>
/// Core AI controller for a single AI team.
/// 
/// DEPENDENCIES:
/// - AIEconomy
/// - AIMilitary
/// - AIBuilder
/// - AIResourceManager
/// - AIThreatDetector (optional)
/// - AIRepairManager (optional)
/// - AIRebuildManager (optional)
///
/// RESPONSIBILITIES:
/// - Tick all AI subsystems
/// - Apply difficulty settings
/// - Forward teamID and personality to subsystems
///
/// IMPORTANT:
/// - This script does NOT create or destroy teams.
/// - It only runs logic for the team it is attached to.
/// </summary>
[RequireComponent(typeof(AIEconomy))]
[RequireComponent(typeof(AIMilitary))]
[RequireComponent(typeof(AIBuilder))]
[RequireComponent(typeof(AIResourceManager))]
[RequireComponent(typeof(AIThreatDetector))]
public class AIPlayer : MonoBehaviour
{
    [Header("Team")]
    [Tooltip("Team ID this AI controls.")]
    public int teamID;

    [Header("AI Settings")]
    public AIDifficulty difficulty = AIDifficulty.Normal;
    public AIPersonality personality = AIPersonality.Balanced;

    float thinkInterval = 2f;
    float timer;

    AIEconomy economy;
    AIMilitary military;
    AIBuilder builder;
    AIResourceManager resourceManager;
    AIThreatDetector threatDetector;
    AIRepairManager repairManager;
    AIRebuildManager rebuildManager;

    void Awake()
    {
        // Cache subsystem references
        economy = GetComponent<AIEconomy>();
        military = GetComponent<AIMilitary>();
        builder = GetComponent<AIBuilder>();
        resourceManager = GetComponent<AIResourceManager>();
        threatDetector = GetComponent<AIThreatDetector>();
        repairManager = GetComponent<AIRepairManager>();
        rebuildManager = GetComponent<AIRebuildManager>();

        // Forward teamID to all subsystems
        economy.teamID = teamID;
        military.teamID = teamID;
        builder.teamID = teamID;
        resourceManager.teamID = teamID;
        if (threatDetector != null) threatDetector.teamID = teamID;
        if (repairManager != null) repairManager.teamID = teamID;
        if (rebuildManager != null) rebuildManager.teamID = teamID;

        // Forward personality
        economy.personality = personality;
        military.personality = personality;
        builder.personality = personality;

        ApplyDifficulty();
    }

    void ApplyDifficulty()
    {
        switch (difficulty)
        {
            case AIDifficulty.Easy:
                thinkInterval = 3f;
                break;
            case AIDifficulty.Hard:
                thinkInterval = 1.2f;
                break;
            default:
                thinkInterval = 2f;
                break;
        }
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = thinkInterval;

        // Tick all AI subsystems
        economy.Tick();
        builder.Tick();
        resourceManager.Tick();
        if (rebuildManager != null) rebuildManager.Tick();
        if (repairManager != null) repairManager.Tick();
        military.Tick();
    }
}