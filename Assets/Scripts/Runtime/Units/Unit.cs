using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour, IHasHealth, IAttackable, ICommandable
{
    [Header("Team")]
    public int teamID;

    [Header("Combat")]
    public bool combatEnabled = true;
    public float attackRange = 5f;
    public float damage = 10f;

    [Header("Health")]
    public float MaxHealth = 100f;
    public float CurrentHealth = 100f;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.stoppingDistance = 0.5f;
            agent.updateRotation = true;
            agent.avoidancePriority = Random.Range(20, 80);
        }
    }

    void OnEnable() => UnitManager.Instance?.Register(this);
    void OnDisable() => UnitManager.Instance?.Unregister(this);

    public void SetTeamID(int newTeamID) => teamID = newTeamID;

    public void MoveTo(Vector3 pos)
    {
        if (agent == null) return;
        agent.isStopped = false;
        agent.SetDestination(pos);
    }

    public void IssueMove(Vector3 worldPos)
    {
        MoveTo(worldPos);
        var combat = GetComponent<UnitCombatController>();
        combat?.CancelCurrentEngagement();
    }

    float IHasHealth.CurrentHealth => CurrentHealth;
    float IHasHealth.MaxHealth => MaxHealth;

    public int TeamID => teamID;
    public bool IsAlive => CurrentHealth > 0f;
    public Transform AimPoint => transform;

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
