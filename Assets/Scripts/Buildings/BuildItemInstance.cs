// ============================================================================
// BuildItemInstance.cs
//
// PURPOSE:
// - Identifies a placed building or construction site by its BuildItemDefinition.
// - Allows systems to query building type without relying on class names.
// - Essential for AI, rebuilding logic, and analytics.
//
// DEPENDENCIES:
// - BuildItemDefinition:
//      * itemId matches the ScriptableObject name (item.name).
// - BuildPlacementManager:
//      * Ensures this component exists on placed buildings.
// - ConstructionSite:
//      * Adds BuildItemInstance to the final building on completion.
// - AIRebuildManager / AIBuilder:
//      * Uses itemId to determine what was built or destroyed.
// - Save/Load System (future):
//      * Should serialize itemId to restore building type.
// - BuildingInspector / UI (future):
//      * Can display building type using itemId.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add building upgrades, itemId should change to the upgraded definition.
// - If you add analytics or statistics, this is the safest identifier to use.
// - If you add multi-building variants (skins, levels), consider adding:
//      * variantId
//      * level
// - If you add a tech tree, itemId can be used to check prerequisites.
//
// INSPECTOR REQUIREMENTS:
// - itemId is auto-assigned by BuildPlacementManager or ConstructionSite.
// ============================================================================

using UnityEngine;

/// <summary>
/// Attached to placed buildings and construction sites so we can count/identify them
/// without relying on specific script/class names like "Turret" or "Stockpile".
/// </summary>
public class BuildItemInstance : MonoBehaviour
{
    [Tooltip("Matches the BuildItemDefinition asset name (item.name).")]
    public string itemId;
}