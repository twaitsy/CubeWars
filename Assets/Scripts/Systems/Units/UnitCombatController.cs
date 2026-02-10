using UnityEngine;
using UnityEngine.AI;

public class UnitCombatController : MonoBehaviour
{
    public enum CombatStance { Hold, Guard, Aggressive }
    public enum BehaviorState { Idle, Moving, AttackMoving, Chasing, Attacking, Holding }

    [Header("Owner")]
    public int teamID;

    [Header("Weapon")]
    public WeaponComponent weapon;
    public float fallbackRange = 8f;
    public float fallbackDamage = 10f;
    public float fallbackCooldown = 0.5f;
    public float fallbackProjectileSpeed = 25f;

    [Header("Awareness")]
    [Min(1f)] public float visionRange = 16f;
    public bool canAttackCivilians = false;
    public LayerMask attackableLayers = ~0;
    [Min(0.1f)] public float targetRefreshSeconds = 0.1f;

    [Header("Runtime")]
    public Attackable currentTarget;
    public CombatStance stance = CombatStance.Guard;
    public BehaviorState behaviorState = BehaviorState.Idle;

    public bool ShowRangeGizmos => showRangeGizmos;

    NavMeshAgent agent;
    bool hasManualTarget;
    bool attackMoveActive;
    Vector3 attackMoveDestination;
    float fallbackFireTimer;
    float retargetTimer;
    bool showRangeGizmos;

    float AttackRange => weapon != null ? weapon.range : fallbackRange;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (weapon == null)
            weapon = GetComponent<WeaponComponent>();
    }

    void Update()
    {
        fallbackFireTimer -= Time.deltaTime;
        retargetTimer -= Time.deltaTime;

        if (currentTarget == null || !IsValidTarget(currentTarget) || OutOfVision(currentTarget))
        {
            currentTarget = null;
            hasManualTarget = false;
        }

        if (ShouldAutoAcquire())
            AcquireTarget();

        if (currentTarget == null)
        {
            UpdatePassiveState();
            return;
        }

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        float desiredStop = Mathf.Max(0.4f, AttackRange * 0.9f);

        if (dist > desiredStop)
        {
            if (stance != CombatStance.Hold)
            {
                MoveToward(currentTarget.transform.position);
                behaviorState = BehaviorState.Chasing;
            }
            return;
        }

        StopMove();
        behaviorState = BehaviorState.Attacking;
        FaceTarget(currentTarget.transform.position);

        if (weapon != null)
        {
            if (weapon.CanFire)
                weapon.FireAtTarget(currentTarget, teamID);
            return;
        }

        if (fallbackFireTimer <= 0f)
        {
            fallbackFireTimer = fallbackCooldown;
            WeaponComponent.SpawnFallbackProjectile(transform, currentTarget, fallbackDamage, teamID, fallbackProjectileSpeed);
        }
    }

    bool ShouldAutoAcquire()
    {
        if (stance == CombatStance.Hold || hasManualTarget)
            return false;

        if (retargetTimer > 0f)
            return false;

        if (attackMoveActive)
            return true;

        return currentTarget == null;
    }

    void UpdatePassiveState()
    {
        if (attackMoveActive)
        {
            float dist = Vector3.Distance(transform.position, attackMoveDestination);
            if (dist <= Mathf.Max(0.6f, agent != null ? agent.stoppingDistance + 0.25f : 0.6f))
            {
                attackMoveActive = false;
                behaviorState = BehaviorState.Idle;
                StopMove();
            }
            else
            {
                MoveToward(attackMoveDestination);
                behaviorState = BehaviorState.AttackMoving;
            }
            return;
        }

        behaviorState = stance == CombatStance.Hold ? BehaviorState.Holding : BehaviorState.Idle;
    }

    bool OutOfVision(Attackable target)
    {
        return target != null && Vector3.Distance(transform.position, target.transform.position) > visionRange;
    }

    void AcquireTarget()
    {
        retargetTimer = targetRefreshSeconds;
        var hits = Physics.OverlapSphere(transform.position, visionRange, attackableLayers);

        Attackable best = null;
        float bestScore = float.MaxValue;

        foreach (var hit in hits)
        {
            var atk = hit.GetComponentInParent<Attackable>();
            if (!IsValidTarget(atk))
                continue;

            float dist = Vector3.Distance(transform.position, atk.transform.position);
            float score = dist;

            if (dist <= AttackRange)
                score -= 3f;

            if (attackMoveActive)
            {
                float pathBias = Vector3.Distance(atk.transform.position, attackMoveDestination);
                score += pathBias * 0.25f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                best = atk;
            }
        }

        if (best != null)
            currentTarget = best;
    }

    bool IsValidTarget(Attackable a)
    {
        if (a == null || !a.IsAlive) return false;
        if (a.teamID == teamID) return false;
        if (a.isCivilian && !canAttackCivilians) return false;
        return DiplomacyManager.Instance == null || DiplomacyManager.Instance.AreAtWar(teamID, a.teamID);
    }

    void MoveToward(Vector3 position)
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = false;
        if (!agent.hasPath || Vector3.Distance(agent.destination, position) > 0.5f)
            agent.SetDestination(position);
    }

    void StopMove()
    {
        if (agent != null && agent.enabled)
            agent.isStopped = true;
    }

    void FaceTarget(Vector3 worldTarget)
    {
        Vector3 toTarget = worldTarget - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.001f) return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), Time.deltaTime * 14f);
    }

    public void SetManualTarget(Attackable target)
    {
        if (!IsValidTarget(target)) return;
        currentTarget = target;
        hasManualTarget = true;
        attackMoveActive = false;
    }

    public void ClearManualTarget()
    {
        hasManualTarget = false;
        currentTarget = null;
    }

    public void SetStance(CombatStance newStance)
    {
        stance = newStance;
        if (stance == CombatStance.Hold)
            StopMove();
    }

    public void ToggleAttackCivilians() => canAttackCivilians = !canAttackCivilians;

    public string GetTargetStatus() => currentTarget == null ? "None" : currentTarget.name + (hasManualTarget ? " (Ordered)" : "");

    public void IssueMoveOrder(Vector3 worldPos)
    {
        hasManualTarget = false;
        currentTarget = null;
        attackMoveActive = false;
        MoveToward(worldPos);
        behaviorState = BehaviorState.Moving;
    }

    public void IssueAttackMoveOrder(Vector3 worldPos)
    {
        attackMoveActive = true;
        hasManualTarget = false;
        attackMoveDestination = worldPos;
        MoveToward(worldPos);
        behaviorState = BehaviorState.AttackMoving;
    }

    public void CancelCurrentEngagement()
    {
        hasManualTarget = false;
        currentTarget = null;
        attackMoveActive = false;
    }

    public void ToggleRangeGizmos() => showRangeGizmos = !showRangeGizmos;

    void OnDrawGizmosSelected()
    {
        if (!showRangeGizmos) return;

        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, weapon != null ? weapon.range : fallbackRange);
    }
}
