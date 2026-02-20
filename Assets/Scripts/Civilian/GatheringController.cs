using UnityEngine;

[DisallowMultipleComponent]
public class GatheringController : MonoBehaviour
{
    private Civilian civilian;
    private MovementController movement;
    private CarryingController carrying;

    private DepositStorageComponent deposit;
    private WithdrawStorageComponent withdraw;

    [Header("Gathering")]
    public int harvestPerTick = 1;
    public float gatherInterval = 1.0f;
    public float gatherRange = 1.5f;

    private float nextGatherTime;
    public ResourceNode CurrentNode { get; private set; }
    public CarryingController Carrying => carrying;
    void Awake()
    {
        civilian = GetComponent<Civilian>();
        movement = GetComponent<MovementController>();
        carrying = GetComponent<CarryingController>();

        deposit = GetComponent<DepositStorageComponent>();
        withdraw = GetComponent<WithdrawStorageComponent>();
    }

    // ---------------------------------------------------------
    // ASSIGNMENT
    // ---------------------------------------------------------

    public bool AssignNode(ResourceNode node)
    {
        if (node == null || node.IsDepleted)
            return false;

        // Release previous node if switching
        if (CurrentNode != null && CurrentNode != node)
            Release(CurrentNode);

        if (!TryReserve(node))
            return false;

        CurrentNode = node;
        MoveToNode(node);
        nextGatherTime = 0f;

        return true;
    }

    public void StopGathering()
    {
        if (CurrentNode != null)
            Release(CurrentNode);

        CurrentNode = null;
        nextGatherTime = 0f;
    }

    // ---------------------------------------------------------
    // SEARCH
    // ---------------------------------------------------------

    public bool TryFindNode(out ResourceNode best)
    {
        best = null;

        var registry = ResourceRegistry.Instance;
        if (registry == null)
            return false;

        float bestScore = float.MinValue;

        foreach (var node in registry.AllNodes)
        {
            if (node == null || node.IsDepleted)
                continue;

            if (node.IsClaimedByOther(civilian.TeamID))
                continue;

            if (!node.HasAvailableGatherSlots)
                continue;

            float distancePenalty =
                (node.transform.position - transform.position).sqrMagnitude * 0.001f;

            float score = node.remaining - distancePenalty;

            if (score > bestScore)
            {
                bestScore = score;
                best = node;
            }
        }

        return best != null;
    }

    // ---------------------------------------------------------
    // RESERVATION
    // ---------------------------------------------------------

    public bool TryReserve(ResourceNode node)
    {
        return node != null && node.TryReserveGatherSlot(civilian);
    }

    public void Release(ResourceNode node)
    {
        if (node != null)
            node.ReleaseGatherSlot(civilian);
    }

    // ---------------------------------------------------------
    // MOVEMENT
    // ---------------------------------------------------------

    public void MoveToNode(ResourceNode node)
    {
        if (node == null || movement == null)
            return;

        movement.MoveTo(node.transform.position, gatherRange);
    }

    // ---------------------------------------------------------
    // GATHERING TICK
    // ---------------------------------------------------------

    public bool TickGathering()
    {
        if (CurrentNode == null)
            return false;

        if (movement == null || !movement.HasArrived())
            return false;

        // Optional: prevent gathering when full
        if (carrying != null && carrying.IsFull)
            return false;

        if (Time.time < nextGatherTime)
            return false;

        nextGatherTime = Time.time + gatherInterval;

        int taken = CurrentNode.Harvest(harvestPerTick);
        if (taken > 0)
        {
            if (carrying != null)
                carrying.AddToInventory(CurrentNode.resource, taken);

            // If node depleted on this tick, stop using it
            if (CurrentNode.IsDepleted)
                StopGathering();

            return true;
        }

        // Nothing harvested → node likely depleted
        StopGathering();
        return false;
    }

    // ---------------------------------------------------------
    // DEPOSIT / WITHDRAW
    // ---------------------------------------------------------

    public bool TryFindDepositStorage(out ResourceStorageContainer container)
    {
        if (deposit != null && deposit.TryFindStorage(out container))
            return true;

        container = null;
        return false;
    }

    public bool TryFindDropoff(out ResourceDropoff dropoff)
    {
        if (deposit != null && deposit.TryFindDropoff(out dropoff))
            return true;

        dropoff = null;
        return false;
    }

    public void MoveToDeposit(Transform target)
    {
        if (deposit != null)
            deposit.MoveToStorage(target);
    }

    public bool TryDeposit(ResourceStorageContainer container)
    {
        return deposit != null && deposit.TryDeposit(container);
    }

    public bool TryDeposit(ResourceDropoff dropoff)
    {
        return deposit != null && deposit.TryDeposit(dropoff);
    }

    public bool TryFindWithdrawStorage(ResourceDefinition resource, out ResourceStorageContainer container)
    {
        if (withdraw != null && withdraw.TryFindStorageWith(resource, out container))
            return true;

        container = null;
        return false;
    }

    public void MoveToWithdraw(ResourceStorageContainer container)
    {
        if (withdraw != null)
            withdraw.MoveToStorage(container);
    }

    public bool TryWithdraw(ResourceStorageContainer container, ResourceDefinition resource, int amount)
    {
        return withdraw != null && withdraw.TryWithdraw(container, resource, amount);
    }
}