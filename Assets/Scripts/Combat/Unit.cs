using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour, IHasHealth, IAttackable, ICommandable
{
    [Header("Identity")]
    public string unitDefinitionId;

    [Header("Team")]
    public int teamID;

    [Header("Combat")]
    public bool combatEnabled = true;
    public float attackRange = 5f;
    public float damage = 10f;
    public float attackCooldown = 0.75f;
    public float armor = 0f;

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

        ApplyDefinitionIfAvailable();
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

        float effectiveDamage = Mathf.Max(0f, amount - armor);
        CurrentHealth -= effectiveDamage;
        if (CurrentHealth <= 0f)
        {
            CurrentHealth = 0f;
            Die();
        }
    }

    public void ApplyDefinitionIfAvailable()
    {
        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null || !loaded.TryGetUnitById(unitDefinitionId, out var def) || def == null)
            return;

        if (def.maxHealth > 0)
        {
            MaxHealth = def.maxHealth;
            CurrentHealth = Mathf.Clamp(CurrentHealth <= 0f ? MaxHealth : CurrentHealth, 0f, MaxHealth);
        }

        if (def.attackRange > 0f)
            attackRange = def.attackRange;

        if (def.attackDamage > 0)
            damage = def.attackDamage;

        if (def.attackCooldown > 0f)
            attackCooldown = def.attackCooldown;

        armor = Mathf.Max(0f, def.armor);

        if (agent != null && def.moveSpeed > 0f)
            agent.speed = def.moveSpeed;

        var combat = GetComponent<UnitCombatController>();
        combat?.ApplyUnitStats(this);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
