using UnityEngine;

/// <summary>
/// Legacy command shim kept for backward compatibility.
///
/// This script no longer owns stances/AI movement so there is only one combat stance
/// authority in the project: UnitCombatController.
///
/// Existing callers that still use ICommandable can keep issuing move commands.
/// </summary>
public class UnitCommandController : MonoBehaviour, ICommandable
{
    private Unit unit;

    void Awake()
    {
        unit = GetComponent<Unit>();
        if (unit == null)
            Debug.LogWarning($"[UnitCommandController] {name} has no Unit component; movement commands will be ignored.", this);
    }

    public void IssueMove(Vector3 worldPos)
    {
        if (unit != null)
            unit.MoveTo(worldPos);
    }

    public void SetDefendHere() { }

    public void SetFollow(Transform target) { }

    public void ClearFollow() { }
}
