using System.Collections.Generic;
using UnityEngine;

public class NeedsController : MonoBehaviour
{
    // ---------------------------------------------------------
    // Configurable Need Definitions (Optional System)
    // ---------------------------------------------------------
    [Header("Need Definitions")]
    [SerializeField] private List<NeedDefinition> startingNeeds = new();
    private readonly List<NeedInstance> activeNeeds = new();

    // ---------------------------------------------------------
    // Survival Needs (Used by Civilian)
    // ---------------------------------------------------------
    [Header("Civilian Survival Needs")]

    [Min(0.01f)]
    [SerializeField] private float hungerRatePerSecond = 0.45f;

    [Min(1f)]
    [SerializeField] private float maxHunger = 100f;

    [Range(0.1f, 1f)]
    [SerializeField] private float seekFoodThreshold01 = 0.55f;

    [Min(1)]
    [SerializeField] private int foodToEatPerMeal = 10;

    [Min(0.1f)]
    [SerializeField] private float eatDurationSeconds = 1.2f;

    [Min(0f)]
    [SerializeField] private float starvationDamagePerSecond = 2f;


    [Min(0.01f)]
    [SerializeField] private float tirednessRatePerSecond = 0.3f;

    [Min(1f)]
    [SerializeField] private float maxTiredness = 100f;

    [Range(0.1f, 1f)]
    [SerializeField] private float seekSleepThreshold01 = 0.6f;

    [Min(0.1f)]
    [SerializeField] private float sleepDurationSeconds = 5f;

    [Min(0f)]
    [SerializeField] private float exhaustionDamagePerSecond = 1f;

    // ---------------------------------------------------------
    // Runtime Values
    // ---------------------------------------------------------
    public float CurrentHunger { get; private set; }
    public float CurrentTiredness { get; private set; }

    // ---------------------------------------------------------
    // Public Accessors (Used by Civilian)
    // ---------------------------------------------------------
    public float HungerRatePerSecond => hungerRatePerSecond;
    public float TirednessRatePerSecond => tirednessRatePerSecond;

    public float MaxHunger => maxHunger;
    public float MaxTiredness => maxTiredness;

    public int FoodToEatPerMeal => foodToEatPerMeal;
    public float EatDurationSeconds => eatDurationSeconds;
    public float SleepDurationSeconds => sleepDurationSeconds;

    public float SeekFoodThreshold01 => seekFoodThreshold01;
    public float SeekSleepThreshold01 => seekSleepThreshold01;

    public float StarvationDamagePerSecond => starvationDamagePerSecond;
    public float ExhaustionDamagePerSecond => exhaustionDamagePerSecond;

    // ---------------------------------------------------------
    // Unity Lifecycle
    // ---------------------------------------------------------
    void Awake()
    {
        InitializeNeeds();
    }

    // ---------------------------------------------------------
    // Initialization
    // ---------------------------------------------------------
    public void SetNeeds(List<NeedDefinition> defs)
    {
        startingNeeds = defs ?? new List<NeedDefinition>();
        InitializeNeeds();
    }

    public void ResetCivilianNeeds()
    {
        CurrentHunger = 0f;
        CurrentTiredness = 0f;
    }

    private void InitializeNeeds()
    {
        activeNeeds.Clear();

        if (startingNeeds == null)
            return;

        foreach (var def in startingNeeds)
        {
            if (def == null)
                continue;

            activeNeeds.Add(new NeedInstance
            {
                definition = def,
                currentValue = def.maxValue
            });
        }
    }

    // ---------------------------------------------------------
    // Main Tick (Called by Civilian)
    // ---------------------------------------------------------
    public void TickCivilianNeeds(float dt, float tirednessRateMultiplier, HealthComponent health)
    {
        // Hunger
        CurrentHunger = Mathf.Clamp(
            CurrentHunger + hungerRatePerSecond * dt,
            0f,
            maxHunger
        );

        // Tiredness
        float tiredRate = tirednessRatePerSecond * Mathf.Max(0.1f, tirednessRateMultiplier);
        CurrentTiredness = Mathf.Clamp(
            CurrentTiredness + tiredRate * dt,
            0f,
            maxTiredness
        );

        // Damage from starvation/exhaustion
        if (health != null)
        {
            if (CurrentHunger >= maxHunger)
                health.TakeDamage(starvationDamagePerSecond * dt);

            if (CurrentTiredness >= maxTiredness)
                health.TakeDamage(exhaustionDamagePerSecond * dt);
        }
    }
    public float BeginEating(FoodDefinition food, int amount)
    {
        float eatTime = Mathf.Max(0.1f, food != null ? food.eatTime : EatDurationSeconds);
        float restorePerUnit = food != null ? Mathf.Max(1, food.hungerRestore) : 1f;

        float totalRestore = restorePerUnit * Mathf.Max(1, amount);
        RestoreHunger(totalRestore);

        return eatTime;
    }
    public float TickSleep(float dt)
    {
        float restoreRate = MaxTiredness / Mathf.Max(0.1f, SleepDurationSeconds);
        RestoreTiredness(restoreRate * dt);

        return CurrentTiredness;
    }

    public void CompleteSleep()
    {
        RestoreTiredness(MaxTiredness);
    }
    // ---------------------------------------------------------
    // Queries
    // ---------------------------------------------------------
    public bool NeedsFood() =>
        CurrentHunger >= maxHunger * Mathf.Clamp01(seekFoodThreshold01);

    public bool NeedsSleep() =>
        CurrentTiredness >= maxTiredness * Mathf.Clamp01(seekSleepThreshold01);
    public bool ShouldEatNow()
    {
        return NeedsFood();
    }

    public bool ShouldSleepNow()
    {
        return NeedsSleep();
    }

    public bool IsHungryEnoughToInterruptWork()
    {
        return CurrentHunger >= MaxHunger * 0.85f;
    }

    public bool IsTiredEnoughToInterruptWork()
    {
        return CurrentTiredness >= MaxTiredness * 0.85f;
    }
    // ---------------------------------------------------------
    // Restoration
    // ---------------------------------------------------------
    public void RestoreHunger(float amount)
    {
        CurrentHunger = Mathf.Max(0f, CurrentHunger - Mathf.Max(0f, amount));
    }

    public void RestoreTiredness(float amount)
    {
        CurrentTiredness = Mathf.Max(0f, CurrentTiredness - Mathf.Max(0f, amount));
    }

    // ---------------------------------------------------------
    // Optional NeedDefinition System Tick
    // ---------------------------------------------------------
    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var need in activeNeeds)
        {
            need.Tick(dt);

            if (need.definition?.behaviour != null)
                need.definition.behaviour.Tick(this, need);
        }
    }
}