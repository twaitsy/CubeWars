using UnityEngine;

public class DepositStorageComponent : MonoBehaviour
{
    private Civilian civilian;
    private MovementController movement;
    private CarryingController carrying;

    private float searchTimer;

    [Header("Deposit Settings")]
    [SerializeField] private float searchRetrySeconds = 1.5f;
    [SerializeField] private float stopDistance = 1.2f;

    public float SearchRetrySeconds => Mathf.Max(0.1f, searchRetrySeconds);

    void Awake()
    {
        civilian = GetComponent<Civilian>();
        movement = GetComponent<MovementController>();
        carrying = GetComponent<CarryingController>();
    }

    // ---------------------------------------------------------
    // PUBLIC API
    // ---------------------------------------------------------

    public bool TryFindStorage(out ResourceStorageContainer bestContainer)
    {
        bestContainer = null;

        if (!carrying.IsCarrying)
            return false;

        searchTimer -= Time.deltaTime;
        if (searchTimer > 0f)
            return false;

        searchTimer = SearchRetrySeconds;

        StorageFacility[] facilities = FindObjectsByType<StorageFacility>(FindObjectsSortMode.None);
        float bestScore = float.MinValue;

        foreach (var facility in facilities)
        {
            if (facility == null)
                continue;

            var container = facility.GetComponentInChildren<ResourceStorageContainer>();
            if (container == null)
                continue;

            if (container.teamID != civilian.TeamID)
                continue;

            if (!container.CanReceive(carrying.CarriedResource))
                continue;

            int free = container.GetFree(carrying.CarriedResource);
            if (free <= 0)
                continue;

            float distancePenalty = (facility.transform.position - transform.position).sqrMagnitude * 0.001f;
            float score = free - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestContainer = container;
            }
        }

        return bestContainer != null;
    }

    public bool TryFindDropoff(out ResourceDropoff dropoff)
    {
        dropoff = ResourceDropoff.FindNearest(
            civilian.TeamID,
            carrying.CarriedResource,
            transform.position
        );

        return dropoff != null;
    }

    public void MoveToStorage(Transform target)
    {
        if (target == null)
            return;

        movement.MoveTo(target.position, stopDistance);
    }

    public bool TryDeposit(ResourceStorageContainer container)
    {
        if (container == null || !movement.HasArrived())
            return false;

        int amount = carrying.CarriedAmount;
        int accepted = container.Deposit(carrying.CarriedResource, amount);

        if (accepted > 0)
            carrying.RemoveFromInventory(accepted);

        return !carrying.IsCarrying;
    }

    public bool TryDeposit(ResourceDropoff dropoff)
    {
        if (dropoff == null || !movement.HasArrived())
        {
            Debug.Log($"[Deposit] FAILED early: dropoff={dropoff}, arrived={movement.HasArrived()}");
            return false;
        }

        var resource = carrying.CarriedResource;
        int amount = carrying.CarriedAmount;

        Debug.Log(
            $"[Deposit Attempt] " +
            $"resource={(resource != null ? resource.ToString() : "NULL")}, " +
            $"amount={amount}"
        );

        // We don't know what API ResourceDropoff exposes,
        // so we log BEFORE and AFTER the deposit call.
        int accepted = dropoff.Deposit(resource, amount);

        Debug.Log(
            $"[Deposit Result] accepted={accepted}, " +
            $"carriedBefore={amount}, " +
            $"carriedAfter={carrying.CarriedAmount}"
        );

        if (accepted > 0)
        {
            carrying.RemoveFromInventory(accepted);
            Debug.Log($"[Deposit Success] Removed {accepted} from inventory. Remaining={carrying.CarriedAmount}");
        }
        else
        {
            Debug.Log("[Deposit Failure] Storage rejected the resource (accepted=0).");
        }

        return !carrying.IsCarrying;
    }
}