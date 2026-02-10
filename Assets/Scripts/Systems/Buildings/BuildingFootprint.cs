using UnityEngine;

/// <summary>
/// Placement metadata for buildable prefabs.
/// Attach to a building prefab to define how many grid cells it occupies,
/// and how position/rotation should be adjusted during placement.
/// </summary>
public class BuildingFootprint : MonoBehaviour
{
    [Header("Grid Footprint")]
    [Min(1)] public int width = 1;
    [Min(1)] public int depth = 1;

    [Header("Placement Offsets")]
    [Tooltip("Extra world-space offset applied after BuildItemDefinition placement offset.")]
    public Vector3 worldOffset = Vector3.zero;

    [Tooltip("Optional additional local Y rotation in degrees.")]
    public float extraYRotation;

    /// <summary>
    /// Returns footprint dimensions for the specified quarter-turn rotation.
    /// </summary>
    public Vector2Int GetDimensionsForQuarterTurns(int quarterTurns)
    {
        int turns = Mathf.Abs(quarterTurns) % 2;
        return turns == 0
            ? new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, depth))
            : new Vector2Int(Mathf.Max(1, depth), Mathf.Max(1, width));
    }
}
