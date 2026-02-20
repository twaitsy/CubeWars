using UnityEngine;

[DisallowMultipleComponent]
public class HousingController : MonoBehaviour
{
    private Civilian civilian;
    public House AssignedHouse { get; private set; }
    public House TargetHouse { get; private set; }

    void Awake()
    {
        civilian = GetComponent<Civilian>();
    }

    public bool TryAssignHouseIfNeeded()
    {
        if (AssignedHouse != null && AssignedHouse.IsAlive)
            return true;

        if (HousingRegistry.Instance != null && HousingRegistry.Instance.TryAssignHouse(civilian, civilian.teamID, transform.position, out House assigned))
        {
            AssignedHouse = assigned;
            TargetHouse = assigned;
            return true;
        }

        return false;
    }

    public void ClearAssignedHouse()
    {
        HousingRegistry.Instance?.ClearAssignment(civilian);
        AssignedHouse = null;
        TargetHouse = null;
    }

    public void SetTargetHouse(House house)
    {
        TargetHouse = house;
    }

    public bool TryEnsureTargetHouse()
    {
        if (TargetHouse != null && TargetHouse.IsAlive)
            return true;

        if (TryAssignHouseIfNeeded())
            return true;

        if (HousingRegistry.Instance != null)
            TargetHouse = HousingRegistry.Instance.FindClosestAvailableHouse(civilian.teamID, civilian, transform.position);

        return TargetHouse != null;
    }

    public bool TryClaimTargetHouse()
    {
        if (TargetHouse == null || !TargetHouse.IsAlive)
            return false;

        if (!TargetHouse.TryAddResident(civilian))
            return false;

        if (AssignedHouse != TargetHouse)
        {
            House newHouse = TargetHouse;
            HousingRegistry.Instance?.ClearAssignment(civilian);
            AssignedHouse = newHouse;
            TargetHouse = newHouse;
            HousingRegistry.Instance?.TryAssignHouse(civilian, civilian.teamID, transform.position, out _);
        }

        return true;
    }

    public float GetHouseSpeedBonusMultiplier()
    {
        if (AssignedHouse == null)
            return 1f;

        return 1f + Mathf.Max(0, AssignedHouse.prestige) * 0.1f;
    }

    public float GetTirednessReductionMultiplier()
    {
        if (AssignedHouse == null)
            return 1f;

        float reduction = Mathf.Max(0, AssignedHouse.comfort) * 0.1f;
        return Mathf.Max(0.1f, 1f - reduction);
    }

    public bool TryConsumeHouseFood(ResourceDefinition type, int amount, out int consumed)
    {
        consumed = 0;
        return AssignedHouse != null && AssignedHouse.TryConsumeFood(type, amount, out consumed);
    }
}
