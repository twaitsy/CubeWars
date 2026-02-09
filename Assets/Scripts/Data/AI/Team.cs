using UnityEngine;

/// <summary>
/// Defines whether a team is controlled by the player or AI.
/// 
/// IMPORTANT:
/// - Keeping this enum in the SAME file as Team ensures Unity never loses it.
/// - Avoid placing this enum in other folders or duplicate files.
/// </summary>
public enum TeamType
{
    Player,
    AI
}

/// <summary>
/// Represents a single team in Cube Wars.
///
/// DEPENDENCIES:
/// - TeamResources:
///     Tracks resources for this team.
/// - TeamColorManager:
///     Applies team colors to units/buildings.
/// - TeamInventory:
///     Stores items and crafting materials.
/// - TeamStorageManager:
///     Manages storage buildings and capacity.
/// - TeamBootstrap:
///     Spawns HQs, workers, and AI scripts.
/// - GameManager:
///     Detects and tracks all teams.
/// - Building / Unit:
///     Use teamID for ownership and combat targeting.
///
/// RESPONSIBILITIES:
/// - Identify team (ID + type)
/// - Provide root containers for HQ, units, buildings
/// - Auto-detect team systems
/// - Store team color
///
/// IMPORTANT:
/// - This script NEVER deletes teams.
/// - This script NEVER spawns anything.
/// - Pure data + references.
/// </summary>
public class Team : MonoBehaviour
{
    // ---------------------------------------------------------
    // TEAM IDENTITY
    // ---------------------------------------------------------
    [Header("Team Identity")]
    [Tooltip("Unique ID for this team (1 = Player, 2+ = AI teams).")]
    public int teamID = 1;

    [Tooltip("Whether this team is controlled by the player or AI.")]
    public TeamType teamType = TeamType.Player;

    [Tooltip("Color used for units, buildings, UI, minimap icons, etc.")]
    public Color teamColor = Color.white;


    // ---------------------------------------------------------
    // TEAM ROOT STRUCTURE
    // ---------------------------------------------------------
    [Header("Team Structure (Auto-Detected)")]
    [Tooltip("Root object containing this team's HQ building.")]
    public Transform hqRoot;

    [Tooltip("Root object containing all units belonging to this team.")]
    public Transform unitsRoot;

    [Tooltip("Root object containing all buildings belonging to this team.")]
    public Transform buildingsRoot;


    // ---------------------------------------------------------
    // TEAM SYSTEMS
    // ---------------------------------------------------------
    [Header("Team Systems (Auto-Detected)")]
    [Tooltip("Tracks resources (wood, stone, food, gold, etc.) for this team.")]
    public TeamResources teamResources;

    [Tooltip("Applies team colors to units, buildings, UI, minimap, etc.")]
    public TeamColorManager teamColorManager;

    [Tooltip("Handles inventory items, crafting materials, etc.")]
    public TeamInventory teamInventory;

    [Tooltip("Manages storage buildings and capacity for this team.")]
    public TeamStorageManager teamStorageManager;


    // ---------------------------------------------------------
    // INITIALIZATION
    // ---------------------------------------------------------
    void Awake()
    {
        // Auto-detect child roots if not assigned
        if (hqRoot == null) hqRoot = transform.Find("HQ");
        if (unitsRoot == null) unitsRoot = transform.Find("Units");
        if (buildingsRoot == null) buildingsRoot = transform.Find("Buildings");

        // Auto-detect team systems if they exist on this object
        if (teamResources == null) teamResources = GetComponent<TeamResources>();
        if (teamColorManager == null) teamColorManager = GetComponent<TeamColorManager>();
        if (teamInventory == null) teamInventory = GetComponent<TeamInventory>();
        if (teamStorageManager == null) teamStorageManager = GetComponent<TeamStorageManager>();
    }
}