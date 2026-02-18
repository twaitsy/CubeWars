using System.Collections.Generic;
using UnityEngine;

public class NeedsController : MonoBehaviour
{
    [Header("Need Definitions")]
    [SerializeField] private List<NeedDefinition> startingNeeds = new();
    private readonly List<NeedInstance> activeNeeds = new();

    [Header("Civilian Survival Needs")]
    [Min(0.01f)] [SerializeField] private float hungerRatePerSecond = 0.45f;
    [Min(1f)] [SerializeField] private float maxHunger = 100f;
    [Range(0.1f, 1f)] [SerializeField] private float seekFoodThreshold01 = 0.55f;
    [Min(1)] [SerializeField] private int foodToEatPerMeal = 10;
    [Min(0.1f)] [SerializeField] private float eatDurationSeconds = 1.2f;
    [Min(0f)] [SerializeField] private float starvationDamagePerSecond = 2f;

    [Min(0.01f)] [SerializeField] private float tirednessRatePerSecond = 0.3f;
    [Min(1f)] [SerializeField] private float maxTiredness = 100f;
    [Range(0.1f, 1f)] [SerializeField] private float seekSleepThreshold01 = 0.6f;
    [Min(0.1f)] [SerializeField] private float sleepDurationSeconds = 5f;
    [Min(0f)] [SerializeField] private float exhaustionDamagePerSecond = 1f;

    public float CurrentHunger { get; private set; }
    public float CurrentTiredness { get; private set; }

    public float TirednessRatePerSecond => tirednessRatePerSecond;
    public float MaxHunger => maxHunger;
    public float MaxTiredness => maxTiredness;
    public int FoodToEatPerMeal => foodToEatPerMeal;
    public float EatDurationSeconds => eatDurationSeconds;
    public float SleepDurationSeconds => sleepDurationSeconds;

    void Awake()
    {
        InitializeNeeds();
    }

    public void SetNeeds(List<NeedDefinition> defs)
    {
        startingNeeds = defs;
        InitializeNeeds();
    }

    public void ResetCivilianNeeds()
    {
        CurrentHunger = 0f;
        CurrentTiredness = 0f;
    }

    public void TickCivilianNeeds(float dt, float tirednessRateMultiplier, HealthComponent health)
    {
        CurrentHunger = Mathf.Clamp(CurrentHunger + hungerRatePerSecond * dt, 0f, maxHunger);
        CurrentTiredness = Mathf.Clamp(CurrentTiredness + tirednessRatePerSecond * Mathf.Max(0.1f, tirednessRateMultiplier) * dt, 0f, maxTiredness);

        if (health == null)
            return;

        if (CurrentHunger >= maxHunger)
            health.TakeDamage(starvationDamagePerSecond * dt);

        if (CurrentTiredness >= maxTiredness)
            health.TakeDamage(exhaustionDamagePerSecond * dt);
    }

    public bool NeedsFood() => CurrentHunger >= maxHunger * Mathf.Clamp01(seekFoodThreshold01);
    public bool NeedsSleep() => CurrentTiredness >= maxTiredness * Mathf.Clamp01(seekSleepThreshold01);

    public void RestoreHunger(float amount)
    {
        CurrentHunger = Mathf.Max(0f, CurrentHunger - Mathf.Max(0f, amount));
    }

    public void RestoreTiredness(float amount)
    {
        CurrentTiredness = Mathf.Max(0f, CurrentTiredness - Mathf.Max(0f, amount));
    }

    private void InitializeNeeds()
    {
        activeNeeds.Clear();

        foreach (var def in startingNeeds)
        {
            if (def == null) continue;

            activeNeeds.Add(new NeedInstance
            {
                definition = def,
                currentValue = def.maxValue
            });
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (var need in activeNeeds)
        {
            need.Tick(dt);

            if (need.definition.behaviour != null)
                need.definition.behaviour.Tick(this, need);
        }
    }
}
