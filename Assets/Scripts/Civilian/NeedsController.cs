using System.Collections.Generic;
using UnityEngine;

public class NeedsController : MonoBehaviour
{
    [Header("Need Definitions")]
    [SerializeField] private List<NeedDefinition> startingNeeds = new();
    [SerializeField] private NeedsDatabase needsDatabase;
    [SerializeField] private NeedsActionDatabase needsActionDatabase;
    private readonly List<NeedInstance> activeNeeds = new();

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

    [Min(0.01f)]
    [SerializeField] private float waterRatePerSecond = 0.55f;

    [Min(1f)]
    [SerializeField] private float maxWaterNeed = 100f;

    [Range(0.1f, 1f)]
    [SerializeField] private float seekWaterThreshold01 = 0.55f;

    [Min(0f)]
    [SerializeField] private float dehydrationDamagePerSecond = 2f;

    public float CurrentHunger { get; private set; }
    public float CurrentTiredness { get; private set; }
    public float CurrentWaterNeed { get; private set; }

    public float HungerRatePerSecond => hungerRatePerSecond;
    public float TirednessRatePerSecond => tirednessRatePerSecond;
    public float WaterRatePerSecond => waterRatePerSecond;
    public float MaxHunger => maxHunger;
    public float MaxTiredness => maxTiredness;
    public float MaxWaterNeed => maxWaterNeed;
    public int FoodToEatPerMeal => foodToEatPerMeal;
    public float EatDurationSeconds => eatDurationSeconds;
    public float SleepDurationSeconds => sleepDurationSeconds;
    public float SeekFoodThreshold01 => seekFoodThreshold01;
    public float SeekSleepThreshold01 => seekSleepThreshold01;
    public float SeekWaterThreshold01 => seekWaterThreshold01;
    public float StarvationDamagePerSecond => starvationDamagePerSecond;
    public float ExhaustionDamagePerSecond => exhaustionDamagePerSecond;
    public float DehydrationDamagePerSecond => dehydrationDamagePerSecond;

    void Awake()
    {
        ResolveDatabases();
        ApplySurvivalRatesFromNeedDefinitions();
        InitializeNeeds();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ResolveDatabases();
            ApplySurvivalRatesFromNeedDefinitions();
        }
    }

    public void SetNeeds(List<NeedDefinition> defs)
    {
        startingNeeds = defs ?? new List<NeedDefinition>();
        ApplySurvivalRatesFromNeedDefinitions();
        InitializeNeeds();
    }

    public void ResetCivilianNeeds()
    {
        CurrentHunger = 0f;
        CurrentTiredness = 0f;
        CurrentWaterNeed = 0f;
    }

    public NeedDrivenActionType GetRecommendedAction()
    {
        if (needsActionDatabase != null && needsActionDatabase.TryGetBestAction(GetNormalizedNeedById, out NeedDrivenActionType action))
            return action;

        if (NeedsSleep())
            return NeedDrivenActionType.SeekRest;

        if (NeedsFood())
            return NeedDrivenActionType.SeekFood;

        if (NeedsWater())
            return NeedDrivenActionType.SeekWater;

        return NeedDrivenActionType.None;
    }

    public void TickCivilianNeeds(float dt, float tirednessRateMultiplier, HealthComponent health)
    {
        CurrentHunger = Mathf.Clamp(CurrentHunger + hungerRatePerSecond * dt, 0f, maxHunger);

        float tiredRate = tirednessRatePerSecond * Mathf.Max(0.1f, tirednessRateMultiplier);
        CurrentTiredness = Mathf.Clamp(CurrentTiredness + tiredRate * dt, 0f, maxTiredness);

        CurrentWaterNeed = Mathf.Clamp(CurrentWaterNeed + waterRatePerSecond * dt, 0f, maxWaterNeed);

        if (health != null)
        {
            if (CurrentHunger >= maxHunger)
                health.TakeDamage(starvationDamagePerSecond * dt);

            if (CurrentWaterNeed >= maxWaterNeed)
                health.TakeDamage(dehydrationDamagePerSecond * dt);

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
        RestoreWaterNeed(totalRestore * 0.35f);

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

    public bool NeedsFood() => CurrentHunger >= maxHunger * Mathf.Clamp01(seekFoodThreshold01);

    public bool NeedsSleep() => CurrentTiredness >= maxTiredness * Mathf.Clamp01(seekSleepThreshold01);

    public bool NeedsWater() => CurrentWaterNeed >= maxWaterNeed * Mathf.Clamp01(seekWaterThreshold01);

    public bool ShouldEatNow()
    {
        return NeedsFood() || NeedsWater();
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

    public void RestoreHunger(float amount)
    {
        CurrentHunger = Mathf.Max(0f, CurrentHunger - Mathf.Max(0f, amount));
    }

    public void RestoreTiredness(float amount)
    {
        CurrentTiredness = Mathf.Max(0f, CurrentTiredness - Mathf.Max(0f, amount));
    }

    public void RestoreWaterNeed(float amount)
    {
        CurrentWaterNeed = Mathf.Max(0f, CurrentWaterNeed - Mathf.Max(0f, amount));
    }

    void InitializeNeeds()
    {
        activeNeeds.Clear();

        if (startingNeeds == null)
            return;

        foreach (NeedDefinition def in startingNeeds)
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

    void ResolveDatabases()
    {
        if (needsDatabase == null)
            needsDatabase = GameDatabaseLoader.ResolveLoaded()?.needs;

        if (needsActionDatabase == null)
            needsActionDatabase = GameDatabaseLoader.ResolveLoaded()?.needsActions;
    }

    void ApplySurvivalRatesFromNeedDefinitions()
    {
        NeedDefinition foodNeed = FindNeedDefinition("food");
        if (foodNeed != null)
        {
            hungerRatePerSecond = Mathf.Max(0.01f, foodNeed.decayRatePerSecond);
            maxHunger = Mathf.Max(1f, foodNeed.maxValue);
            seekFoodThreshold01 = Mathf.Clamp01(foodNeed.seekThreshold01);
            starvationDamagePerSecond = Mathf.Max(0f, starvationDamagePerSecond);
        }

        NeedDefinition restNeed = FindNeedDefinition("rest");
        if (restNeed != null)
        {
            tirednessRatePerSecond = Mathf.Max(0.01f, restNeed.decayRatePerSecond);
            maxTiredness = Mathf.Max(1f, restNeed.maxValue);
            seekSleepThreshold01 = Mathf.Clamp01(restNeed.seekThreshold01);
            exhaustionDamagePerSecond = Mathf.Max(0f, exhaustionDamagePerSecond);
        }

        NeedDefinition waterNeed = FindNeedDefinition("water");
        if (waterNeed != null)
        {
            waterRatePerSecond = Mathf.Max(0.01f, waterNeed.decayRatePerSecond);
            maxWaterNeed = Mathf.Max(1f, waterNeed.maxValue);
            seekWaterThreshold01 = Mathf.Clamp01(waterNeed.seekThreshold01);
            dehydrationDamagePerSecond = Mathf.Max(0f, dehydrationDamagePerSecond);
        }
    }

    NeedDefinition FindNeedDefinition(string id)
    {
        if (needsDatabase != null && needsDatabase.TryGetById(id, out NeedDefinition fromDb) && fromDb != null)
            return fromDb;

        if (startingNeeds == null)
            return null;

        for (int i = 0; i < startingNeeds.Count; i++)
        {
            NeedDefinition need = startingNeeds[i];
            if (need == null || string.IsNullOrWhiteSpace(need.id))
                continue;

            if (string.Equals(need.id.Trim(), id, System.StringComparison.OrdinalIgnoreCase))
                return need;
        }

        return null;
    }

    float GetNormalizedNeedById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return 0f;

        string normalized = id.Trim().ToLowerInvariant();
        switch (normalized)
        {
            case "food":
            case "hunger":
                return maxHunger <= 0f ? 0f : CurrentHunger / maxHunger;
            case "rest":
            case "sleep":
            case "tiredness":
                return maxTiredness <= 0f ? 0f : CurrentTiredness / maxTiredness;
            case "water":
            case "thirst":
                return maxWaterNeed <= 0f ? 0f : CurrentWaterNeed / maxWaterNeed;
            default:
                return 0f;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        foreach (NeedInstance need in activeNeeds)
        {
            need.Tick(dt);

            if (need.definition?.behaviour != null)
                need.definition.behaviour.Tick(this, need);
        }
    }
}
