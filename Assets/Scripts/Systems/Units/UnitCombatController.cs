// =============================================================
// UnitCombatController.cs (Updated for WeaponComponent + UI)
// =============================================================

using UnityEngine;
using UnityEngine.AI;

public class UnitCombatController : MonoBehaviour
{
    public enum CombatStance { Hold, Guard, Aggressive }

    [Header("Owner")]
    public int teamID;

    [Header("Weapon")]
    public WeaponComponent weapon;

    [Header("Fallback Weapon (used when WeaponComponent is missing)")]
    public float fallbackRange = 8f;
    public float fallbackDamage = 10f;
    public float fallbackCooldown = 0.8f;
    public float fallbackProjectileSpeed = 25f;

    [Header("Target Rules")]
    public bool canAttackCivilians = false;

    [Header("Detection")]
    public LayerMask attackableLayers;

    [Header("Runtime")]
    public Attackable currentTarget;

    public CombatStance stance = CombatStance.Guard;

    private bool hasManualTarget;
    private NavMeshAgent agent;
    private float fallbackFireTimer;
    private float nextRetargetLogTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (weapon == null)
            weapon = GetComponent<WeaponComponent>();

        if (weapon == null)
            Debug.LogWarning($"[UnitCombatController] {name} has no WeaponComponent. Using fallback projectile settings.", this);
    }

    void Update()
    {
        fallbackFireTimer -= Time.deltaTime;

        if (currentTarget == null || !IsValidTarget(currentTarget))
        {
            currentTarget = null;
            hasManualTarget = false;
        }

        if (!hasManualTarget && stance != CombatStance.Hold)
            AcquireTarget();

        if (currentTarget == null)
            return;

        float attackRange = weapon != null ? weapon.range : fallbackRange;
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);

        if (dist > attackRange)
        {
            TryMoveTowardsTarget();
            return;
        }

        if (agent != null && agent.enabled)
            agent.isStopped = true;

        Vector3 toTarget = currentTarget.transform.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(toTarget),
                Time.deltaTime * 10f
            );

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

    void TryMoveTowardsTarget()
    {
        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning($"[UnitCombatController] {name} cannot move toward targets because NavMeshAgent is missing/disabled.", this);
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(currentTarget.transform.position);
    }

    float GetSearchRange()
    {
        float baseRange = weapon != null ? weapon.range : fallbackRange;
        return stance == CombatStance.Aggressive ? Mathf.Max(baseRange, 40f) : baseRange;
    }

    void LogTargetingIssue(string reason)
    {
        if (Time.time < nextRetargetLogTime)
            return;

        nextRetargetLogTime = Time.time + 3f;
        Debug.Log($"[UnitCombatController] {name} cannot acquire a target: {reason}", this);
    }

    void AcquireTarget()
    {
        float searchRange = GetSearchRange();
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange, attackableLayers);

        if (hits == null || hits.Length == 0)
        {
            LogTargetingIssue($"no colliders found in layers {attackableLayers.value} within range {searchRange:0.0}");
            currentTarget = null;
            return;
        }

        Attackable best = null;
        float bestDist = float.MaxValue;
        bool foundEnemyTeam = false;
        bool foundEnemyNotAtWar = false;
        bool foundCivilianOnly = false;

        foreach (var hit in hits)
        {
            var atk = hit.GetComponentInParent<Attackable>();
            if (atk == null || !atk.IsAlive) continue;
            if (atk.teamID == teamID) continue;

            foundEnemyTeam = true;

            if (atk.isCivilian && !canAttackCivilians)
            {
                foundCivilianOnly = true;
                continue;
            }

            if (DiplomacyManager.Instance != null && !DiplomacyManager.Instance.AreAtWar(teamID, atk.teamID))
            {
                foundEnemyNotAtWar = true;
                continue;
            }

            float d = Vector3.Distance(transform.position, atk.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = atk;
            }
        }

        if (best == null)
        {
            if (!foundEnemyTeam)
                LogTargetingIssue("all nearby attackables are same team");
            else if (foundEnemyNotAtWar)
                LogTargetingIssue("enemy teams detected but diplomacy is not set to war");
            else if (foundCivilianOnly)
                LogTargetingIssue("only civilians were found while civilian attacks are disabled");
            else
                LogTargetingIssue("no valid attackable target passed filters");
        }

        currentTarget = best;
    }

    bool IsValidTarget(Attackable a)
    {
        if (a == null || !a.IsAlive) return false;
        if (a.teamID == teamID) return false;
        if (a.isCivilian && !canAttackCivilians) return false;

        return DiplomacyManager.Instance == null ||
               DiplomacyManager.Instance.AreAtWar(teamID, a.teamID);
    }

    public void SetManualTarget(Attackable target)
    {
        if (!IsValidTarget(target))
        {
            Debug.LogWarning($"[UnitCombatController] {name} ignored manual target because it is invalid.", this);
            return;
        }

        currentTarget = target;
        hasManualTarget = true;
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
        {
            if (agent != null && agent.enabled)
                agent.isStopped = true;
            if (!hasManualTarget)
                currentTarget = null;
        }
    }

    public string GetTargetStatus()
    {
        if (currentTarget == null)
            return "None";

        return currentTarget.name + (hasManualTarget ? " (Ordered)" : "");
    }

    public void ToggleAttackCivilians()
    {
        canAttackCivilians = !canAttackCivilians;
    }
}
