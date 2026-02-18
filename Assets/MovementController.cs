using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private bool useRoadBonus = true;
    [SerializeField] private float roadSpeedMultiplier = 1.2f;

    private NavMeshAgent agent;

    public float MoveSpeed => moveSpeed;
    public float StopDistance => stopDistance;
    public bool UseRoadBonus => useRoadBonus;
    public float RoadSpeedMultiplier => roadSpeedMultiplier;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ApplyToAgent(1f);
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
        agent.stoppingDistance = stopDistance;
    }
}
