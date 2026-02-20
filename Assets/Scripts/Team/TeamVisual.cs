using UnityEngine;

/// <summary>
/// TeamVisual
///
/// PURPOSE:
/// - Applies team-based visuals (colors, materials, decals, etc.)
///   to units, civilians, and buildings.
/// - Delegates styling to SciFiTeamStyler if present, otherwise
///   falls back to simple color tinting via TeamColorManager.
///
/// DEPENDENCIES:
/// - SciFiTeamStyler (optional):
///     * Centralized styling system for meshes, materials, emissives,
///       decals, etc.
///     * Must expose: void Apply(GameObject obj, int teamID, VisualKind kind).
/// - TeamColorManager (optional fallback):
///     * Simple color tinting using Renderer.material.color.
/// - Team.cs:
///     * teamID is usually assigned from Team / Building / Unit.
/// - Building / Unit / Civilian:
///     * These components typically own the teamID that should be
///       mirrored here.
///
/// NOTES FOR FUTURE MAINTENANCE:
/// - This script NEVER deletes teams or GameObjects.
/// - Safe to put on any prefab that needs team visuals.
/// - If you add new visual kinds (e.g., Vehicle, StructureAddon),
///   extend VisualKind and update SciFiTeamStyler accordingly.
/// - If you move away from a global SciFiTeamStyler, you can inject
///   styling via another system and keep this as a thin adapter.
/// - applyOnStart is useful for prefabs; for pooled objects you may
///   want to call Apply() manually after teamID is set.
///
/// ARCHITECTURE:
/// - Runs once on Start() if applyOnStart is true.
/// - Purely visual; no gameplay logic, no resource logic, no deletion.
/// </summary>
public enum VisualKind { Unit, Civilian, Building }

public class TeamVisual : MonoBehaviour
{
    public int teamID;
    public VisualKind kind = VisualKind.Unit;
    public bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart) Apply();
    }

    public void Apply()
    {
        if (SciFiTeamStyler.Instance != null)
            SciFiTeamStyler.Instance.Apply(gameObject, teamID, kind);
        else if (TeamColorManager.Instance != null)
            TeamColorManager.Instance.ApplyTeamColor(gameObject, teamID); // fallback
    }
}