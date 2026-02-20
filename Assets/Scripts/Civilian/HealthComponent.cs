using UnityEngine;

/// <summary>
/// Standalone health module for any unit.
/// Owns all health state, initialization, team assignment,
/// and definition-driven configuration.
/// </summary>
[DisallowMultipleComponent]
public class HealthComponent : MonoBehaviour, IHasHealth
{
    [Header("Team")]
    [SerializeField] private int teamID;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    public int TeamID => teamID;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0f;

    // Optional events for future systems (UI, death handlers, etc.)
    public System.Action<float> OnDamaged;
    public System.Action<float> OnHealed;
    public System.Action OnDied;

    void Awake()
    {
        // Ensure health is valid on spawn
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void Start()
    {
        // If currentHealth was never initialized, initialize it now
        if (currentHealth <= 0f)
            currentHealth = maxHealth;
    }

    // ---------------------------------------------------------
    // Initialization & Definition Application
    // ---------------------------------------------------------

    /// <summary>
    /// Called by unit initialization systems (e.g., GameDatabaseLoader).
    /// </summary>
    public void ApplyDefinition(UnitDefinition def)
    {
        if (def == null)
            return;

        SetMaxHealth(def.maxHealth);
    }


    /// <summary>
    /// Assigns the team ID for this unit.
    /// </summary>
    public void SetTeam(int team)
    {
        teamID = team;
    }

    /// <summary>
    /// Sets max health and clamps current health accordingly.
    /// </summary>
    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(1f, value);
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    // ---------------------------------------------------------
    // Damage & Healing
    // ---------------------------------------------------------

    public void TakeDamage(float amount)
    {
        if (!IsAlive)
            return;

        float prev = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - Mathf.Abs(amount));

        OnDamaged?.Invoke(prev - currentHealth);

        if (currentHealth <= 0f)
            OnDied?.Invoke();
    }

    public void Heal(float amount)
    {
        if (!IsAlive)
            return;

        float prev = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + Mathf.Abs(amount));

        OnHealed?.Invoke(currentHealth - prev);
    }

    // ---------------------------------------------------------
    // Utility
    // ---------------------------------------------------------

    public void Kill()
    {
        if (!IsAlive)
            return;

        currentHealth = 0f;
        OnDied?.Invoke();
    }
}