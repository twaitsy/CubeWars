using UnityEngine;
using UnityEngine.AI;

public class MovementController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = value;
        if (agent != null)
            agent.speed = value;
    }
}