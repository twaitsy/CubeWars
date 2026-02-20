using UnityEngine;

/// <summary>
/// Base class for all buildings in Cube Wars.
///
/// DEPENDENCIES:
/// - ITargetable / IAttackable / IHasHealth:
///     Used by combat, AI targeting, and UI.
/// - ConstructionSite:
///     Spawns final building prefabs that inherit from Building.
/// - BuildPlacementManager:
///     Assigns teamID when placing buildings.
/// - TeamVisual:
///     Applies team colors.
/// - ResourceStorageProvider / ResourceDropoff:
///     Many buildings include these; teamID must be consistent.
///
/// RESPONSIBILITIES:
/// - Store team ownership
/// - Handle health and death
/// - Provide combat targeting info
///
/// IMPORTANT:
/// - Does NOT delete teams.
/// - Only destroys THIS building when health reaches zero.
/// </summary>
[RequireComponent(typeof(BuildingInteractionPointController))]
public abstract class Building : MonoBehaviour, ITargetable, IHasHealth, IAttackable
{
    [Header("Identity")]
    public string buildingDefinitionId;

    [Header("Team")]
    public int teamID;

    [Header("Health")]
    public float maxHealth = 300f;
    public float currentHealth;

    // ITargetable + IAttackable
    public int TeamID => teamID;
    public bool IsAlive => currentHealth > 0;
    public Transform AimPoint => transform;

    // IHasHealth
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    protected virtual void Start()
    {
        ApplyDefinitionIfAvailable();
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    protected virtual void Die()
    {
        // Only destroys THIS building, not the team.
        Destroy(gameObject);
    }

    public virtual void ApplyDefinitionIfAvailable()
    {
        ResolveDefinitionId();

        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null || !loaded.TryGetBuildingById(buildingDefinitionId, out var def) || def == null)
            return;

        ApplyDefinition(def);
    }

    protected virtual void ApplyDefinition(BuildingDefinition def)
    {
        if (def == null)
            return;

        if (def.maxHealth > 0)
            maxHealth = def.maxHealth;
    }

    void ResolveDefinitionId()
    {
        if (!string.IsNullOrWhiteSpace(buildingDefinitionId))
            return;

        BuildItemInstance instance = GetComponent<BuildItemInstance>();
        if (instance != null && !string.IsNullOrWhiteSpace(instance.itemId))
        {
            buildingDefinitionId = instance.itemId;
            return;
        }

        string sanitizedName = name.Replace("(Clone)", string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(sanitizedName))
            buildingDefinitionId = sanitizedName;
    }

    public virtual void Demolish()
    {
        if (!IsAlive) return;
        currentHealth = 0f;
        Die();
    }
}
