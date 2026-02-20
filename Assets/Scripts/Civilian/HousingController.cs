using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HousingController : MonoBehaviour
{
    [SerializeField] private float homelessRecheckIntervalSeconds = 2f;

    private Civilian civilian;
    private float homelessRecheckTimer;
    public House AssignedHouse { get; private set; }
    public House TargetHouse { get; private set; }

    void Awake()
    {
        civilian = GetComponent<Civilian>();
        homelessRecheckTimer = Random.Range(0f, homelessRecheckIntervalSeconds);
    }

    void Update()
    {
        if (civilian == null || !civilian.IsAlive)
            return;

        if (AssignedHouse != null && !AssignedHouse.IsAlive)
            ClearAssignedHouse();

        homelessRecheckTimer -= Time.deltaTime;
        if (homelessRecheckTimer > 0f)
            return;

        homelessRecheckTimer = Mathf.Max(0.25f, homelessRecheckIntervalSeconds);

        if (AssignedHouse == null)
            TryAssignHouseIfNeeded();
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

    public bool TryConsumeNearbyHouseFood(Vector3 civilianPosition, float nearDistance, IEnumerable<FoodDefinition> foods, int amountPerMeal, out ResourceDefinition consumedType, out FoodDefinition consumedFood, out int consumedAmount)
    {
        consumedType = null;
        consumedFood = null;
        consumedAmount = 0;

        if (AssignedHouse == null)
            return false;

        float maxDistance = Mathf.Max(0.1f, nearDistance);
        if ((civilianPosition - AssignedHouse.transform.position).sqrMagnitude > maxDistance * maxDistance)
            return false;

        foreach (FoodDefinition food in foods)
        {
            if (food == null || food.resource == null)
                continue;

            if (!TryConsumeHouseFood(food.resource, Mathf.Max(1, amountPerMeal), out int consumed) || consumed <= 0)
                continue;

            consumedType = food.resource;
            consumedFood = food;
            consumedAmount = consumed;
            return true;
        }

        return false;
    }

    public bool TryConsumeHouseFoodWhileResting(IEnumerable<FoodDefinition> foods, int amountPerMeal, NeedsController needsController, System.Func<bool> needsFood)
    {
        if (AssignedHouse == null || needsController == null || needsFood == null || !needsFood())
            return false;

        bool consumedAny = false;
        foreach (FoodDefinition food in foods)
        {
            if (food == null || food.resource == null)
                continue;

            if (!TryConsumeHouseFood(food.resource, Mathf.Max(1, amountPerMeal), out int consumed) || consumed <= 0)
                continue;

            float restore = consumed * Mathf.Max(1, food.hungerRestore);
            needsController.RestoreHunger(restore);
            consumedAny = true;

            if (!needsFood())
                break;
        }

        return consumedAny;
    }
}
