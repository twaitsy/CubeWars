// ============================================================================
// Attackable.cs
//
// PURPOSE:
// - Universal health + damage interface for ANY object that can be attacked.
// - Used by units, civilians, buildings, turrets, and future entities.
// - Provides a consistent API for combat, AI, UI, and projectiles.
// ============================================================================

using UnityEngine;

public class Attackable : MonoBehaviour, IHasHealth, IAttackable
{
    [Header("Team & Classification")]
    public int teamID;
    public bool isCivilian;
    public bool isBuilding;

    [Header("Health")]
    public float maxHealth = 100f;
    float currentHealth;

    [Header("Repair")]
    public bool canBeRepaired = true;

    IAttackable attackProxy;
    IHasHealth healthProxy;
    Civilian civilianProxy;

    public int TeamID => teamID;
    public Transform AimPoint => transform;
    public bool IsAlive => CurrentHealth > 0f;
    public float CurrentHealth => healthProxy != null ? healthProxy.CurrentHealth : currentHealth;
    public float MaxHealth => healthProxy != null ? healthProxy.MaxHealth : maxHealth;
    public bool IsDamaged => IsAlive && CurrentHealth < MaxHealth;

    void Awake()
    {
        var unit = GetComponent<Unit>();
        var civ = GetComponent<Civilian>();
        var building = GetComponent<Building>();

        if (unit != null)
        {
            teamID = unit.teamID;
            attackProxy = unit;
            healthProxy = unit;
        }
        else if (civ != null)
        {
            teamID = civ.teamID;
            civilianProxy = civ;
            healthProxy = civ;
            isCivilian = true;
        }
        else if (building != null)
        {
            teamID = building.teamID;
            attackProxy = building;
            healthProxy = building;
            isBuilding = true;
        }

        if (healthProxy == null)
            currentHealth = maxHealth;
    }

    public void Repair(float amount)
    {
        if (!canBeRepaired || !IsAlive || healthProxy != null) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void TakeDamage(float dmg)
    {
        if (!IsAlive) return;

        if (attackProxy != null)
        {
            attackProxy.TakeDamage(dmg);
            return;
        }

        if (civilianProxy != null)
        {
            civilianProxy.TakeDamage(dmg);
            return;
        }

        currentHealth -= dmg;

        if (AlertManager.Instance != null)
            AlertManager.Instance.Push($"{name} is under attack!");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (AlertManager.Instance != null)
            AlertManager.Instance.Push($"{name} destroyed");

        Destroy(gameObject);
    }
}
