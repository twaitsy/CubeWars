using UnityEngine;

public class WithdrawStorageComponent : MonoBehaviour
{
    private Civilian civilian;
    private MovementController movement;
    private CarryingController carrying;

    private float searchTimer;

    [Header("Withdraw Settings")]
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

    public bool TryFindStorageWith(ResourceDefinition resource, out ResourceStorageContainer bestContainer)
    {
        bestContainer = null;

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

            if (!container.CanSupply(resource))
                continue;

            int available = container.GetStored(resource);
            if (available <= 0)
                continue;

            float distancePenalty = (facility.transform.position - transform.position).sqrMagnitude * 0.001f;
            float score = available - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                bestContainer = container;
            }
        }

        return bestContainer != null;
    }

    public void MoveToStorage(ResourceStorageContainer container)
    {
        if (container == null)
            return;

        Transform moveTarget = container.transform;
        var points = container.GetComponentInParent<BuildingInteractionPointController>();
        if (points != null && points.TryGetClosestPoint(BuildingInteractionPointType.Storage, transform.position, out Transform interaction))
            moveTarget = interaction;

        movement.MoveTo(moveTarget.position, stopDistance);
    }

    public bool TryWithdraw(ResourceStorageContainer container, ResourceDefinition resource, int amount)
    {
        if (container == null || !movement.HasArrived())
            return false;

        int taken = container.Withdraw(resource, amount);

        if (taken > 0)
            carrying.AddToInventory(resource, taken);

        return taken >= amount;
    }
}