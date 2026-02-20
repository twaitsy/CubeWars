using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(CarryingController))]
[RequireComponent(typeof(GatheringController))]
[RequireComponent(typeof(NeedsController))]
[RequireComponent(typeof(HousingController))]
[RequireComponent(typeof(ConstructionWorkerControl))]
[RequireComponent(typeof(ToolController))]
[RequireComponent(typeof(JobController))]
[RequireComponent(typeof(TrainingController))]
[RequireComponent(typeof(UpgradeController))]

public class Civilian : MonoBehaviour, ITargetable
{
    [Header("Identity")]
    public string unitDefinitionId = "civilian";
    [Header("Team")]
    public int teamID;
    public bool IsAlive => health != null && health.IsAlive;


    [Header("Job (Unified Registry)")]
    public JobDefinition jobDefinition;

    public CivilianJobType jobType = CivilianJobType.Gatherer;
    public CivilianJobType JobType => jobType;

    private ResourceStorageContainer targetStorage; 
    public ConstructionSite CurrentAssignedSite { get; set; }
    public ConstructionSite CurrentDeliverySite { get; set; }
    public ResourceDefinition CarriedType => carryingController != null ? carryingController.CarriedResource : null;
    public int CarriedAmount => carryingController != null ? carryingController.CarriedAmount : 0;
    private GatheringController gatheringController;

    private ResourceDropoff targetDropoff;
    public bool HasJob;

    private float searchTimer;

    private ConstructionSite targetSite;

    private CraftingBuilding targetCraftingBuilding;
    private Transform targetWorkPoint;
    private bool manualCraftingAssignment;

    public CraftingBuilding AssignedCraftingBuilding => targetCraftingBuilding;
    public string CurrentTaskLabel => BuildTaskLabel();
    public string CurrentStateDetails => BuildStateDetails();
    public float CraftingProgress => targetCraftingBuilding != null ? targetCraftingBuilding.CraftProgress01 : 0f;
    public bool IsAtAssignedCraftingWorkPoint => state == State.CraftingAtWorkPoint && movementController.HasArrived();
    public float CurrentHunger => needsController != null ? needsController.CurrentHunger : 0f;
    public float CurrentTiredness => needsController != null ? needsController.CurrentTiredness : 0f;
    public float CurrentWaterNeed => needsController != null ? needsController.CurrentWaterNeed : 0f;
    public House AssignedHouse => housingController != null ? housingController.AssignedHouse : null;
    public FoodDatabase foodDatabase;
    public ResourcesDatabase resourcesDatabase;

    // IHasHealth / ITargetable style properties
    public int TeamID => teamID;
    public enum State
    {
        Idle,

        SeekingFoodStorage,
        Eating,
        SeekingHouse,
        Sleeping,

        SearchingNode,
        GoingToNode,
        Gathering,

        FindDepositStorage,
        GoingToDepositStorage,
        Depositing,

        SearchingBuildSite,
        GoingToBuildSite,
        Building,

        SearchingSupplySite,
        GoingToPickupStorage,
        PickingUp,
        GoingToDeliverSite,
        Delivering,

        FetchingCraftInput,
        DeliveringCraftInput,
        GoingToWorkPoint,
        CraftingAtWorkPoint,
        CollectingCraftOutput,
        DeliveringCraftOutput
    }

    private State state;
    private State stateBeforeNeed;
    private readonly Dictionary<State, CivilianStateBase> states = new();
    private CivilianStateBase currentStateHandler;

    public void SetState(State newState)
    {
        if (state == newState && currentStateHandler != null)
            return;

        currentStateHandler?.Exit();
        state = newState;

        if (states.TryGetValue(newState, out var nextState))
        {
            currentStateHandler = nextState;
            currentStateHandler.Enter();
            return;
        }

        currentStateHandler = null;
    }

    public string CurrentState => state.ToString();
    public string CurrentTargetName
    {
        get
        {
            if (targetCraftingBuilding != null)
                return SanitizeName(targetCraftingBuilding.name);

            if (targetSite != null)
                return SanitizeName(targetSite.name);

            var node = gatheringController?.CurrentNode;
            if (node != null)
                return SanitizeName(node.name);

            if (targetStorage != null)
                return SanitizeName(targetStorage.name);

            return "None";
        }
    }

    // Registration timing: teamID is often assigned right after Instantiate (before Start).
    // So we register with JobManager in Start, and only re-register on OnEnable if Start has run.
    private bool started;
    private bool registeredWithJobManager;

    private ResourceDefinition carriedResource;
    private int carriedAmount;
    private float currentHunger;

    private float currentTiredness;
    private float needActionTimer;
    private bool hasStoredRoleState;
    private ResourceStorageContainer targetFoodStorage;
    private ResourceDefinition targetFoodResource;
    private FoodDefinition targetFoodDefinition;
    private int pendingFoodAmount;
    private float currentEatDurationSeconds;
    private readonly HashSet<CivilianToolType> equippedTools = new();
    private float toolPickupTimer;
    private float idleWanderTimer;
    private Vector3 idleWanderTarget;
    private float idleNoTaskTimer;
    private float stalledAtWorkPointTimer;
    private HealthComponent health;
    private MovementController movementController;
    private CarryingController carryingController;
    private ConstructionWorkerControl constructionWorkerControl;
    private NeedsController needsController;
    private HousingController housingController;

    public float speed => movementController != null ? movementController.MoveSpeed : 2.5f;
    public float stopDistance => movementController != null ? movementController.StopDistance : 1.2f;
    private bool useRoadBonus => movementController != null && movementController.UseRoadBonus;
    private float roadSpeedMultiplier => movementController != null ? movementController.RoadSpeedMultiplier : 1.2f;
    public int carryCapacity => carryingController != null ? carryingController.Capacity : 30;
    public float gatherTickSeconds => gatheringController != null ? gatheringController.gatherInterval : 1f;
    public int harvestPerTick => gatheringController != null ? gatheringController.harvestPerTick : 1;
    private float searchRetrySeconds => 1.5f;
    private bool buildersCanHaulMaterials => constructionWorkerControl == null || constructionWorkerControl.CanHaulMaterials;
    public int foodToEatPerMeal => needsController != null ? needsController.FoodToEatPerMeal : 10;
    private float eatDurationSeconds => needsController != null ? needsController.EatDurationSeconds : 1.2f;
    public float maxTiredness => needsController != null ? needsController.MaxTiredness : 100f;
    public float maxHunger => needsController != null ? needsController.MaxHunger : 100f;
    public float maxWaterNeed => needsController != null ? needsController.MaxWaterNeed : 100f;
    public float hungerRatePerSecond => needsController != null ? needsController.HungerRatePerSecond : 0f;
    public float tirednessRatePerSecond => needsController != null ? needsController.TirednessRatePerSecond : 0f;
    public float waterRatePerSecond => needsController != null ? needsController.WaterRatePerSecond : 0f;
    private float sleepDurationSeconds => needsController != null ? needsController.SleepDurationSeconds : 5f;
    void Awake()
    {
        health = GetComponent<HealthComponent>();
        movementController = GetComponent<MovementController>();
        carryingController = GetComponent<CarryingController>();
        gatheringController = GetComponent<GatheringController>();
        needsController = GetComponent<NeedsController>();
        housingController = GetComponent<HousingController>();
        constructionWorkerControl = GetComponent<ConstructionWorkerControl>();
        InitializeStateMachine();
        CivilianRegistry.Register(this);
    }

    void OnEnable()
    {
        // Only register here if Start has already run (prevents wrong-team registration on Instantiate)
        if (started)
        {
            RegisterWithJobManager();
            CraftingJobManager.Instance?.RegisterCivilian(this);
            WorkerTaskDispatcher.Instance?.RegisterWorker(this);
        }
    }

    void OnDisable()
    {
        // Stop any gathering activity cleanly via the new controller
        if (gatheringController != null)
            gatheringController.StopGathering();

        housingController?.ClearAssignedHouse();

        if (registeredWithJobManager)
            UnregisterFromJobManager();

        CraftingJobManager.Instance?.UnregisterCivilian(this);
        WorkerTaskDispatcher.Instance?.UnregisterWorker(this);
        CivilianRegistry.Unregister(this);
    }

    void Start()
    {
        if (health != null)
            health.SetTeam(teamID);

        ApplyDatabaseDefinition(); // now calls health.ApplyDefinition()

        needsController?.ResetCivilianNeeds();
        currentHunger = needsController != null ? needsController.CurrentHunger : 0f;
        currentTiredness = needsController != null ? needsController.CurrentTiredness : 0f;

        if (jobDefinition != null)
            jobType = jobDefinition.defaultJobType;

        SetJobType(jobType);

        started = true;
        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);
        WorkerTaskDispatcher.Instance?.RegisterWorker(this);
        CivilianRegistry.Register(this);

        housingController?.TryAssignHouseIfNeeded();
    }


    private void ApplyDatabaseDefinition()
    {
        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null)
        {
            Debug.LogWarning($"[{nameof(Civilian)}] {name}: GameDatabaseLoader.Loaded is null; unitDefinitionId '{unitDefinitionId}' cannot be validated.", this);
            return;
        }

        if (!loaded.TryGetUnitById(unitDefinitionId, out var def) || def == null)
        {
            Debug.LogWarning($"[{nameof(Civilian)}] {name}: unitDefinitionId '{unitDefinitionId}' was not found in UnitsDatabase.", this);
            return;
        }

        // Health
        health?.ApplyDefinition(def);

        // Movement
        movementController?.SetMoveSpeed(def.moveSpeed);

        // Carrying
        carryingController?.SetCapacity(def.carryCapacity);

        // Gathering
        if (gatheringController != null)
        {
            gatheringController.harvestPerTick = Mathf.Max(1, Mathf.RoundToInt(def.gatherSpeed));
            gatheringController.gatherInterval = 1f / Mathf.Max(0.1f, def.gatherSpeed);
        }

        // Building
        constructionWorkerControl?.SetBuildSpeed(def.buildSpeed);

        // Combat
        GetComponent<CombatController>()?.SetCombatStats(
            def.attackDamage, def.attackRange, def.attackCooldown, def.armor);

        // Tools
        if (def.startingTools != null)
            GetComponent<ToolController>()?.SetStartingTools(def.startingTools);

        // Needs
        if (def.needs != null)
            needsController?.SetNeeds(def.needs);

        // Jobs
        GetComponent<JobController>()?.SetJobType(def.jobType);

        // Training
        GetComponent<TrainingController>()?.SetTraining(
            def.trainingCost, def.trainingTime, def.trainedAt);

        // Upgrades
        GetComponent<UpgradeController>()?.SetUpgradeTarget(def.upgradeTo);
    }

    void RegisterWithJobManager()
    {
        if (registeredWithJobManager) return;
        if (JobManager.Instance == null) return;
        JobManager.Instance.RegisterCivilian(this);
        registeredWithJobManager = true;
    }

    void UnregisterFromJobManager()
    {
        if (!registeredWithJobManager) return;
        if (JobManager.Instance != null)
            JobManager.Instance.UnregisterCivilian(this);
        registeredWithJobManager = false;
    }

    /// <summary>
    /// Call this if you change teamID at runtime and want the TaskBoard counts to move teams.
    /// </summary>
    public void RefreshJobManagerRegistration()
    {
        if (!started) return;

        if (registeredWithJobManager)
            UnregisterFromJobManager();

        CraftingJobManager.Instance?.UnregisterCivilian(this);
        WorkerTaskDispatcher.Instance?.UnregisterWorker(this);
        CivilianRegistry.Unregister(this);

        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);
        WorkerTaskDispatcher.Instance?.RegisterWorker(this);
    }

    public void SetTeamID(int newTeamID)
    {
        if (teamID == newTeamID) return;
        teamID = newTeamID;
        RefreshJobManagerRegistration();
    }

    void Update()
    {
        if (health != null && !health.IsAlive)
            return;

        TickNeeds();

        movementController.ApplyMovement(GetMovementSpeedMultiplier());

        currentStateHandler?.Tick();


    }



    void TickNeeds()
    {
        if (state == State.CraftingAtWorkPoint && movementController.HasArrived() && targetCraftingBuilding != null && targetCraftingBuilding.State == CraftingBuilding.ProductionState.InProgress)
            return;

        TickToolPickup();

        if (needsController != null)
        {
            float tirednessMultiplier = GetTirednessRate() / Mathf.Max(0.01f, 0.3f);
            needsController.TickCivilianNeeds(Time.deltaTime, tirednessMultiplier, health);
            currentHunger = needsController.CurrentHunger;
            currentTiredness = needsController.CurrentTiredness;
            ProcessNeedDrivenState();
        }

        if (health != null && !health.IsAlive)
            return;

        return;
    }

    bool NeedsFood() => needsController != null && needsController.NeedsFood();

    void ProcessNeedDrivenState()
    {
        if (needsController == null)
            return;

        NeedDrivenActionType action = needsController.GetRecommendedAction();
        if (action == NeedDrivenActionType.None)
            return;

        if (state == State.Eating || state == State.Sleeping || state == State.SeekingFoodStorage || state == State.SeekingHouse)
            return;

        if (!hasStoredRoleState)
        {
            stateBeforeNeed = state;
            hasStoredRoleState = true;
        }

        switch (action)
        {
            case NeedDrivenActionType.SeekRest:
                SetState(State.SeekingHouse);
                break;
            case NeedDrivenActionType.SeekFood:
            case NeedDrivenActionType.SeekWater:
                SetState(State.SeekingFoodStorage);
                break;
        }
    }

    void ResumeAfterNeed()
    {
        State fallback = ResolveRoleFallbackState();
        if (targetCraftingBuilding != null && (jobType == CivilianJobType.Crafter || CivilianJobRegistry.GetProfile(jobType).supportsCraftingAssignment))
        {
            SetState(State.GoingToWorkPoint);
            hasStoredRoleState = false;
            return;
        }

        if (hasStoredRoleState)
        {
            SetState(stateBeforeNeed);
            hasStoredRoleState = false;

            if (state == State.SeekingFoodStorage || state == State.Eating || state == State.SeekingHouse || state == State.Sleeping)
                SetState(fallback);
        }
        else
        {
            SetState(fallback);
        }
    }

    State ResolveRoleFallbackState()
    {
        switch (CivilianJobRegistry.GetProfile(jobType).domain)
        {
            case CivilianWorkDomain.Gathering: return State.SearchingNode;
            case CivilianWorkDomain.Building: return State.SearchingBuildSite;
            case CivilianWorkDomain.Hauling: return State.SearchingSupplySite;
            case CivilianWorkDomain.Crafting: return State.FetchingCraftInput;
            default: return State.Idle;
        }
    }

    void TickIdleAtAssignedHousePoint()
    {
        if (AssignedHouse == null)
            return;

        Transform houseTarget = movementController.ResolveInteractionTarget(AssignedHouse.transform, BuildingInteractionPointType.House);
        float stop = movementController.ResolveStopDistance(AssignedHouse.transform, BuildingStopDistanceType.House);
        movementController.MoveTo(houseTarget.position, stop);
    }

    bool TryConsumeFoodFromAssignedHouse()
    {
        if (housingController == null)
            return false;

        if (!housingController.TryConsumeNearbyHouseFood(transform.position, Mathf.Max(0.1f, stopDistance * 2f), EnumerateConfiguredFoods(), foodToEatPerMeal, out ResourceDefinition consumedType, out FoodDefinition consumedFood, out int consumed))
            return false;

        targetFoodResource = consumedType;
        targetFoodDefinition = consumedFood;
        pendingFoodAmount = consumed;
        currentEatDurationSeconds = Mathf.Max(0.1f, consumedFood != null ? consumedFood.eatTime : eatDurationSeconds);
        needActionTimer = 0f;
        SetState(State.Eating);
        targetFoodStorage = null;
        return true;
    }

    bool TryFindBestFoodStorage(out ResourceStorageContainer bestStorage, out ResourceDefinition bestType, out FoodDefinition bestFood)
    {
        bestStorage = null;
        bestType = null;
        bestFood = null;

        var storageManager = TeamStorageManager.Instance;
        if (storageManager == null)
            return false;

        var storages = storageManager.GetStoragesForTeam(teamID);
        float bestScore = float.MinValue;

        for (int s = 0; s < storages.Count; s++)
        {
            ResourceStorageContainer storage = storages[s];
            if (storage == null || storage.teamID != teamID)
                continue;

            foreach (FoodDefinition food in EnumerateConfiguredFoods())
            {
                if (!TryGetFoodResource(food, out ResourceDefinition type))
                    continue;

                if (!storage.CanSupply(type))
                    continue;

                int stored = storage.GetStored(type);
                if (stored <= 0)
                    continue;

                int hungerRestore = Mathf.Max(1, food.hungerRestore);
                float qualityBonus = hungerRestore * 10f;
                bool isHouseStorage = storage.GetComponentInParent<House>() != null;
                float housePriorityBonus = isHouseStorage ? 1000f : 0f;
                float distancePenalty = (storage.transform.position - transform.position).sqrMagnitude * 0.001f;
                float score = stored + qualityBonus + housePriorityBonus - distancePenalty;

                if (score <= bestScore)
                    continue;

                bestScore = score;
                bestStorage = storage;
                bestType = type;
                bestFood = food;
            }
        }

        return bestStorage != null;
    }



    IEnumerable<FoodDefinition> EnumerateConfiguredFoods()
    {
        if (foodDatabase != null)
        {
            foreach (FoodDefinition food in foodDatabase.EnumerateFoods())
                yield return food;
            yield break;
        }

        if (resourcesDatabase == null || resourcesDatabase.resources == null)
            yield break;

        for (int i = 0; i < resourcesDatabase.resources.Count; i++)
        {
            ResourceDefinition resource = resourcesDatabase.resources[i];
            if (resource == null || !resourcesDatabase.IsCategory(resource, ResourceCategory.Food))
                continue;

            yield return new FoodDefinition
            {
                resource = resource,
                hungerRestore = 1,
                eatTime = eatDurationSeconds
            };
        }
    }

    bool TryGetFoodResource(FoodDefinition food, out ResourceDefinition type)
    {
        type = food != null ? food.resource : null;
        return type != null;
    }

    public void SetJobType(CivilianJobType newJobType)
    {
        jobType = newJobType;

        GrantStartingToolForCurrentJob();


        targetSite = null;
        targetStorage = null;

        CurrentAssignedSite = null;
        CurrentDeliverySite = null;
        ClearCraftingAssignment();

        if (carriedAmount > 0)
        {
            SetState(State.GoingToDepositStorage);
            return;
        }

        SetState(ResolveRoleFallbackState());
    }

    public void GrantStartingToolForCurrentJob()
    {
        if (!CivilianToolRegistry.TryGetPreferredStartingTool(jobType, out CivilianToolType toolType))
            return;

        equippedTools.Add(toolType);
    }


    public void IssueMoveCommand(Vector3 worldPos)
    {
        // Stop any gathering activity cleanly
        gatheringController.StopGathering();

        // Clear other job-related targets
        targetSite = null;
        targetStorage = null;
        CurrentAssignedSite = null;
        CurrentDeliverySite = null;
        ClearCraftingAssignment();

        HasJob = false;

        // Reset state
        SetState(State.Idle);

        // Move civilian
        if (movementController != null)
        {
            float stop = movementController.ResolveStopDistance(null, BuildingStopDistanceType.Default);
            movementController.MoveTo(worldPos, stop);
        }
    }
    // ---------- Hauler / Supply to Construction ----------

    bool TryFallbackToHouseInteractionPoint()
    {
        if (idleNoTaskTimer < 10f)
            return false;

        if (AssignedHouse == null)
            return false;

        TickIdleAtAssignedHousePoint();
        return true;
    }


    bool TryHandleCraftingHaulerPriority()
    {
        if (targetCraftingBuilding != null)
        {
            if (carriedAmount > 0)
            {
                if (targetCraftingBuilding.NeedsInput(carriedResource))
                    SetState(State.DeliveringCraftInput);
                else
                    SetState(State.DeliveringCraftOutput);
                return true;
            }

            if (targetCraftingBuilding.NeedsAnyInput())
            {
                SetState(State.FetchingCraftInput);
                return true;
            }

            if (targetCraftingBuilding.HasAnyOutputQueued())
            {
                SetState(State.CollectingCraftOutput);
                return true;
            }

            SetState(State.FetchingCraftInput);
            return true;
        }

        if (carriedAmount > 0)
        {
            var inputBuilding = CraftingJobManager.Instance?.FindNearestBuildingNeedingInput(teamID, carriedResource, transform.position);
            if (inputBuilding != null)
            {
                targetCraftingBuilding = inputBuilding;
                SetState(State.DeliveringCraftInput);
                return true;
            }

            SetState(State.GoingToDepositStorage);
            return true;
        }

        CraftingBuilding prioritized = CraftingJobManager.Instance?.FindNearestBuildingWithInputPriority(teamID, transform.position);
        if (prioritized != null)
        {
            targetCraftingBuilding = prioritized;
            SetState(prioritized.NeedsAnyInput() ? State.FetchingCraftInput : State.CollectingCraftOutput);
            return true;
        }

        return false;
    }

    // ---------- Helpers ----------

    bool IsCraftingLogisticsHauler()
    {
        return targetCraftingBuilding != null
            && targetCraftingBuilding.requireHaulerLogistics
            && jobType == CivilianJobType.Hauler;
    }

    int GetCarryCapacity()
    {
        int value = carryCapacity > 0 ? carryCapacity : 0;
        if (value > 0)
            return Mathf.RoundToInt(value * (HasTool(CivilianToolType.Backpack) && jobType == CivilianJobType.Hauler ? 1.5f : 1f));

        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null && stats.CarryCapacity > 0) return Mathf.RoundToInt(stats.CarryCapacity * (HasTool(CivilianToolType.Backpack) && jobType == CivilianJobType.Hauler ? 1.5f : 1f));
        return 1;
    }

    int GetHarvestPerTick()
    {
        int value = harvestPerTick > 0 ? harvestPerTick : 0;
        if (value > 0)
            return Mathf.RoundToInt(value * (HasTool(CivilianToolType.Pickaxe) && jobType == CivilianJobType.Gatherer ? 1.5f : 1f));

        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null && stats.HarvestPerTick > 0) return Mathf.RoundToInt(stats.HarvestPerTick * (HasTool(CivilianToolType.Pickaxe) && jobType == CivilianJobType.Gatherer ? 1.5f : 1f));
        return 1;
    }

    float GetBuildMultiplier()
    {
        CharacterStats stats = GetComponent<CharacterStats>();
        float baseValue = stats != null ? stats.BuildWorkMultiplier : 1f;
        if (HasTool(CivilianToolType.Trowel) && jobType == CivilianJobType.Builder)
            baseValue *= 1.5f;
        return baseValue;
    }
    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        return raw.Replace("(Clone)", "").Trim();
    }

    ConstructionSite FindNearestConstructionSite(int team, Vector3 pos)
    {
        var registry = ConstructionRegistry.Instance;
        if (registry == null)
            return null;

        var sites = registry.GetSitesForTeam(team);
        ConstructionSite best = null;
        float bestD = float.MaxValue;

        for (int i = 0; i < sites.Count; i++)
        {
            var s = sites[i];
            if (s == null) continue;
            if (s.teamID != team) continue;
            if (s.IsComplete) continue;

            float d = (s.transform.position - pos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = s;
            }
        }

        return best;
    }

    private void InitializeStateMachine()
    {
        states.Clear();
        states[State.Idle] = new IdleState(this);
        states[State.SeekingFoodStorage] = new SeekingFoodStorageState(this);
        states[State.Eating] = new EatingState(this);
        states[State.SeekingHouse] = new SeekingHouseState(this);
        states[State.Sleeping] = new SleepingState(this);
        states[State.SearchingNode] = new SearchingNodeState(this);
        states[State.GoingToNode] = new GoingToNodeState(this);
        states[State.Gathering] = new GatheringState(this);
        states[State.FindDepositStorage] = new FindDepositStorageState(this);
        states[State.GoingToDepositStorage] = new GoingToDepositStorageState(this);
        states[State.Depositing] = new DepositingState(this);
        states[State.SearchingBuildSite] = new SearchingBuildSiteState(this);
        states[State.GoingToBuildSite] = new GoingToBuildSiteState(this);
        states[State.Building] = new BuildingState(this);
        states[State.SearchingSupplySite] = new SearchingSupplySiteState(this);
        states[State.GoingToPickupStorage] = new GoingToPickupStorageState(this);
        states[State.PickingUp] = new PickingUpState(this);
        states[State.GoingToDeliverSite] = new GoingToDeliverSiteState(this);
        states[State.Delivering] = new DeliveringState(this);
        states[State.FetchingCraftInput] = new FetchingCraftInputState(this);
        states[State.DeliveringCraftInput] = new DeliveringCraftInputState(this);
        states[State.GoingToWorkPoint] = new GoingToWorkPointState(this);
        states[State.CraftingAtWorkPoint] = new CraftingAtWorkPointState(this);
        states[State.CollectingCraftOutput] = new CollectingCraftOutputState(this);
        states[State.DeliveringCraftOutput] = new DeliveringCraftOutputState(this);

        SetState(state);
    }

    bool TryChooseNeededResource(ConstructionSite site, out ResourceDefinition neededType)
    {
        neededType = default;
        if (site == null) return false;

        ResourceCost[] costs = site.GetRequiredCosts();
        if (costs == null || costs.Length == 0) return false;

        int bestMissing = 0;
        ResourceDefinition bestType = costs[0].resource;

        for (int i = 0; i < costs.Length; i++)
        {
            int missing = site.GetMissing(costs[i].resource);
            if (missing > bestMissing)
            {
                bestMissing = missing;
                bestType = costs[i].resource;
            }
        }

        if (bestMissing <= 0) return false;

        neededType = bestType;
        return true;
    }


    public bool CanTakeCraftingAssignment(CivilianJobType requiredType)
    {
        if (targetCraftingBuilding != null && !manualCraftingAssignment)
            return false;

        if (requiredType == CivilianJobType.Generalist || requiredType == CivilianJobType.Idle)
            return jobType == requiredType || CivilianJobRegistry.GetProfile(jobType).supportsCraftingAssignment;

        return jobType == requiredType;
    }

    public void AssignCraftingBuilding(CraftingBuilding building, bool manual = false)
    {
        if (targetCraftingBuilding != null && targetCraftingBuilding != building)
            targetCraftingBuilding.RemoveWorker(this);

        if (building != null && !building.TryAssignWorker(this, manual))
        {
            if (targetCraftingBuilding == building)
                targetCraftingBuilding = null;

            SetState(ResolveRoleFallbackState());
            return;
        }

        if (jobType == CivilianJobType.Idle || jobType == CivilianJobType.Gatherer || jobType == CivilianJobType.Builder)
            SetJobType(CivilianJobType.Crafter);

        targetCraftingBuilding = building;
        manualCraftingAssignment = manual;
        SetState(State.FetchingCraftInput);
    }

    public void ClearCraftingAssignment()
    {
        if (targetCraftingBuilding != null)
        {
            targetCraftingBuilding.ReleaseWorkPoint(this);
            targetCraftingBuilding.RemoveWorker(this);
        }

        targetCraftingBuilding = null;
        targetWorkPoint = null;
        manualCraftingAssignment = false;
    }

                            float GetMovementSpeedMultiplier()
    {
        float multiplier = 1f + (GetHouseSpeedBonusMultiplier() - 1f);
        multiplier *= GetToolMoveSpeedMultiplier();

        if (needsController != null)
        {
            float tiredness01 = maxTiredness <= 0f ? 0f : Mathf.Clamp01(currentTiredness / maxTiredness);
            if (tiredness01 > 0.7f)
            {
                float penalty = Mathf.Lerp(1f, 0.55f, Mathf.InverseLerp(0.7f, 1f, tiredness01));
                multiplier *= penalty;
            }
        }

        if (!useRoadBonus)
            return multiplier;

        Vector3 probe = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider != null && hit.collider.CompareTag("Road"))
                return multiplier * Mathf.Max(1f, roadSpeedMultiplier);
        }

        return multiplier;
    }

    bool MoveToBuildingAndCheckArrival(Transform target, BuildingStopDistanceType stopType, BuildingInteractionPointType pointType)
    {
        if (target == null)
            return false;

        movementController.MoveToBuildingTarget(target, stopType, pointType);
        return movementController.HasArrived();
    }

    bool MoveToPositionAndCheckArrival(Vector3 position, float stopDistance)
    {
        movementController.MoveTo(position, stopDistance);
        return movementController.HasArrived();
    }

    int PickupResourceFromStorage(ResourceStorageContainer storage, ResourceDefinition resource, int maxAmount)
    {
        if (storage == null || resource == null || maxAmount <= 0)
            return 0;

        carriedAmount = storage.Withdraw(resource, maxAmount);
        carryingController?.SetCarried(resource, carriedAmount);
        return carriedAmount;
    }

    void ApplyDeliveryAccepted(int accepted)
    {
        if (accepted <= 0)
            return;

        carriedAmount -= accepted;
        carryingController?.SetCarried(carriedResource, carriedAmount);
    }

    bool ShouldRetrySearch(ref float timer, float interval)
    {
        timer += Time.deltaTime;
        if (timer < interval)
            return false;

        timer = 0f;
        return true;
    }

    bool TickTimer(ref float timer, float duration)
    {
        timer += Time.deltaTime;
        return timer >= duration;
    }

    string BuildTaskLabel()
    {
        return currentStateHandler != null ? currentStateHandler.TaskLabel : state.ToString();
    }

    string BuildStateDetails()
    {
        return currentStateHandler != null ? currentStateHandler.GetStateDetails() : "No active target";
    }


    float GetHouseSpeedBonusMultiplier()
    {
        return housingController != null ? housingController.GetHouseSpeedBonusMultiplier() : 1f;
    }

    float GetTirednessRate()
    {
        float baseRate = needsController != null ? needsController.TirednessRatePerSecond : 0.3f;
        float multiplier = housingController != null ? housingController.GetTirednessReductionMultiplier() : 1f;
        return baseRate * multiplier;
    }

    void TickToolPickup()
    {
        toolPickupTimer += Time.deltaTime;
        if (toolPickupTimer < 1f)
            return;

        toolPickupTimer = 0f;

        if (TeamInventory.Instance == null)
            return;

        var tools = CivilianToolRegistry.AllTools;
        for (int i = 0; i < tools.Count; i++)
        {
            var profile = tools[i];
            if (!CivilianToolRegistry.MatchesJob(profile, jobType) || equippedTools.Contains(profile.toolType))
                continue;

            ToolItem item = FindToolItemAsset(profile.toolType.ToString());
            if (item == null)
                continue;

            if (TeamInventory.Instance.RemoveTool(teamID, item, 1))
                equippedTools.Add(profile.toolType);
        }
    }

    ToolItem FindToolItemAsset(string nameHint)
    {
        ToolItem[] items = Resources.LoadAll<ToolItem>("Tool Catalog");
        for (int i = 0; i < items.Length; i++)
        {
            ToolItem item = items[i];
            if (item == null) continue;
            if (item.name == nameHint || item.displayName == nameHint)
                return item;
        }

        return null;
    }

    float GetToolMoveSpeedMultiplier() => HasTool(CivilianToolType.RunningShoes) ? 1.5f : 1f;

    bool HasTool(CivilianToolType toolType) => equippedTools.Contains(toolType);

    void ConsumeHouseFoodWhileResting()
    {
        if (housingController == null || needsController == null)
            return;

        if (housingController.TryConsumeHouseFoodWhileResting(EnumerateConfiguredFoods(), foodToEatPerMeal, needsController, NeedsFood))
            currentHunger = needsController.CurrentHunger;
    }

    public float GetCraftingSpeedMultiplierFromTools()
    {
        return HasTool(CivilianToolType.Spanner) && jobType == CivilianJobType.Engineer ? 1.5f : 1f;
    }
    public void TakeDamage(float damage)
    {
        if (health == null)
            return;

        health.TakeDamage(damage);

        if (!health.IsAlive)
        {
            housingController?.ClearAssignedHouse();
            Destroy(gameObject);
        }
    }

    // If your ITargetable interface expects something like this, you can keep it:
    public Transform GetTransform()
    {
        return transform;
    }
    public bool CanPerform(WorkerCapability capability)
    {
        bool canByCurrentJob;
        switch (capability)
        {
            case WorkerCapability.Gather:
                canByCurrentJob = jobType == CivilianJobType.Gatherer || jobType == CivilianJobType.Generalist;
                break;
            case WorkerCapability.Build:
                canByCurrentJob = jobType == CivilianJobType.Builder || jobType == CivilianJobType.Generalist;
                break;
            case WorkerCapability.Haul:
                canByCurrentJob = jobType == CivilianJobType.Hauler || jobType == CivilianJobType.Builder || jobType == CivilianJobType.Generalist;
                break;
            case WorkerCapability.Craft:
                canByCurrentJob = jobType == CivilianJobType.Crafter || CivilianJobRegistry.GetProfile(jobType).supportsCraftingAssignment;
                break;
            default:
                canByCurrentJob = false;
                break;
        }

        if (canByCurrentJob)
            return true;

        return jobDefinition != null && jobDefinition.HasCapability(capability);
    }

    public bool TryAssignTask(WorkerTaskRequest task)
    {
        bool canPerform = task.taskType == WorkerTaskType.Craft && task.requiredCraftJobType == CivilianJobType.Hauler
            ? CanPerform(WorkerCapability.Haul)
            : CanPerform(task.requiredCapability);

        if (!canPerform)
            return false;

        idleNoTaskTimer = 0f;

        switch (task.taskType)
        {
            case WorkerTaskType.Gather:
                if (task.resourceNode == null) return false;
                SetJobType(CivilianJobType.Gatherer);
                gatheringController.AssignPreferredNode(task.resourceNode);
                return true;
            case WorkerTaskType.Build:
                SetJobType(CivilianJobType.Builder);
                return true;
            case WorkerTaskType.Haul:
                if (jobType != CivilianJobType.Builder)
                    SetJobType(CivilianJobType.Hauler);
                return true;
            case WorkerTaskType.Craft:
                if (task.craftingBuilding == null) return false;
                if (task.requiredCraftJobType != CivilianJobType.Generalist && task.requiredCraftJobType != CivilianJobType.Idle && jobType != task.requiredCraftJobType)
                    SetJobType(task.requiredCraftJobType);
                AssignCraftingBuilding(task.craftingBuilding);
                return true;
            default:
                return false;
        }
    }

    private abstract class CivilianStateBase
    {
        protected readonly Civilian civilian;

        protected CivilianStateBase(Civilian civilian)
        {
            this.civilian = civilian;
        }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
        public virtual string TaskLabel => civilian.state.ToString();
        public virtual string GetStateDetails()
        {
            string target = civilian.CurrentTargetName;
            return target != "None" ? $"Target: {target}" : "No active target";
        }

        protected string MovingTowardTarget() => $"Moving toward {civilian.CurrentTargetName}";
        protected string InteractingWithTarget() => $"Interacting with {civilian.CurrentTargetName}";
    }

    private sealed class IdleState : CivilianStateBase
    {
        public IdleState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Waiting for job";
        public override string GetStateDetails() => civilian.HasJob ? "Idle between assignments" : "No assignment yet";

        public override void Tick()
        {
            civilian.idleNoTaskTimer += Time.deltaTime;

            if (civilian.jobType != CivilianJobType.Idle)
                return;

            civilian.housingController?.TryAssignHouseIfNeeded();

            if (civilian.AssignedHouse == null)
                return;

            civilian.idleWanderTimer -= Time.deltaTime;
            if (civilian.idleWanderTimer <= 0f || (civilian.idleWanderTarget - civilian.transform.position).sqrMagnitude < 1f)
            {
                civilian.idleWanderTimer = Random.Range(2f, 5f);
                Vector2 jitter = Random.insideUnitCircle * 3f;
                civilian.idleWanderTarget = civilian.AssignedHouse.transform.position + new Vector3(jitter.x, 0f, jitter.y);
            }

            float stop = civilian.movementController.ResolveStopDistance(civilian.AssignedHouse.transform, BuildingStopDistanceType.House);
            civilian.movementController.MoveTo(civilian.idleWanderTarget, stop);
        }
    }

    private sealed class SeekingFoodStorageState : CivilianStateBase
    {
        public SeekingFoodStorageState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Seeking food";

        public override void Tick()
        {
            if (civilian.TryConsumeFoodFromAssignedHouse())
                return;

            if (TeamStorageManager.Instance == null)
                return;

            if (civilian.targetFoodStorage == null || civilian.targetFoodStorage.teamID != civilian.teamID || civilian.targetFoodStorage.GetStored(civilian.targetFoodResource) <= 0)
            {
                if (!civilian.TryFindBestFoodStorage(out civilian.targetFoodStorage, out civilian.targetFoodResource, out civilian.targetFoodDefinition))
                    return;
            }

            if (!civilian.MoveToBuildingAndCheckArrival(civilian.targetFoodStorage.transform, BuildingStopDistanceType.Storage, BuildingInteractionPointType.Storage))
                return;

            civilian.pendingFoodAmount = Mathf.Max(1, civilian.foodToEatPerMeal);
            int eatenUnits = civilian.targetFoodStorage.Withdraw(civilian.targetFoodResource, civilian.pendingFoodAmount);
            if (eatenUnits <= 0)
            {
                civilian.targetFoodStorage = null;
                return;
            }

            civilian.pendingFoodAmount = eatenUnits;
            civilian.currentEatDurationSeconds = civilian.targetFoodDefinition != null ? Mathf.Max(0.1f, civilian.targetFoodDefinition.eatTime) : civilian.eatDurationSeconds;
            civilian.needActionTimer = 0f;
            civilian.SetState(State.Eating);
        }
    }

    private sealed class EatingState : CivilianStateBase
    {
        public EatingState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Eating";

        public override void Tick()
        {
            if (!civilian.TickTimer(ref civilian.needActionTimer, civilian.currentEatDurationSeconds))
                return;

            civilian.needsController.BeginEating(civilian.targetFoodDefinition, civilian.pendingFoodAmount);
            civilian.pendingFoodAmount = 0;
            civilian.targetFoodStorage = null;
            civilian.targetFoodDefinition = null;

            if (civilian.needsController.ShouldSleepNow())
                civilian.SetState(State.SeekingHouse);
            else if (!civilian.needsController.ShouldEatNow())
                civilian.ResumeAfterNeed();
            else
                civilian.SetState(State.SeekingFoodStorage);
        }
    }

    private sealed class SeekingHouseState : CivilianStateBase
    {
        public SeekingHouseState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Seeking house";

        public override void Tick()
        {
            if (!civilian.housingController.TryEnsureTargetHouse())
                return;

            House targetHouse = civilian.housingController.TargetHouse;
            if (targetHouse == null)
                return;

            if (!civilian.MoveToBuildingAndCheckArrival(targetHouse.transform, BuildingStopDistanceType.House, BuildingInteractionPointType.House))
                return;

            if (!civilian.housingController.TryClaimTargetHouse())
            {
                civilian.housingController.SetTargetHouse(null);
                return;
            }

            civilian.needActionTimer = 0f;
            civilian.SetState(State.Sleeping);
        }
    }

    private sealed class SleepingState : CivilianStateBase
    {
        public SleepingState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Sleeping";

        public override void Tick()
        {
            if (civilian.AssignedHouse == null)
            {
                civilian.SetState(State.SeekingHouse);
                return;
            }

            civilian.needsController.TickSleep(Time.deltaTime);
            civilian.currentTiredness = civilian.needsController.CurrentTiredness;
            civilian.ConsumeHouseFoodWhileResting();

            if (!civilian.TickTimer(ref civilian.needActionTimer, civilian.sleepDurationSeconds))
                return;

            civilian.needsController.CompleteSleep();
            civilian.currentTiredness = civilian.needsController.CurrentTiredness;
            civilian.housingController.SetTargetHouse(civilian.AssignedHouse);
            civilian.ResumeAfterNeed();
        }
    }

    private sealed class SearchingNodeState : CivilianStateBase
    {
        public SearchingNodeState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Searching for resource node";
        public override string GetStateDetails() => "Scanning for nearest valid task";

        public override void Tick()
        {
            civilian.idleNoTaskTimer += Time.deltaTime;

            if (civilian.gatheringController.forcedNode != null)
            {
                var node = civilian.gatheringController.forcedNode;
                if (!node.IsDepleted && civilian.gatheringController.AssignNode(node))
                {
                    civilian.idleNoTaskTimer = 0f;
                    civilian.SetState(State.GoingToNode);
                    return;
                }
                civilian.gatheringController.forcedNode = null;
            }

            if (civilian.gatheringController.TryFindNode(out var bestNode) && civilian.gatheringController.AssignNode(bestNode))
            {
                civilian.idleNoTaskTimer = 0f;
                civilian.SetState(State.GoingToNode);
                return;
            }

            civilian.TryFallbackToHouseInteractionPoint();
        }
    }

    private sealed class GoingToNodeState : CivilianStateBase
    {
        public GoingToNodeState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to target node";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            if (civilian.carryingController.IsFull)
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            if (civilian.gatheringController.CurrentNode == null)
            {
                civilian.SetState(State.SearchingNode);
                return;
            }

            if (civilian.movementController.HasArrived())
                civilian.SetState(State.Gathering);
        }
    }

    private sealed class GatheringState : CivilianStateBase
    {
        public GatheringState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Collecting resource";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (civilian.gatheringController.CurrentNode == null)
            {
                civilian.SetState(State.SearchingNode);
                return;
            }

            if (civilian.carryingController.IsFull)
            {
                civilian.SetState(State.FindDepositStorage);
                return;
            }

            bool gathered = civilian.gatheringController.TickGathering();
            if (!gathered && civilian.gatheringController.CurrentNode == null)
                civilian.SetState(State.SearchingNode);
        }
    }

    private sealed class FindDepositStorageState : CivilianStateBase
    {
        public FindDepositStorageState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Finding storage";

        public override void Tick()
        {
            civilian.targetStorage = null;
            civilian.targetDropoff = null;

            if (civilian.gatheringController.TryFindDepositStorage(out civilian.targetStorage) || civilian.gatheringController.TryFindDropoff(out civilian.targetDropoff))
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            civilian.SetState(State.Idle);
        }
    }

    private sealed class GoingToDepositStorageState : CivilianStateBase
    {
        public GoingToDepositStorageState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to storage";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            Transform depositTarget = civilian.targetStorage != null ? civilian.targetStorage.transform : civilian.targetDropoff != null ? civilian.targetDropoff.transform : null;
            if (depositTarget == null)
            {
                civilian.SetState(State.FindDepositStorage);
                return;
            }

            if (civilian.MoveToBuildingAndCheckArrival(depositTarget, BuildingStopDistanceType.Storage, BuildingInteractionPointType.Storage))
                civilian.SetState(State.Depositing);
        }
    }

    private sealed class DepositingState : CivilianStateBase
    {
        public DepositingState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Depositing carried resources";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (civilian.targetStorage == null && civilian.targetDropoff == null)
            {
                civilian.SetState(State.FindDepositStorage);
                return;
            }

            bool emptiedInventory = civilian.targetStorage != null
                ? civilian.gatheringController.TryDeposit(civilian.targetStorage)
                : civilian.gatheringController.TryDeposit(civilian.targetDropoff);

            if (emptiedInventory || !civilian.carryingController.IsCarrying)
            {
                civilian.SetState(State.SearchingNode);
                return;
            }

            civilian.SetState(State.FindDepositStorage);
        }
    }

    private sealed class SearchingBuildSiteState : CivilianStateBase
    {
        public SearchingBuildSiteState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Searching for construction work";
        public override string GetStateDetails() => "Scanning for nearest valid task";

        public override void Tick()
        {
            civilian.idleNoTaskTimer += Time.deltaTime;

            if (!civilian.ShouldRetrySearch(ref civilian.searchTimer, civilian.searchRetrySeconds))
                return;

            civilian.targetSite = civilian.FindNearestConstructionSite(civilian.teamID, civilian.transform.position);
            civilian.CurrentAssignedSite = civilian.targetSite;

            if (civilian.targetSite == null)
            {
                civilian.TryFallbackToHouseInteractionPoint();
                return;
            }

            if (!civilian.targetSite.MaterialsComplete && civilian.buildersCanHaulMaterials)
            {
                civilian.idleNoTaskTimer = 0f;
                civilian.SetState(State.SearchingSupplySite);
                return;
            }

            civilian.idleNoTaskTimer = 0f;
            civilian.SetState(State.GoingToBuildSite);
            civilian.movementController.MoveToBuildingTarget(civilian.targetSite.transform, BuildingStopDistanceType.Construction, BuildingInteractionPointType.Default);
        }
    }

    private sealed class GoingToBuildSiteState : CivilianStateBase
    {
        public GoingToBuildSiteState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to build site";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            if (civilian.targetSite == null || civilian.targetSite.IsComplete)
            {
                civilian.targetSite = null;
                civilian.CurrentAssignedSite = null;
                civilian.SetState(State.SearchingBuildSite);
                return;
            }

            if (!civilian.targetSite.MaterialsComplete)
            {
                civilian.SetState(civilian.buildersCanHaulMaterials ? State.SearchingSupplySite : State.SearchingBuildSite);
                return;
            }

            civilian.movementController.MoveToBuildingTarget(civilian.targetSite.transform, BuildingStopDistanceType.Construction, BuildingInteractionPointType.Default);
            if (civilian.movementController.HasArrived())
                civilian.SetState(State.Building);
        }
    }

    private sealed class BuildingState : CivilianStateBase
    {
        public BuildingState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Building structure";

        public override void Tick()
        {
            if (civilian.targetSite == null || civilian.targetSite.IsComplete)
            {
                civilian.targetSite = null;
                civilian.CurrentAssignedSite = null;
                civilian.SetState(State.SearchingBuildSite);
                return;
            }

            if (!civilian.targetSite.MaterialsComplete)
            {
                civilian.SetState(civilian.buildersCanHaulMaterials ? State.SearchingSupplySite : State.SearchingBuildSite);
                return;
            }

            civilian.targetSite.AddWork(Time.deltaTime * Mathf.Max(0.25f, civilian.GetBuildMultiplier()));
        }
    }

    private sealed class SearchingSupplySiteState : CivilianStateBase
    {
        public SearchingSupplySiteState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Waiting for haul assignment";
        public override string GetStateDetails() => "Scanning for nearest valid task";

        public override void Tick()
        {
            civilian.idleNoTaskTimer += Time.deltaTime;

            if (civilian.jobType == CivilianJobType.Hauler && civilian.TryHandleCraftingHaulerPriority())
                return;

            if (civilian.carriedAmount > 0 && civilian.targetSite != null)
            {
                civilian.SetState(State.GoingToDeliverSite);
                return;
            }

            if (!civilian.ShouldRetrySearch(ref civilian.searchTimer, civilian.searchRetrySeconds))
                return;

            civilian.targetSite = civilian.FindNearestConstructionSite(civilian.teamID, civilian.transform.position);
            civilian.CurrentDeliverySite = civilian.targetSite;

            if (civilian.targetSite == null)
            {
                civilian.TryFallbackToHouseInteractionPoint();
                return;
            }

            if (civilian.targetSite.IsComplete || civilian.targetSite.MaterialsComplete)
            {
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            if (!civilian.TryChooseNeededResource(civilian.targetSite, out civilian.carriedResource))
            {
                civilian.targetSite = null;
                civilian.CurrentDeliverySite = null;
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            civilian.SetState(State.GoingToPickupStorage);
            civilian.idleNoTaskTimer = 0f;
        }
    }

    private sealed class GoingToPickupStorageState : CivilianStateBase
    {
        public GoingToPickupStorageState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to pickup";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            if (TeamStorageManager.Instance == null || civilian.targetSite == null)
            {
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(civilian.targetSite.SiteKey, civilian.carriedResource);
            if (reservedForSite <= 0)
            {
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            if (civilian.targetStorage == null)
                civilian.targetStorage = TeamStorageManager.Instance.FindNearestStorageWithStored(civilian.teamID, civilian.carriedResource, civilian.transform.position);

            if (civilian.targetStorage == null)
                return;

            if (civilian.MoveToBuildingAndCheckArrival(civilian.targetStorage.transform, BuildingStopDistanceType.Storage, BuildingInteractionPointType.Storage))
                civilian.SetState(State.PickingUp);
        }
    }

    private sealed class PickingUpState : CivilianStateBase
    {
        public PickingUpState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Collecting resource from storage";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (TeamStorageManager.Instance == null || civilian.targetStorage == null || civilian.targetSite == null)
            {
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            int missing = civilian.targetSite.GetMissing(civilian.carriedResource);
            int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(civilian.targetSite.SiteKey, civilian.carriedResource);
            int want = Mathf.Min(civilian.GetCarryCapacity(), missing);
            want = Mathf.Min(want, reservedForSite);

            if (want <= 0)
            {
                civilian.targetStorage = null;
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            int took = civilian.PickupResourceFromStorage(civilian.targetStorage, civilian.carriedResource, want);
            if (took <= 0)
            {
                civilian.targetStorage = null;
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            TeamStorageManager.Instance.ConsumeReserved(civilian.teamID, civilian.targetSite.SiteKey, civilian.carriedResource, took);
            civilian.targetStorage = null;
            civilian.SetState(State.GoingToDeliverSite);
            civilian.movementController.MoveToBuildingTarget(civilian.targetSite.transform, BuildingStopDistanceType.Construction, BuildingInteractionPointType.Default);
        }
    }

    private sealed class GoingToDeliverSiteState : CivilianStateBase
    {
        public GoingToDeliverSiteState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to delivery target";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            if (civilian.targetSite == null)
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            if (civilian.MoveToBuildingAndCheckArrival(civilian.targetSite.transform, BuildingStopDistanceType.Construction, BuildingInteractionPointType.Default))
                civilian.SetState(State.Delivering);
        }
    }

    private sealed class DeliveringState : CivilianStateBase
    {
        public DeliveringState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Delivering resources";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (civilian.targetSite == null || civilian.carriedAmount <= 0)
            {
                civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
                return;
            }

            int accepted = civilian.targetSite.ReceiveDelivery(civilian.carriedResource, civilian.carriedAmount);
            civilian.ApplyDeliveryAccepted(accepted);

            if (civilian.carriedAmount > 0)
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            civilian.SetState((civilian.jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite);
        }
    }

    private sealed class FetchingCraftInputState : CivilianStateBase
    {
        public FetchingCraftInputState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Fetching crafting inputs";
        public override string GetStateDetails() => "Scanning for nearest valid task";

        public override void Tick()
        {
            if (civilian.targetCraftingBuilding == null)
            {
                civilian.SetState(State.Idle);
                return;
            }

            if (civilian.targetCraftingBuilding.requireHaulerLogistics && civilian.jobType != CivilianJobType.Hauler)
            {
                civilian.SetState(State.GoingToWorkPoint);
                return;
            }

            if (civilian.carriedAmount > 0)
            {
                civilian.SetState(civilian.targetCraftingBuilding.NeedsInput(civilian.carriedResource)
                    ? State.DeliveringCraftInput
                    : State.GoingToDepositStorage);
                return;
            }

            if (civilian.targetCraftingBuilding.TryGetInputRequest(civilian.transform.position, out civilian.carriedResource, out int amount, out civilian.targetStorage))
            {
                if (civilian.targetStorage == null)
                {
                    ProductionNotificationManager.Instance?.NotifyIfReady($"missing-storage-{civilian.targetCraftingBuilding.GetInstanceID()}", $"{civilian.targetCraftingBuilding.name}: no storage with {civilian.carriedResource} found.");
                    return;
                }

                if (civilian.MoveToBuildingAndCheckArrival(civilian.targetStorage.transform, BuildingStopDistanceType.Storage, BuildingInteractionPointType.Storage))
                {
                    int took = civilian.PickupResourceFromStorage(civilian.targetStorage, civilian.carriedResource, Mathf.Min(amount, civilian.GetCarryCapacity()));
                    civilian.SetState(took > 0 ? State.DeliveringCraftInput : State.FetchingCraftInput);
                }
                return;
            }

            if (civilian.targetCraftingBuilding.requireHaulerLogistics)
                civilian.SetState(civilian.targetCraftingBuilding.HasAnyOutputQueued() ? State.CollectingCraftOutput : State.FetchingCraftInput);
            else
                civilian.SetState(civilian.targetCraftingBuilding.HasAnyOutputQueued() ? State.CollectingCraftOutput : State.GoingToWorkPoint);
        }
    }

    private sealed class DeliveringCraftInputState : CivilianStateBase
    {
        public DeliveringCraftInputState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Delivering crafting inputs";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (civilian.targetCraftingBuilding == null)
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            Transform inputTransform = civilian.targetCraftingBuilding.inputSlot != null
                ? civilian.targetCraftingBuilding.inputSlot
                : civilian.movementController.ResolveInteractionTarget(civilian.targetCraftingBuilding.transform, BuildingInteractionPointType.CraftInput);
            float stop = civilian.movementController.ResolveStopDistance(civilian.targetCraftingBuilding.transform, BuildingStopDistanceType.CraftInput);
            if (!civilian.MoveToPositionAndCheckArrival(inputTransform.position, stop))
                return;

            if (civilian.carriedAmount <= 0)
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            civilian.targetCraftingBuilding.TryDeliverInput(civilian.carriedResource, civilian.carriedAmount, out int accepted);
            civilian.ApplyDeliveryAccepted(accepted);

            if (civilian.carriedAmount > 0)
            {
                civilian.SetState(State.GoingToDepositStorage);
                return;
            }

            if (civilian.IsCraftingLogisticsHauler())
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            civilian.SetState(civilian.jobType == CivilianJobType.Crafter ? State.GoingToWorkPoint : civilian.ResolveRoleFallbackState());
        }
    }

    private sealed class GoingToWorkPointState : CivilianStateBase
    {
        public GoingToWorkPointState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Moving to work point";
        public override string GetStateDetails() => MovingTowardTarget();

        public override void Tick()
        {
            if (civilian.targetCraftingBuilding == null)
            {
                civilian.SetState(State.Idle);
                return;
            }

            if (civilian.IsCraftingLogisticsHauler())
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            if (!civilian.targetCraftingBuilding.TryReserveWorkPoint(civilian, out civilian.targetWorkPoint) || civilian.targetWorkPoint == null)
                return;

            Transform ctx = civilian.targetCraftingBuilding.transform;
            float stop = civilian.movementController.ResolveStopDistance(ctx, BuildingStopDistanceType.CraftWork);
            if (civilian.MoveToPositionAndCheckArrival(civilian.targetWorkPoint.position, stop))
            {
                civilian.stalledAtWorkPointTimer = 0f;
                civilian.SetState(State.CraftingAtWorkPoint);
                return;
            }

            civilian.stalledAtWorkPointTimer += Time.deltaTime;
            if (civilian.stalledAtWorkPointTimer < 10f)
                return;

            Debug.LogWarning($"[{nameof(Civilian)}] Workpoint stalled for {civilian.name}. Clearing crafting assignment.");
            civilian.stalledAtWorkPointTimer = 0f;
            civilian.targetCraftingBuilding.RemoveWorker(civilian);
            civilian.ClearCraftingAssignment();
            civilian.SetState(civilian.ResolveRoleFallbackState());
        }
    }

    private sealed class CraftingAtWorkPointState : CivilianStateBase
    {
        public CraftingAtWorkPointState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Producing goods";
        public override string GetStateDetails() => "Producing goods at assigned workstation";

        public override void Tick()
        {
            if (civilian.targetCraftingBuilding == null)
            {
                civilian.SetState(State.Idle);
                return;
            }

            if (civilian.IsCraftingLogisticsHauler())
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            if (!civilian.targetCraftingBuilding.requireHaulerLogistics)
            {
                if (civilian.targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForInputs)
                    civilian.SetState(State.FetchingCraftInput);
                else if (civilian.targetCraftingBuilding.State == CraftingBuilding.ProductionState.OutputReady || civilian.targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForPickup)
                    civilian.SetState(State.CollectingCraftOutput);
            }
        }
    }

    private sealed class CollectingCraftOutputState : CivilianStateBase
    {
        public CollectingCraftOutputState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Collecting outputs";
        public override string GetStateDetails() => "Scanning for nearest valid task";

        public override void Tick()
        {
            if (civilian.targetCraftingBuilding == null)
            {
                civilian.SetState(State.Idle);
                return;
            }

            if (civilian.targetCraftingBuilding.requireHaulerLogistics && civilian.jobType != CivilianJobType.Hauler)
            {
                civilian.SetState(State.GoingToWorkPoint);
                return;
            }

            if (civilian.targetCraftingBuilding.NeedsAnyInput())
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            if (civilian.carriedAmount > 0)
            {
                civilian.SetState(civilian.targetCraftingBuilding.NeedsInput(civilian.carriedResource)
                    ? State.DeliveringCraftInput
                    : State.DeliveringCraftOutput);
                return;
            }

            if (!civilian.targetCraftingBuilding.TryGetOutputRequest(civilian.transform.position, out civilian.carriedResource, out int amount, out civilian.targetStorage))
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            Transform outputTransform = civilian.targetCraftingBuilding.outputSlot != null
                ? civilian.targetCraftingBuilding.outputSlot
                : civilian.movementController.ResolveInteractionTarget(civilian.targetCraftingBuilding.transform, BuildingInteractionPointType.CraftOutput);
            float stop = civilian.movementController.ResolveStopDistance(civilian.targetCraftingBuilding.transform, BuildingStopDistanceType.CraftOutput);
            if (!civilian.MoveToPositionAndCheckArrival(outputTransform.position, stop))
                return;

            civilian.carriedAmount = civilian.targetCraftingBuilding.CollectOutput(civilian.carriedResource, Mathf.Min(amount, civilian.GetCarryCapacity()));
            civilian.carryingController?.SetCarried(civilian.carriedResource, civilian.carriedAmount);
            civilian.SetState(civilian.carriedAmount > 0 ? State.DeliveringCraftOutput : State.FetchingCraftInput);
        }
    }

    private sealed class DeliveringCraftOutputState : CivilianStateBase
    {
        public DeliveringCraftOutputState(Civilian civilian) : base(civilian) { }
        public override string TaskLabel => "Delivering crafted goods";
        public override string GetStateDetails() => InteractingWithTarget();

        public override void Tick()
        {
            if (civilian.carriedAmount <= 0)
            {
                civilian.SetState(State.FetchingCraftInput);
                return;
            }

            if (civilian.targetCraftingBuilding != null && civilian.targetCraftingBuilding.NeedsInput(civilian.carriedResource))
            {
                civilian.SetState(State.DeliveringCraftInput);
                return;
            }

            if (civilian.targetStorage == null || civilian.targetStorage.teamID != civilian.teamID || civilian.targetStorage.GetFree(civilian.carriedResource) <= 0)
                civilian.targetStorage = TeamStorageManager.Instance?.FindNearestStorageCached(civilian.teamID, civilian.carriedResource, civilian.transform.position, true);

            if (civilian.targetStorage == null)
            {
                ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{civilian.teamID}-{civilian.carriedResource}", $"No free storage for {civilian.carriedResource}. Production stalled.");
                return;
            }

            if (!civilian.MoveToBuildingAndCheckArrival(civilian.targetStorage.transform, BuildingStopDistanceType.Storage, BuildingInteractionPointType.Storage))
                return;

            int accepted = civilian.targetStorage.Deposit(civilian.carriedResource, civilian.carriedAmount);
            civilian.ApplyDeliveryAccepted(accepted);

            if (civilian.carriedAmount > 0)
                ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{civilian.targetStorage.GetInstanceID()}-{civilian.carriedResource}", $"Storage full for {civilian.carriedResource}. Output queue blocked.");

            civilian.SetState(civilian.carriedAmount > 0
                ? State.DeliveringCraftOutput
                : (civilian.IsCraftingLogisticsHauler() ? State.FetchingCraftInput : State.GoingToWorkPoint));
        }
    }

}
