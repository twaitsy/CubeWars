using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private bool useRoadBonus = true;
    [SerializeField] private float roadSpeedMultiplier = 1.2f;
    public float MoveSpeed => moveSpeed;
    public float StopDistance => stopDistance;
    public bool UseRoadBonus => useRoadBonus;
    public float RoadSpeedMultiplier => roadSpeedMultiplier;
    public bool IsOnRoad => roadTriggerCount > 0;

    private NavMeshAgent agent;
    private Vector3 lastDestination = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    private float lastStoppingDistance = -1f;
    private int roadTriggerCount;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        ApplyToAgent(1f);
    }

    public void ApplyMovement(float speedMultiplier)
    {
        agent.speed = moveSpeed * speedMultiplier;
        agent.stoppingDistance = stopDistance;
    }

    public void MoveTo(Vector3 destination, float stoppingDistance)
    {
        if (agent == null || !agent.enabled)
            return;

        if ((lastDestination - destination).sqrMagnitude < 0.01f && Mathf.Abs(lastStoppingDistance - stoppingDistance) < 0.05f)
            return;

        lastDestination = destination;
        lastStoppingDistance = stoppingDistance;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(destination);
    }

    public void MoveToBuildingTarget(Transform destination, BuildingStopDistanceType stopType, BuildingInteractionPointType interactionType)
    {
        if (destination == null)
            return;

        float stop = ResolveStopDistance(destination, stopType);
        Transform moveTarget = ResolveInteractionTarget(destination, interactionType);
        MoveTo(moveTarget.position, stop);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null && other.CompareTag("Road"))
            roadTriggerCount++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other != null && other.CompareTag("Road"))
            roadTriggerCount = Mathf.Max(0, roadTriggerCount - 1);
    }

    public float ResolveStopDistance(Transform destination, BuildingStopDistanceType stopType)
    {
        if (destination == null)
            return Mathf.Max(0.1f, stopDistance);

        var settings = destination.GetComponentInParent<BuildingInteractionSettings>();
        if (settings == null)
            return Mathf.Max(0.1f, stopDistance);

        return settings.GetStopDistance(stopType, stopDistance);
    }

    public Transform ResolveInteractionTarget(Transform destination, BuildingInteractionPointType pointType)
    {
        if (destination == null)
            return transform;

        var controller = destination.GetComponentInParent<BuildingInteractionPointController>();
        if (controller != null && controller.TryGetClosestPoint(pointType, transform.position, out Transform interactionPoint))
            return interactionPoint;

        return destination;
    }

    public bool HasArrived()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            return true;

        if (!agent.hasPath && agent.velocity.sqrMagnitude < 0.01f)
            return true;

        return false;
    }

    public void SetMoveSpeed(float value) => moveSpeed = Mathf.Max(0.01f, value);
    public void SetStopDistance(float value) => stopDistance = Mathf.Max(0.01f, value);

    public void SetRoadBonus(bool enabled, float multiplier)
    {
        useRoadBonus = enabled;
        roadSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public float GetEffectiveSpeed(float worldMultiplier) => moveSpeed * Mathf.Max(0.01f, worldMultiplier);

    public void ApplyToAgent(float worldMultiplier)
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent == null)
            return;

        agent.speed = GetEffectiveSpeed(worldMultiplier);
        agent.stoppingDistance = StopDistance;
    }
}
