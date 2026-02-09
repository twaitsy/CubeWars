// =============================================================
// UnitProductionDefinition.cs
//
// DEPENDENCIES:
// - Barracks:
//      * Uses unitPrefab, buildTime, costs to enqueue units.
// - UnitProductionQueue:
//      * Uses buildTime and unitName for timing and UI.
// - UnitInspectorUI:
//      * Displays unitName and costs for the player.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new cost types (e.g., population), extend this definition
//   and update Barracks + UI accordingly.
// - Keep unitPrefab consistent with the Unit/UnitCombatController setup.
// =============================================================

using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Unit Production Definition")]
public class UnitProductionDefinition : ScriptableObject
{
    public string unitName;
    public GameObject unitPrefab;
    public float buildTime = 5f;
    public ResourceCost[] costs;
}