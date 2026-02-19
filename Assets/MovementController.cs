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
    private NavMeshAgent agent;


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

        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(destination);
    }
    public bool HasArrived()
    {
        if (agent == null || !agent.enabled || !agent.gameObject.activeInHierarchy || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending) return false;
        if (!agent.hasPath) return false;
        if (agent.remainingDistance == Mathf.Infinity) return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0.01f, value);
    }

    public void SetStopDistance(float value)
    {
        stopDistance = Mathf.Max(0.01f, value);
    }

    public void SetRoadBonus(bool enabled, float multiplier)
    {
        useRoadBonus = enabled;
        roadSpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public float GetEffectiveSpeed(float worldMultiplier)
    {
        return moveSpeed * Mathf.Max(0.01f, worldMultiplier);
    }

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
