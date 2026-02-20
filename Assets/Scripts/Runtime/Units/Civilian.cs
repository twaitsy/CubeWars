using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(MovementController))]

public class Civilian : MonoBehaviour, ITargetable
{
    [Header("Identity")]
    public string unitDefinitionId = "civilian"; 
    [Header("Team")]
    public int teamID;
    public bool IsAlive => health != null && health.IsAlive;

    // Forward health properties to HealthComponent
    //   public bool IsAlive => health != null && health.IsAlive;
    //    public float CurrentHealth => health != null ? health.CurrentHealth : 0f;
    //   public float MaxHealth => health != null ? health.MaxHealth : 0f;


    [Header("Job (Unified Registry)")]
    public JobDefinition jobDefinition;
    public CivilianJobType jobType = CivilianJobType.Gatherer;
    [Tooltip("Legacy mirror of jobType for older systems/UI.")]
    public CivilianRole role = CivilianRole.Gatherer;

    public CivilianJobType JobType => jobType;

    [Header("Gathering Progress UI")]
    public bool showGatherProgressBar = true;
    public Vector3 gatherProgressBarOffset = new(0f, 2.2f, 0f);
    public float gatherProgressBarWidth = 1.0f;
    public float gatherProgressBarHeight = 0.1f;
    private ResourceStorageContainer targetStorage; 
    // Compatibility fields referenced elsewhere
    public ResourceNode CurrentReservedNode { get; set; }
    public ConstructionSite CurrentAssignedSite { get; set; }
    public ConstructionSite CurrentDeliverySite { get; set; }

    // Expose carried for UI
    public ResourceDefinition CarriedType => carryingController != null ? carryingController.CarriedResource : null;
    public int CarriedAmount => carryingController != null ? carryingController.CarriedAmount : 0;
    private GatheringController gatheringController;

    //    private float currentHealth;
//    private StorageFacility targetStorage;
    private ResourceDropoff targetDropoff;
    // Legacy/compat fields (kept for external references)
    public bool HasJob;
    public ResourceNode CurrentNode;

    private float gatherTimer;
    private float searchTimer;
    private float retargetTimer;

    private ResourceNode forcedNode;
    private ConstructionSite targetSite;
//    private ResourceStorageContainer targetStorage;

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
    public House AssignedHouse => assignedHouse;
    public FoodDatabase foodDatabase;
    public ResourcesDatabase resourcesDatabase;

    // IHasHealth / ITargetable style properties
    public int TeamID => teamID;
//    public bool IsAlive => currentHealth > 0f;
//    public float CurrentHealth => currentHealth;
//    public float MaxHealth => maxHealth;

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
    public void SetState(State newState)
    {
        state = newState;
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
    private House targetHouse;
    private House assignedHouse;
    private readonly HashSet<CivilianToolType> equippedTools = new();
    private float toolPickupTimer;
    private float idleWanderTimer;
    private Vector3 idleWanderTarget;
    private float stalledAtWorkPointTimer;
    private WorldProgressBar gatherProgressBar;
    private HealthComponent health;
    private MovementController movementController;
    private CarryingController carryingController;
//    private GatheringControl gatheringControl;
    private ConstructionWorkerControl constructionWorkerControl;
    private NeedsController needsController;

    public float speed => movementController != null ? movementController.MoveSpeed : 2.5f;
    public float stopDistance => movementController != null ? movementController.StopDistance : 1.2f;
    private bool useRoadBonus => movementController != null && movementController.UseRoadBonus;
    private float roadSpeedMultiplier => movementController != null ? movementController.RoadSpeedMultiplier : 1.2f;
    public int carryCapacity => carryingController != null ? carryingController.Capacity : 30;
    public float gatherTickSeconds => gatheringController != null ? gatheringController.gatherInterval : 1f;
    public int harvestPerTick => gatheringController != null ? gatheringController.harvestPerTick : 1;
    private float searchRetrySeconds => 1.5f;
    private bool buildersCanHaulMaterials => constructionWorkerControl == null || constructionWorkerControl.CanHaulMaterials;
    private float retargetSeconds => constructionWorkerControl != null ? constructionWorkerControl.RetargetSeconds : 0.6f;
    public int foodToEatPerMeal => needsController != null ? needsController.FoodToEatPerMeal : 10;
    private float eatDurationSeconds => needsController != null ? needsController.EatDurationSeconds : 1.2f;
    public float maxTiredness => needsController != null ? needsController.MaxTiredness : 100f;
    public float maxHunger => needsController != null ? needsController.MaxHunger : 100f;
    public float hungerRatePerSecond => needsController != null ? needsController.HungerRatePerSecond : 0f;
    public float tirednessRatePerSecond => needsController != null ? needsController.TirednessRatePerSecond : 0f;
    private float sleepDurationSeconds => needsController != null ? needsController.SleepDurationSeconds : 5f;
    void Awake()
    {
        health = GetComponent<HealthComponent>();
        movementController = GetComponent<MovementController>();
        carryingController = GetComponent<CarryingController>();
        gatheringController = GetComponent<GatheringController>();
  //      constructionWorkerControl = GetComponent<ConstructionWorkerControl>();
        needsController = GetComponent<NeedsController>();
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

        ClearAssignedHouse();

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
        {
            jobType = jobDefinition.defaultJobType;
            role = jobDefinition.legacyRole;
        }

        SetJobType(jobType);

        started = true;
        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);
        WorkerTaskDispatcher.Instance?.RegisterWorker(this);
        CivilianRegistry.Register(this);

        EnsureGatherProgressBar();
        TryAssignHouseIfNeeded();
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

        // ---------------------------------------------------------
        // Health
        // ---------------------------------------------------------
        health = GetComponent<HealthComponent>();
        movementController = GetComponent<MovementController>();
        carryingController = GetComponent<CarryingController>();
        gatheringController = GetComponent<GatheringController>();
        constructionWorkerControl = GetComponent<ConstructionWorkerControl>();
        needsController = GetComponent<NeedsController>();
        if (health != null)
            health.ApplyDefinition(def);


        // ---------------------------------------------------------
        // Movement
        // ---------------------------------------------------------
        var movement = GetComponent<MovementController>();
        if (movement != null)
            movement.SetMoveSpeed(def.moveSpeed);

        // ---------------------------------------------------------
        // Carrying
        // ---------------------------------------------------------
        var carry = GetComponent<CarryingController>();
        if (carry != null)
            carry.SetCapacity(def.carryCapacity);

        // ---------------------------------------------------------
        // Gathering
        // ---------------------------------------------------------
        var gather = GetComponent<GatheringController>();
        if (gather != null)
            gather.harvestPerTick = Mathf.Max(1, Mathf.RoundToInt(def.gatherSpeed));
        gather.gatherInterval = 1f / Mathf.Max(0.1f, def.gatherSpeed);

        // ---------------------------------------------------------
        // Building
        // ---------------------------------------------------------
        var builder = GetComponent<ConstructionWorkerControl>();
        if (builder != null)
            builder.SetBuildSpeed(def.buildSpeed);

        // ---------------------------------------------------------
        // Combat
        // ---------------------------------------------------------
        var combat = GetComponent<CombatController>();
        if (combat != null)
            combat.SetCombatStats(def.attackDamage, def.attackRange, def.attackCooldown, def.armor);

        // ---------------------------------------------------------
        // Tools
        // ---------------------------------------------------------
        var tools = GetComponent<ToolController>();
        if (tools != null && def.startingTools != null)
            tools.SetStartingTools(def.startingTools);

        // ---------------------------------------------------------
        // Needs
        // ---------------------------------------------------------
        var needs = GetComponent<NeedsController>();
        if (needs != null && def.needs != null)
            needs.SetNeeds(def.needs);

        // ---------------------------------------------------------
        // Jobs
        // ---------------------------------------------------------
        var job = GetComponent<JobController>();
        if (job != null)
            job.SetJobType(def.jobType);

        // ---------------------------------------------------------
        // Training
        // ---------------------------------------------------------
        var training = GetComponent<TrainingController>();
        if (training != null)
            training.SetTraining(def.trainingCost, def.trainingTime, def.trainedAt);

        // ---------------------------------------------------------
        // Upgrades
        // ---------------------------------------------------------
        var upgrade = GetComponent<UpgradeController>();
        if (upgrade != null)
            upgrade.SetUpgradeTarget(def.upgradeTo);
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

        switch (state)
        {
            case State.Idle: TickIdle(); break;

            case State.SeekingFoodStorage: TickSeekFoodStorage(); break;
            case State.Eating: TickEating(); break;
            case State.SeekingHouse: TickSeekHouse(); break;
            case State.Sleeping: TickSleeping(); break;

            case State.SearchingNode: TickSearchNode(); break;
            case State.GoingToNode: TickGoNode(); break;
            case State.Gathering: TickGather(); break;
            case State.FindDepositStorage: TickFindDepositStorage(); break;
            case State.GoingToDepositStorage: TickGoingToDepositStorage(); break;
            case State.Depositing: TickDepositing(); break;





            case State.SearchingBuildSite: TickSearchBuildSite(); break;
            case State.GoingToBuildSite: TickGoBuildSite(); break;
            case State.Building: TickBuild(); break;

            case State.SearchingSupplySite: TickSearchSupplySite(); break;
            case State.GoingToPickupStorage: TickGoPickup(); break;
            case State.PickingUp: TickPickup(); break;
            case State.GoingToDeliverSite: TickGoDeliver(); break;
            case State.Delivering: TickDeliver(); break;

            case State.FetchingCraftInput: TickFetchCraftInput(); break;
            case State.DeliveringCraftInput: TickDeliverCraftInput(); break;
            case State.GoingToWorkPoint: TickGoWorkPoint(); break;
            case State.CraftingAtWorkPoint: TickCraftAtWorkPoint(); break;
            case State.CollectingCraftOutput: TickCollectCraftOutput(); break;
            case State.DeliveringCraftOutput: TickDeliverCraftOutput(); break;
        }

        UpdateGatherProgressBar();
    }

    void EnsureGatherProgressBar()
    {
        if (!showGatherProgressBar)
            return;

        if (gatherProgressBar == null)
            gatherProgressBar = GetComponent<WorldProgressBar>();

        if (gatherProgressBar == null)
            gatherProgressBar = gameObject.AddComponent<WorldProgressBar>();

        gatherProgressBar.offset = gatherProgressBarOffset;
        gatherProgressBar.width = Mathf.Max(0.1f, gatherProgressBarWidth);
        gatherProgressBar.height = Mathf.Max(0.02f, gatherProgressBarHeight);
        gatherProgressBar.hideWhenZero = true;
        gatherProgressBar.progress01 = 0f;
    }

    void UpdateGatherProgressBar()
    {
        if (!showGatherProgressBar)
            return;

        if (gatherProgressBar == null)
            EnsureGatherProgressBar();

        if (gatherProgressBar == null)
            return;

        if (state != State.Gathering || gatherTickSeconds <= 0f)
        {
            gatherProgressBar.progress01 = 0f;
            return;
        }

        gatherProgressBar.progress01 = Mathf.Clamp01(gatherTimer / Mathf.Max(0.01f, gatherTickSeconds));
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
        }

        if (health != null && !health.IsAlive)
            return;

        return;
    }

    bool NeedsFood() => needsController != null && needsController.NeedsFood();
    void ResumeAfterNeed()
    {
        State fallback = ResolveRoleFallbackState();
        if (targetCraftingBuilding != null && (jobType == CivilianJobType.Crafter || CivilianJobRegistry.GetProfile(jobType).supportsCraftingAssignment))
        {
            state = State.GoingToWorkPoint;
            hasStoredRoleState = false;
            return;
        }

        if (hasStoredRoleState)
        {
            state = stateBeforeNeed;
            hasStoredRoleState = false;

            if (state == State.SeekingFoodStorage || state == State.Eating || state == State.SeekingHouse || state == State.Sleeping)
                state = fallback;
        }
        else
        {
            state = fallback;
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

    void TickIdle()
    {
        if (jobType != CivilianJobType.Idle)
            return;

        TryAssignHouseIfNeeded();

        if (assignedHouse == null)
            return;

        idleWanderTimer -= Time.deltaTime;
        if (idleWanderTimer <= 0f || (idleWanderTarget - transform.position).sqrMagnitude < 1f)
        {
            idleWanderTimer = Random.Range(2f, 5f);
            Vector2 jitter = Random.insideUnitCircle * 3f;
            idleWanderTarget = assignedHouse.transform.position + new Vector3(jitter.x, 0f, jitter.y);
        }

        float stop = ResolveStopDistance(assignedHouse.transform, BuildingStopDistanceType.House);
        movementController.MoveTo(idleWanderTarget, stop);
    }

    void TickSeekFoodStorage()
    {
        if (TryConsumeFoodFromAssignedHouse())
            return;

        if (TeamStorageManager.Instance == null)
            return;

        if (targetFoodStorage == null || targetFoodStorage.teamID != teamID || targetFoodStorage.GetStored(targetFoodResource) <= 0)
        {
            if (!TryFindBestFoodStorage(out targetFoodStorage, out targetFoodResource, out targetFoodDefinition))
                return;
        }

        float stop = ResolveStopDistance(targetFoodStorage.transform, BuildingStopDistanceType.Storage);
        movementController.MoveTo(targetFoodStorage.transform.position, stop);
        if (!movementController.HasArrived())
            return;

        pendingFoodAmount = Mathf.Max(1, foodToEatPerMeal);
        int eatenUnits = targetFoodStorage.Withdraw(targetFoodResource, pendingFoodAmount);
        if (eatenUnits <= 0)
        {
            targetFoodStorage = null;
            return;
        }

        pendingFoodAmount = eatenUnits;
        currentEatDurationSeconds = targetFoodDefinition != null ? Mathf.Max(0.1f, targetFoodDefinition.eatTime) : eatDurationSeconds;
        needActionTimer = 0f;
        state = State.Eating;
    }

    bool TryConsumeFoodFromAssignedHouse()
    {
        if (assignedHouse == null)
            return false;

        float nearDistance = Mathf.Max(0.1f, stopDistance * 2f);
        if ((transform.position - assignedHouse.transform.position).sqrMagnitude > nearDistance * nearDistance)
            return false;

        foreach (FoodDefinition food in EnumerateConfiguredFoods())
        {
            if (!TryGetFoodResource(food, out ResourceDefinition type))
                continue;

            if (!assignedHouse.TryConsumeFood(type, Mathf.Max(1, foodToEatPerMeal), out int consumed) || consumed <= 0)
                continue;

            targetFoodResource = type;
            targetFoodDefinition = food;
            pendingFoodAmount = consumed;
            currentEatDurationSeconds = Mathf.Max(0.1f, food.eatTime);
            needActionTimer = 0f;
            state = State.Eating;
            targetFoodStorage = null;
            return true;
        }

        return false;
    }

    void TickEating()
    {
        needActionTimer += Time.deltaTime;

        if (needActionTimer < currentEatDurationSeconds)
            return;

        // NeedsController handles the actual hunger restoration
        needsController.BeginEating(targetFoodDefinition, pendingFoodAmount);

        pendingFoodAmount = 0;
        targetFoodStorage = null;
        targetFoodDefinition = null;

        if (needsController.ShouldSleepNow())
            state = State.SeekingHouse;
        else if (!needsController.ShouldEatNow())
            ResumeAfterNeed();
        else
            state = State.SeekingFoodStorage;
    }

    void TickSeekHouse()
    {
        if (targetHouse == null || !targetHouse.IsAlive)
        {
            TryAssignHouseIfNeeded();
            targetHouse = assignedHouse != null && assignedHouse.IsAlive
                ? assignedHouse
                : FindClosestAvailableHouse();
        }

        if (targetHouse == null)
            return;

        float stop = ResolveStopDistance(targetHouse.transform, BuildingStopDistanceType.House);
        movementController.MoveTo(targetHouse.transform.position, stop);
        if (!movementController.HasArrived())
            return;

        if (!targetHouse.TryAddResident(this))
        {
            targetHouse = null;
            return;
        }

        if (assignedHouse != targetHouse)
        {
            ClearAssignedHouse();
            assignedHouse = targetHouse;
        }

        needActionTimer = 0f;
        state = State.Sleeping;
    }

    void TickSleeping()
    {
        if (assignedHouse == null)
        {
            state = State.SeekingHouse;
            return;
        }

        needActionTimer += Time.deltaTime;

        needsController.TickSleep(Time.deltaTime);
        currentTiredness = needsController.CurrentTiredness;

        ConsumeHouseFoodWhileResting();

        if (needActionTimer < sleepDurationSeconds)
            return;

        needsController.CompleteSleep();
        currentTiredness = needsController.CurrentTiredness;

        targetHouse = assignedHouse;
        ResumeAfterNeed();
    }

    bool TryFindBestFoodStorage(out ResourceStorageContainer bestStorage, out ResourceDefinition bestType, out FoodDefinition bestFood)
    {
        bestStorage = null;
        bestType = null;
        bestFood = null;

        ResourceStorageContainer[] storages = FindObjectsByType<ResourceStorageContainer>(FindObjectsSortMode.None);
        float bestScore = float.MinValue;

        for (int s = 0; s < storages.Length; s++)
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

    House FindClosestAvailableHouse()
    {
        return House.FindAvailableForTeam(teamID, this, transform.position);
    }

    void TryAssignHouseIfNeeded()
    {
        if (assignedHouse != null && assignedHouse.IsAlive)
            return;

        targetHouse = FindClosestAvailableHouse();
        if (targetHouse != null && targetHouse.TryAddResident(this))
            assignedHouse = targetHouse;
    }

    void ClearAssignedHouse()
    {
        if (assignedHouse != null)
            assignedHouse.RemoveResident(this);

        assignedHouse = null;
    }

    public void SetRole(CivilianRole newRole)
    {
        SetJobType(CivilianJobRegistry.ToJobType(newRole));
    }

    public void SetJobType(CivilianJobType newJobType)
    {
        jobType = newJobType;
        role = CivilianJobRegistry.GetProfile(jobType).legacyRole;

        GrantStartingToolForCurrentJob();


        targetSite = null;
        targetStorage = null;

        CurrentAssignedSite = null;
        CurrentDeliverySite = null;
        ClearCraftingAssignment();

        if (carriedAmount > 0)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        state = ResolveRoleFallbackState();
    }

    public void GrantStartingToolForCurrentJob()
    {
        if (!CivilianToolRegistry.TryGetPreferredStartingTool(jobType, out CivilianToolType toolType))
            return;

        equippedTools.Add(toolType);
    }

    public void AssignGatherJob(ResourceNode node)
    {
        HasJob = node != null;
        CurrentNode = node;

        carriedAmount = 0;
        carryingController?.SetCarried(carriedResource, carriedAmount);
        targetSite = null;
        targetStorage = null;

        CurrentAssignedSite = null;
        CurrentDeliverySite = null;

        role = CivilianRole.Gatherer;
        forcedNode = node;

    }

    public void AssignPreferredNode(ResourceNode node)
    {
        gatheringController.AssignNode(node);
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
            float stop = ResolveStopDistance(null, BuildingStopDistanceType.Default);
            movementController.MoveTo(worldPos, stop);
        }
    }
    private void TickSearchNode()
    {
        // Ask the controller to find a valid node
        if (gatheringController.TryFindNode(out var node))
        {
            // Attempt to reserve + move
            if (gatheringController.AssignNode(node))
            {
                SetState(State.GoingToNode);
                return;
            }
        }

        // If we reach here, no node was found OR reservation failed
        // Civilian should retry later
        // (You can add idle animation or wander logic here)
    }
    private void TickGoNode()
    {
        // If full before arrival, go deposit instead
        if (carryingController.IsFull)
        {
            SetState(State.GoingToDepositStorage);
            return;
        }

        var node = gatheringController.CurrentNode;

        if (node == null)
        {
            SetState(State.SearchingNode);
            return;
        }

        if (movementController.HasArrived())
            SetState(State.Gathering);
    }
    private void TickGather()
    {
        // If we lost the node, restart search
        if (gatheringController.CurrentNode == null)
        {
            SetState(State.SearchingNode);
            return;
        }

        // If full, go deposit
        if (carryingController.IsFull)
        {
            SetState(State.FindDepositStorage);
            return;
        }

        // Attempt to gather
        bool gathered = gatheringController.TickGathering();

        // If gathering failed because node depleted, restart search
        if (!gathered && gatheringController.CurrentNode == null)
        {
            SetState(State.SearchingNode);
            return;
        }
    }
    private void TickGoDeposit()
    {
        gatheringController.MoveToDeposit(targetStorage.transform);

        if (movementController.HasArrived())
            state = State.Depositing;
    }
    private void TickDeposit()
    {
        if (gatheringController.TryDeposit(targetStorage))
        {
            carryingController.ClearCarried();
            state = State.SearchingNode;
            return;
        }

        // Could not deposit → idle
        state = State.Idle;
    }
    private void TickFindDepositStorage()
    {
        if (gatheringController.TryFindDepositStorage(out targetStorage))
        {
            state = State.GoingToDepositStorage;
            return;
        }

        // No storage found → idle
        state = State.Idle;
    }
    void TickDepositing()
    {
        if (targetStorage == null)
        {
            state = State.FindDepositStorage;
            return;
        }

        // Get the REAL storage container component
        var facility = targetStorage.GetComponent<StorageFacility>();
        var container = facility != null
            ? facility.GetComponent<ResourceStorageContainer>()
            : null;

        if (container == null)
        {
            Debug.LogError("Gatherer: StorageFacility has no ResourceStorageContainer component!");
            state = State.SearchingNode;
            return;
        }

        // Use the correct TryDeposit overload
        if (gatheringController.TryDeposit(container))
        {
            state = State.SearchingNode;
        }
    }
    void TickGoingToDepositStorage()
    {
        if (targetStorage == null)
        {
            state = State.FindDepositStorage;
            return;
        }

        // Resolve stop distance
        float stop = ResolveStopDistance(targetStorage.transform, BuildingStopDistanceType.Storage);

        // Resolve interaction point
        Vector3 targetPos;
        var facility = targetStorage.GetComponent<StorageFacility>();
        if (facility != null && facility.interactionPoint != null)
            targetPos = facility.interactionPoint.position;
        else
            targetPos = targetStorage.transform.position;

        // Move to the correct point
        movementController.MoveTo(targetPos, stop);

        if (movementController.HasArrived())
            state = State.Depositing;
    }
    void TickSearchBuildSite()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer < searchRetrySeconds) return;
        searchTimer = 0f;

        targetSite = FindNearestConstructionSite(teamID, transform.position);
        CurrentAssignedSite = targetSite;

        if (targetSite == null) return;

        if (!targetSite.MaterialsComplete && buildersCanHaulMaterials)
        {
            state = State.SearchingSupplySite;
            return;
        }

        state = State.GoingToBuildSite;

        float stop = ResolveStopDistance(targetSite.transform, BuildingStopDistanceType.Construction);
        movementController.MoveTo(targetSite.transform.position, stop);
    }

    void TickGoBuildSite()
    {
        if (targetSite == null || targetSite.IsComplete)
        {
            targetSite = null;
            CurrentAssignedSite = null;
            state = State.SearchingBuildSite;
            return;
        }

        if (!targetSite.MaterialsComplete)
        {
            state = buildersCanHaulMaterials ? State.SearchingSupplySite : State.SearchingBuildSite;
            return;
        }

        float stop = ResolveStopDistance(targetSite.transform, BuildingStopDistanceType.Construction);
        movementController.MoveTo(targetSite.transform.position, stop);
        if (movementController.HasArrived()) state = State.Building;
    }

    void TickBuild()
    {
        if (targetSite == null || targetSite.IsComplete)
        {
            targetSite = null;
            CurrentAssignedSite = null;
            state = State.SearchingBuildSite;
            return;
        }

        if (!targetSite.MaterialsComplete)
        {
            state = buildersCanHaulMaterials ? State.SearchingSupplySite : State.SearchingBuildSite;
            return;
        }

        targetSite.AddWork(Time.deltaTime * Mathf.Max(0.25f, GetBuildMultiplier()));
    }

    // ---------- Hauler / Supply to Construction ----------

    void TickSearchSupplySite()
    {
        if (jobType == CivilianJobType.Hauler && TryHandleCraftingHaulerPriority())
            return;

        if (carriedAmount > 0 && targetSite != null)
        {
            state = State.GoingToDeliverSite;
            return;
        }

        searchTimer += Time.deltaTime;
        if (searchTimer < searchRetrySeconds) return;
        searchTimer = 0f;

        targetSite = FindNearestConstructionSite(teamID, transform.position);
        CurrentDeliverySite = targetSite;

        if (targetSite == null) return;
        if (targetSite.IsComplete || targetSite.MaterialsComplete)
        {
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        if (!TryChooseNeededResource(targetSite, out carriedResource))
        {
            targetSite = null;
            CurrentDeliverySite = null;
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        state = State.GoingToPickupStorage;
    }


    bool TryHandleCraftingHaulerPriority()
    {
        if (targetCraftingBuilding != null)
        {
            if (carriedAmount > 0)
            {
                if (targetCraftingBuilding.NeedsInput(carriedResource))
                    state = State.DeliveringCraftInput;
                else
                    state = State.DeliveringCraftOutput;
                return true;
            }

            if (targetCraftingBuilding.NeedsAnyInput())
            {
                state = State.FetchingCraftInput;
                return true;
            }

            if (targetCraftingBuilding.HasAnyOutputQueued())
            {
                state = State.CollectingCraftOutput;
                return true;
            }

            state = State.FetchingCraftInput;
            return true;
        }

        if (carriedAmount > 0)
        {
            var inputBuilding = CraftingJobManager.Instance?.FindNearestBuildingNeedingInput(teamID, carriedResource, transform.position);
            if (inputBuilding != null)
            {
                targetCraftingBuilding = inputBuilding;
                state = State.DeliveringCraftInput;
                return true;
            }

            state = State.GoingToDepositStorage;
            return true;
        }

        CraftingBuilding prioritized = CraftingJobManager.Instance?.FindNearestBuildingWithInputPriority(teamID, transform.position);
        if (prioritized != null)
        {
            targetCraftingBuilding = prioritized;
            state = prioritized.NeedsAnyInput() ? State.FetchingCraftInput : State.CollectingCraftOutput;
            return true;
        }

        return false;
    }

    void TickGoPickup()
    {
        if (TeamStorageManager.Instance == null || targetSite == null)
        {
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        // Only pick up up to what is reserved for this site
        int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(targetSite.SiteKey, carriedResource);
        if (reservedForSite <= 0)
        {
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        if (targetStorage == null)
            targetStorage = TeamStorageManager.Instance.FindNearestStorageWithStored(teamID, carriedResource, transform.position);

        if (targetStorage == null)
            return;

        float stop = ResolveStopDistance(targetStorage.transform, BuildingStopDistanceType.Storage);
        movementController.MoveTo(targetStorage.transform.position, stop);

        if (movementController.HasArrived())
            state = State.PickingUp;
    }

    void TickPickup()
    {
        if (TeamStorageManager.Instance == null || targetStorage == null || targetSite == null)
        {
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int missing = targetSite.GetMissing(carriedResource);
        int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(targetSite.SiteKey, carriedResource);

        int want = Mathf.Min(GetCarryCapacity(), missing);
        want = Mathf.Min(want, reservedForSite);

        if (want <= 0)
        {
            targetStorage = null;
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int took = targetStorage.Withdraw(carriedResource, want);
        if (took <= 0)
        {
            targetStorage = null;
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        // Reduce reservation as soon as the resources are physically removed from storage
        TeamStorageManager.Instance.ConsumeReserved(teamID, targetSite.SiteKey, carriedResource, took);

        carriedAmount = took;
        carryingController?.SetCarried(carriedResource, carriedAmount);

        targetStorage = null;
        state = State.GoingToDeliverSite;
        float stop = ResolveStopDistance(targetSite.transform, BuildingStopDistanceType.Construction);
        movementController.MoveTo(targetSite.transform.position, stop);
    }

    void TickGoDeliver()
    {
        if (targetSite == null)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        float stop = ResolveStopDistance(targetSite.transform, BuildingStopDistanceType.Construction);
        movementController.MoveTo(targetSite.transform.position, stop);
        if (movementController.HasArrived()) state = State.Delivering;
    }

    void TickDeliver()
    {
        if (targetSite == null || carriedAmount <= 0)
        {
            state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int accepted = targetSite.ReceiveDelivery(carriedResource, carriedAmount);
        carriedAmount -= accepted;
        carryingController?.SetCarried(carriedResource, carriedAmount);

        if (carriedAmount > 0)
        {
            // if somehow couldn’t accept, put it back into storage
            state = State.GoingToDepositStorage;
            return;
        }

        state = (jobType == CivilianJobType.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
    }

    // ---------- Helpers ----------

    bool IsCraftingLogisticsHauler()
    {
        return targetCraftingBuilding != null
            && targetCraftingBuilding.requireHaulerLogistics
            && jobType == CivilianJobType.Hauler;
    }

    float ResolveStopDistance(Transform destination, BuildingStopDistanceType stopType)
    {
        if (destination == null)
            return Mathf.Max(0.1f, stopDistance);

        var settings = destination.GetComponentInParent<BuildingInteractionSettings>();
        if (settings == null)
            return Mathf.Max(0.1f, stopDistance);

        return settings.GetStopDistance(stopType, stopDistance);
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
        ConstructionSite[] sites = FindObjectsByType<ConstructionSite>(FindObjectsSortMode.None);
        ConstructionSite best = null;
        float bestD = float.MaxValue;

        for (int i = 0; i < sites.Length; i++)
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

        return requiredType == CivilianJobType.Generalist || jobType == requiredType || CivilianJobRegistry.GetProfile(jobType).supportsCraftingAssignment;
    }

    public void AssignCraftingBuilding(CraftingBuilding building, bool manual = false)
    {
        if (targetCraftingBuilding != null && targetCraftingBuilding != building)
            targetCraftingBuilding.RemoveWorker(this);

        if (building != null && !building.TryAssignWorker(this, manual))
        {
            if (targetCraftingBuilding == building)
                targetCraftingBuilding = null;

            state = ResolveRoleFallbackState();
            return;
        }

        if (jobType == CivilianJobType.Idle || jobType == CivilianJobType.Gatherer || jobType == CivilianJobType.Builder)
            SetJobType(CivilianJobType.Crafter);

        targetCraftingBuilding = building;
        manualCraftingAssignment = manual;
        state = State.FetchingCraftInput;
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

    void TickFetchCraftInput()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (targetCraftingBuilding.requireHaulerLogistics && jobType != CivilianJobType.Hauler)
        {
            state = State.GoingToWorkPoint;
            return;
        }

        if (carriedAmount > 0)
        {
            state = targetCraftingBuilding.NeedsInput(carriedResource)
                ? State.DeliveringCraftInput
                : State.GoingToDepositStorage;
            return;
        }

        if (targetCraftingBuilding.TryGetInputRequest(transform.position, out carriedResource, out int amount, out targetStorage))
        {
            if (targetStorage == null)
            {
                ProductionNotificationManager.Instance?.NotifyIfReady($"missing-storage-{targetCraftingBuilding.GetInstanceID()}", $"{targetCraftingBuilding.name}: no storage with {carriedResource} found.");
                return;
            }

            float stop = ResolveStopDistance(targetStorage.transform, BuildingStopDistanceType.Storage);
            movementController.MoveTo(targetStorage.transform.position, stop);
            if (movementController.HasArrived())
            {
                int took = targetStorage.Withdraw(carriedResource, Mathf.Min(amount, GetCarryCapacity()));
                carriedAmount = took;
        carryingController?.SetCarried(carriedResource, carriedAmount);
                state = took > 0 ? State.DeliveringCraftInput : State.FetchingCraftInput;
            }
            return;
        }

        if (targetCraftingBuilding.requireHaulerLogistics)
            state = targetCraftingBuilding.HasAnyOutputQueued() ? State.CollectingCraftOutput : State.FetchingCraftInput;
        else
            state = targetCraftingBuilding.HasAnyOutputQueued() ? State.CollectingCraftOutput : State.GoingToWorkPoint;
    }

    void TickDeliverCraftInput()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        Vector3 slot = targetCraftingBuilding.inputSlot != null ? targetCraftingBuilding.inputSlot.position : targetCraftingBuilding.transform.position;
        Transform ctx = targetCraftingBuilding != null ? targetCraftingBuilding.transform : null;
        float stop = ResolveStopDistance(ctx, BuildingStopDistanceType.CraftInput);
        movementController.MoveTo(slot, stop);

        if (!movementController.HasArrived()) return;

        if (carriedAmount <= 0)
        {
            state = State.FetchingCraftInput;
            return;
        }

        targetCraftingBuilding.TryDeliverInput(carriedResource, carriedAmount, out int accepted);
        carriedAmount -= accepted;
        carryingController?.SetCarried(carriedResource, carriedAmount);

        if (carriedAmount > 0)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        if (IsCraftingLogisticsHauler())
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (jobType == CivilianJobType.Crafter)
            state = State.GoingToWorkPoint;
        else
            state = ResolveRoleFallbackState();
    }

    void TickGoWorkPoint()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (IsCraftingLogisticsHauler())
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (!targetCraftingBuilding.TryReserveWorkPoint(this, out targetWorkPoint) || targetWorkPoint == null)
            return;

        Transform ctx = targetCraftingBuilding != null ? targetCraftingBuilding.transform : targetWorkPoint;
        float stop = ResolveStopDistance(ctx, BuildingStopDistanceType.CraftWork);
        movementController.MoveTo(targetWorkPoint.position, stop);
        if (movementController.HasArrived())
        {
            stalledAtWorkPointTimer = 0f;
            state = State.CraftingAtWorkPoint;
            return;
        }

        stalledAtWorkPointTimer += Time.deltaTime;
        if (stalledAtWorkPointTimer < 10f)
            return;

        Debug.LogWarning($"[{nameof(Civilian)}] Workpoint stalled for {name}. Clearing crafting assignment.");
        stalledAtWorkPointTimer = 0f;
        targetCraftingBuilding.RemoveWorker(this);
        ClearCraftingAssignment();
        state = ResolveRoleFallbackState();
    }

    void TickCraftAtWorkPoint()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (IsCraftingLogisticsHauler())
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (!targetCraftingBuilding.requireHaulerLogistics)
        {
            if (targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForInputs)
                state = State.FetchingCraftInput;
            else if (targetCraftingBuilding.State == CraftingBuilding.ProductionState.OutputReady || targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForPickup)
                state = State.CollectingCraftOutput;
        }
    }

    void TickCollectCraftOutput()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (targetCraftingBuilding.requireHaulerLogistics && jobType != CivilianJobType.Hauler)
        {
            state = State.GoingToWorkPoint;
            return;
        }

        if (targetCraftingBuilding.NeedsAnyInput())
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (carriedAmount > 0)
        {
            state = targetCraftingBuilding.NeedsInput(carriedResource)
                ? State.DeliveringCraftInput
                : State.DeliveringCraftOutput;
            return;
        }

        if (!targetCraftingBuilding.TryGetOutputRequest(transform.position, out carriedResource, out int amount, out targetStorage))
        {
            state = State.FetchingCraftInput;
            return;
        }

        Vector3 slot = targetCraftingBuilding.outputSlot != null ? targetCraftingBuilding.outputSlot.position : targetCraftingBuilding.transform.position;
        Transform ctx = targetCraftingBuilding != null ? targetCraftingBuilding.transform : null;
        float stop = ResolveStopDistance(ctx, BuildingStopDistanceType.CraftOutput);
        movementController.MoveTo(slot, stop);
        if (!movementController.HasArrived()) return;

        carriedAmount = targetCraftingBuilding.CollectOutput(carriedResource, Mathf.Min(amount, GetCarryCapacity()));
        carryingController?.SetCarried(carriedResource, carriedAmount);
        state = carriedAmount > 0 ? State.DeliveringCraftOutput : State.FetchingCraftInput;
    }

    void TickDeliverCraftOutput()
    {
        if (carriedAmount <= 0)
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (targetCraftingBuilding != null && targetCraftingBuilding.NeedsInput(carriedResource))
        {
            state = State.DeliveringCraftInput;
            return;
        }

        if (targetStorage == null || targetStorage.teamID != teamID || targetStorage.GetFree(carriedResource) <= 0)
            targetStorage = TeamStorageManager.Instance?.FindNearestStorageCached(teamID, carriedResource, transform.position, true);

        if (targetStorage == null)
        {
            ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{teamID}-{carriedResource}", $"No free storage for {carriedResource}. Production stalled.");
            return;
        }

        float stop = ResolveStopDistance(targetStorage.transform, BuildingStopDistanceType.Storage);
        movementController.MoveTo(targetStorage.transform.position, stop);
        if (!movementController.HasArrived()) return;

        int accepted = targetStorage.Deposit(carriedResource, carriedAmount);
        carriedAmount -= accepted;
        carryingController?.SetCarried(carriedResource, carriedAmount);

        if (carriedAmount > 0)
            ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{targetStorage.GetInstanceID()}-{carriedResource}", $"Storage full for {carriedResource}. Output queue blocked.");

        if (carriedAmount > 0)
            state = State.DeliveringCraftOutput;
        else
            state = IsCraftingLogisticsHauler() ? State.FetchingCraftInput : State.GoingToWorkPoint;
    }

    float GetMovementSpeedMultiplier()
    {
        float multiplier = 1f + (GetHouseSpeedBonusMultiplier() - 1f);
        multiplier *= GetToolMoveSpeedMultiplier();

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

    string BuildTaskLabel()
    {
        switch (state)
        {
            case State.Idle: return "Waiting for job";
            case State.SeekingFoodStorage: return "Seeking food";
            case State.Eating: return "Eating";
            case State.SeekingHouse: return "Seeking house";
            case State.Sleeping: return "Sleeping";
            case State.SearchingNode: return "Searching for resource node";
            case State.GoingToNode: return "Moving to target node";
            case State.Gathering: return "Collecting resource";
            case State.GoingToDepositStorage: return "Moving to storage";
            case State.Depositing: return "Depositing carried resources";
            case State.SearchingBuildSite: return "Searching for construction work";
            case State.GoingToBuildSite: return "Moving to build site";
            case State.Building: return "Building structure";
            case State.SearchingSupplySite: return "Waiting for haul assignment";
            case State.GoingToPickupStorage: return "Moving to pickup";
            case State.PickingUp: return "Collecting resource from storage";
            case State.GoingToDeliverSite: return "Moving to delivery target";
            case State.Delivering: return "Delivering resources";
            case State.FetchingCraftInput: return "Fetching crafting inputs";
            case State.DeliveringCraftInput: return "Delivering crafting inputs";
            case State.GoingToWorkPoint: return "Moving to work point";
            case State.CraftingAtWorkPoint: return "Producing goods";
            case State.CollectingCraftOutput: return "Collecting outputs";
            case State.DeliveringCraftOutput: return "Delivering crafted goods";
            default: return state.ToString();
        }
    }

    string BuildStateDetails()
    {
        string target = CurrentTargetName;
        switch (state)
        {
            case State.Idle:
                return HasJob ? "Idle between assignments" : "No assignment yet";
            case State.SearchingNode:
            case State.SearchingBuildSite:
            case State.SearchingSupplySite:
            case State.FetchingCraftInput:
            case State.CollectingCraftOutput:
                return "Scanning for nearest valid task";
            case State.GoingToNode:
            case State.GoingToDepositStorage:
            case State.GoingToBuildSite:
            case State.GoingToPickupStorage:
            case State.GoingToDeliverSite:
            case State.GoingToWorkPoint:
                return $"Moving toward {target}";
            case State.Gathering:
            case State.PickingUp:
            case State.Depositing:
            case State.Delivering:
            case State.DeliveringCraftInput:
            case State.DeliveringCraftOutput:
                return $"Interacting with {target}";
            case State.CraftingAtWorkPoint:
                return "Producing goods at assigned workstation";
            default:
                return target != "None" ? $"Target: {target}" : "No active target";
        }
    }


    float GetHouseSpeedBonusMultiplier()
    {
        if (assignedHouse == null)
            return 1f;

        return 1f + Mathf.Max(0, assignedHouse.prestige) * 0.1f;
    }

    float GetTirednessRate()
    {
        float reduction = assignedHouse != null ? Mathf.Max(0, assignedHouse.comfort) * 0.1f : 0f;
        float baseRate = needsController != null ? needsController.TirednessRatePerSecond : 0.3f;
        return baseRate * Mathf.Max(0.1f, 1f - reduction);
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
        if (assignedHouse == null || !NeedsFood())
            return;

        foreach (FoodDefinition food in EnumerateConfiguredFoods())
        {
            if (!TryGetFoodResource(food, out ResourceDefinition type))
                continue;

            if (!assignedHouse.TryConsumeFood(type, foodToEatPerMeal, out int consumed) || consumed <= 0)
                continue;

            float restore = consumed * Mathf.Max(1, food.hungerRestore);
            needsController?.RestoreHunger(restore);
        currentHunger = needsController != null ? needsController.CurrentHunger : Mathf.Max(0f, currentHunger - restore);
            if (!NeedsFood())
                return;
        }
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
            ClearAssignedHouse();
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
        if (!CanPerform(task.requiredCapability))
            return false;

        switch (task.taskType)
        {
            case WorkerTaskType.Gather:
                if (task.resourceNode == null) return false;
                SetJobType(CivilianJobType.Gatherer);
                AssignPreferredNode(task.resourceNode);
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

}
