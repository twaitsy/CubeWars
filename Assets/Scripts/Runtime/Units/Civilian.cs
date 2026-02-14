using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Civilian : MonoBehaviour, ITargetable, IHasHealth
{
    [Header("Identity")]
    public string unitDefinitionId = "civilian";

    [Header("Team")]
    public int teamID;

    [Header("Job (Unified Registry)")]
    public CivilianJobType jobType = CivilianJobType.Gatherer;
    [Tooltip("Legacy mirror of jobType for older systems/UI.")]
    public CivilianRole role = CivilianRole.Gatherer;

    public CivilianJobType JobType => jobType;

    [Header("Health")]
    public float maxHealth = 50f;

    [Header("Movement")]
    public float speed = 2.5f;
    public float stopDistance = 1.2f;
    public bool useRoadBonus = true;
    public float roadSpeedMultiplier = 1.2f;

    [Header("Carrying")]
    public int carryCapacity = 30;

    [Header("Gathering")]
    public float gatherTickSeconds = 0.8f;
    public int harvestPerTick = 10;
    public float searchRetrySeconds = 1.5f;

    [Header("Building/Hauling")]
    public bool buildersCanHaulMaterials = true;
    public float retargetSeconds = 0.6f;

    [Header("Needs - Food")]
    [Tooltip("Resource database used to resolve ResourceDefinition assets by id.")]
    public ResourcesDatabase resourcesDatabase;
    [Tooltip("Maps edible resources to hunger/eat/cooking/spoilage data.")]
    public FoodDatabase foodDatabase;
    [Min(0.01f)] public float hungerRatePerSecond = 0.45f;
    [Min(1f)] public float maxHunger = 100f;
    [Range(0.1f, 1f)] public float seekFoodThreshold01 = 0.55f;
    [Min(1)] public int foodToEatPerMeal = 10;
    [Min(0.1f)] public float eatDurationSeconds = 1.2f;
    [Min(0f)] public float starvationDamagePerSecond = 2f;

    [Header("Needs - Sleep")]
    [Min(0.01f)] public float tirednessRatePerSecond = 0.3f;
    [Min(1f)] public float maxTiredness = 100f;
    [Range(0.1f, 1f)] public float seekSleepThreshold01 = 0.6f;
    [Min(0.1f)] public float sleepDurationSeconds = 5f;
    [Min(0f)] public float exhaustionDamagePerSecond = 1f;

    // Compatibility fields referenced elsewhere
    public ResourceNode CurrentReservedNode { get; set; }
    public ConstructionSite CurrentAssignedSite { get; set; }
    public ConstructionSite CurrentDeliverySite { get; set; }

    // Expose carried for UI
    public ResourceDefinition CarriedType => carriedResource;
    public int CarriedAmount => carriedAmount;

    private float currentHealth;
    private NavMeshAgent agent;

    private ResourceDefinition carriedResource;
    private int carriedAmount;

    // Legacy/compat fields (kept for external references)
    public bool HasJob;
    public ResourceNode CurrentNode;

    private float gatherTimer;
    private float searchTimer;
    private float retargetTimer;

    private ResourceNode targetNode;
    private ResourceNode forcedNode;
    private ConstructionSite targetSite;
    private ResourceStorageContainer targetStorage;

    private CraftingBuilding targetCraftingBuilding;
    private Transform targetWorkPoint;
    private bool manualCraftingAssignment;

    public CraftingBuilding AssignedCraftingBuilding => targetCraftingBuilding;
    public string CurrentTaskLabel => BuildTaskLabel();
    public float CraftingProgress => targetCraftingBuilding != null ? targetCraftingBuilding.CraftProgress01 : 0f;
    public bool IsAtAssignedCraftingWorkPoint => state == State.CraftingAtWorkPoint && Arrived();

    public float CurrentHunger => currentHunger;
    public float CurrentTiredness => currentTiredness;
    public House AssignedHouse => assignedHouse;

    // IHasHealth / ITargetable style properties
    public int TeamID => teamID;
    public bool IsAlive => currentHealth > 0f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private enum State
    {
        Idle,

        SeekingFoodStorage,
        Eating,
        SeekingHouse,
        Sleeping,

        SearchingNode,
        GoingToNode,
        Gathering,
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

    public string CurrentState => state.ToString();
    public string CurrentTargetName
    {
        get
        {
            if (targetCraftingBuilding != null) return SanitizeName(targetCraftingBuilding.name);
            if (targetSite != null) return SanitizeName(targetSite.name);
            if (targetNode != null) return SanitizeName(targetNode.name);
            if (targetStorage != null) return SanitizeName(targetStorage.name);
            return "None";
        }
    }

    // Registration timing: teamID is often assigned right after Instantiate (before Start).
    // So we register with JobManager in Start, and only re-register on OnEnable if Start has run.
    private bool started;
    private bool registeredWithJobManager;

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

    private readonly HashSet<CivilianToolType> equippedTools = new HashSet<CivilianToolType>();
    private float toolPickupTimer;
    private float idleWanderTimer;
    private Vector3 idleWanderTarget;
    private float stalledAtWorkPointTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;
        agent.autoBraking = true;
    }

    void OnEnable()
    {
        // Only register here if Start has already run (prevents wrong-team registration on Instantiate)
        if (started)
        {
            RegisterWithJobManager();
            CraftingJobManager.Instance?.RegisterCivilian(this);
        }
    }

    void OnDisable()
    {
        ReleaseNodeReservation();
        ClearAssignedHouse();

        if (registeredWithJobManager)
            UnregisterFromJobManager();

        CraftingJobManager.Instance?.UnregisterCivilian(this);
    }

    void Start()
    {
        ApplyDatabaseDefinition();
        currentHealth = maxHealth;
        currentHunger = 0f;
        currentTiredness = 0f;
        SetJobType(jobType);

        started = true;
        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);

        TryAssignHouseIfNeeded();
    }


    private void ApplyDatabaseDefinition()
    {
        var loaded = GameDatabaseLoader.Loaded;
        if (loaded == null || !loaded.TryGetUnitById(unitDefinitionId, out var def) || def == null)
            return;

        if (def.maxHealth > 0)
            maxHealth = def.maxHealth;

        if (def.moveSpeed > 0f)
            speed = def.moveSpeed;

        if (def.carryCapacity > 0)
            carryCapacity = def.carryCapacity;

        if (agent != null && speed > 0f)
            agent.speed = speed;
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

        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);
    }

    public void SetTeamID(int newTeamID)
    {
        if (teamID == newTeamID) return;
        teamID = newTeamID;
        RefreshJobManagerRegistration();
    }

    void Update()
    {
        if (!IsAlive) return;

        TickNeeds();

        agent.speed = speed * GetMovementSpeedMultiplier();
        agent.stoppingDistance = stopDistance;

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
            case State.GoingToDepositStorage: TickGoDeposit(); break;
            case State.Depositing: TickDeposit(); break;

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
    }

    void TickNeeds()
    {
        if (state == State.CraftingAtWorkPoint && Arrived() && targetCraftingBuilding != null && targetCraftingBuilding.State == CraftingBuilding.ProductionState.InProgress)
            return;

        currentHunger = Mathf.Clamp(currentHunger + hungerRatePerSecond * Time.deltaTime, 0f, maxHunger);
        currentTiredness = Mathf.Clamp(currentTiredness + GetTirednessRate() * Time.deltaTime, 0f, maxTiredness);

        TickToolPickup();

        if (currentHunger >= maxHunger)
            TakeDamage(starvationDamagePerSecond * Time.deltaTime);

        if (currentTiredness >= maxTiredness)
            TakeDamage(exhaustionDamagePerSecond * Time.deltaTime);

        if (!IsAlive)
            return;

        if (state == State.Eating || state == State.Sleeping || state == State.SeekingFoodStorage || state == State.SeekingHouse)
            return;

        if (NeedsSleep())
        {
            PushNeedState(State.SeekingHouse);
            return;
        }

        if (NeedsFood())
            PushNeedState(State.SeekingFoodStorage);
    }

    bool NeedsFood() => currentHunger >= maxHunger * Mathf.Clamp01(seekFoodThreshold01);
    bool NeedsSleep() => currentTiredness >= maxTiredness * Mathf.Clamp01(seekSleepThreshold01);

    void PushNeedState(State needState)
    {
        if (!hasStoredRoleState)
        {
            stateBeforeNeed = state;
            hasStoredRoleState = true;
        }

        state = needState;
        targetStorage = null;
        targetFoodStorage = null;
    }

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

        MoveTo(idleWanderTarget, assignedHouse.transform, BuildingStopDistanceType.House);
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

        MoveTo(targetFoodStorage.transform.position, targetFoodStorage.transform, BuildingStopDistanceType.Storage);
        if (!Arrived())
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
        float eatDuration = Mathf.Max(0.1f, currentEatDurationSeconds > 0f ? currentEatDurationSeconds : eatDurationSeconds);
        if (needActionTimer < eatDuration)
            return;

        float restorePerUnit = targetFoodDefinition != null ? Mathf.Max(1, targetFoodDefinition.hungerRestore) : 1f;
        float restore = Mathf.Max(1, pendingFoodAmount) * restorePerUnit;
        currentHunger = Mathf.Max(0f, currentHunger - restore);
        pendingFoodAmount = 0;
        targetFoodStorage = null;
        targetFoodDefinition = null;

        if (NeedsSleep())
            state = State.SeekingHouse;
        else if (!NeedsFood())
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

        MoveTo(targetHouse.transform.position, targetHouse.transform, BuildingStopDistanceType.House);
        if (!Arrived())
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
        currentTiredness = Mathf.Max(0f, currentTiredness - (maxTiredness / Mathf.Max(0.1f, sleepDurationSeconds)) * Time.deltaTime);
        ConsumeHouseFoodWhileResting();

        if (needActionTimer < sleepDurationSeconds)
            return;

        currentTiredness = 0f;
        targetHouse = assignedHouse;
        ResumeAfterNeed();
    }

    bool TryFindBestFoodStorage(out ResourceStorageContainer bestStorage, out ResourceDefinition bestType, out FoodDefinition bestFood)
    {
        bestStorage = null;
        bestType = null;
        bestFood = null;

        ResourceStorageContainer[] storages = FindObjectsOfType<ResourceStorageContainer>();
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

        SetTargetNode(null);
        targetSite = null;
        targetStorage = null;

        CurrentReservedNode = null;
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

    public void AssignGatherJob(ResourceNode node)
    {
        HasJob = node != null;
        CurrentNode = node;

        carriedAmount = 0;
        targetSite = null;
        targetStorage = null;

        CurrentAssignedSite = null;
        CurrentDeliverySite = null;

        role = CivilianRole.Gatherer;
        forcedNode = node;
        bool reserved = TrySetTargetNode(node);
        CurrentReservedNode = reserved ? targetNode : null;

        if (targetNode != null)
        {
            state = State.GoingToNode;
            MoveTo(targetNode.transform.position, targetNode.transform);
        }
        else
        {
            state = State.SearchingNode;
        }
    }

    public void AssignPreferredNode(ResourceNode node)
    {
        forcedNode = (node != null && node.amount > 0) ? node : null;

        if (role != CivilianRole.Gatherer)
            SetRole(CivilianRole.Gatherer);

        bool reserved = TrySetTargetNode(forcedNode);
        CurrentReservedNode = reserved ? targetNode : null;

        if (targetNode != null)
        {
            state = State.GoingToNode;
            MoveTo(targetNode.transform.position, targetNode.transform);
        }
        else if (carriedAmount > 0)
        {
            state = State.GoingToDepositStorage;
        }
        else
        {
            state = State.SearchingNode;
        }
    }



    public void IssueMoveCommand(Vector3 worldPos)
    {
        forcedNode = null;
        SetTargetNode(null);
        targetSite = null;
        targetStorage = null;
        CurrentAssignedSite = null;
        CurrentDeliverySite = null;
        CurrentReservedNode = null;
        HasJob = false;
        CurrentNode = null;
        state = State.Idle;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            MoveTo(worldPos, null);
        }
    }
    // ---------- Gatherer ----------

    void TickSearchNode()
    {
        searchTimer += Time.deltaTime;
        if (searchTimer < searchRetrySeconds) return;
        searchTimer = 0f;

        bool assigned = false;
        if (forcedNode != null && forcedNode.amount > 0)
            assigned = TrySetTargetNode(forcedNode);

        if (!assigned)
            assigned = TrySetTargetNode(FindClosestResourceNode());

        CurrentReservedNode = targetNode;

        if (targetNode != null)
        {
            state = State.GoingToNode;
            MoveTo(targetNode.transform.position, targetNode.transform);
        }
    }

    void TickGoNode()
    {
        if (targetNode == null || targetNode.amount <= 0)
        {
            if (forcedNode == targetNode)
                forcedNode = null;

            SetTargetNode(null);
            CurrentReservedNode = null;
            state = State.SearchingNode;
            return;
        }

        if (!targetNode.TryReserveGatherSlot(this))
        {
            SetTargetNode(null);
            CurrentReservedNode = null;
            state = State.SearchingNode;
            return;
        }

        retargetTimer += Time.deltaTime;
        if (retargetTimer >= retargetSeconds)
        {
            retargetTimer = 0f;
            MoveTo(targetNode.transform.position, targetNode.transform);
        }

        if (Arrived())
        {
            state = State.Gathering;
            gatherTimer = 0f;
        }
    }

    void TickGather()
    {
        if (targetNode == null || targetNode.amount <= 0)
        {
            if (forcedNode == targetNode)
                forcedNode = null;

            SetTargetNode(null);
            CurrentReservedNode = null;
            state = (carriedAmount > 0) ? State.GoingToDepositStorage : State.SearchingNode;
            return;
        }

        if (!targetNode.TryReserveGatherSlot(this))
        {
            SetTargetNode(null);
            CurrentReservedNode = null;
            state = (carriedAmount > 0) ? State.GoingToDepositStorage : State.SearchingNode;
            return;
        }

        gatherTimer += Time.deltaTime;
        if (gatherTimer < gatherTickSeconds) return;
        gatherTimer = 0f;

        if (carriedAmount == 0) carriedResource = targetNode.resource;
        if (carriedResource != targetNode.resource)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        int remaining = GetCarryCapacity() - carriedAmount;
        if (remaining <= 0)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        // Must have physical storage free to avoid “phantom resources”
        if (TeamStorageManager.Instance != null)
        {
            var s = TeamStorageManager.Instance.FindNearestStorageWithFree(teamID, carriedResource, transform.position);
            if (s == null || s.GetFree(carriedResource) <= 0)
            {
                state = State.GoingToDepositStorage;
                return;
            }
        }

        int want = Mathf.Min(GetHarvestPerTick(), remaining);
        int got = targetNode.Harvest(want);
        if (got > 0) carriedAmount += got;

        if (carriedAmount >= GetCarryCapacity())
            state = State.GoingToDepositStorage;
    }

    void TickGoDeposit()
    {
        if (carriedAmount <= 0)
        {
            state = State.SearchingNode;
            return;
        }

        if (TeamStorageManager.Instance == null)
        {
            // fallback: dump into TeamResources primary storage
            TeamResources.Instance?.Deposit(teamID, carriedResource, carriedAmount);
            carriedAmount = 0;
            state = (jobType == CivilianJobType.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            return;
        }

        if (targetStorage == null || targetStorage.teamID != teamID || targetStorage.GetFree(carriedResource) <= 0)
            targetStorage = TeamStorageManager.Instance.FindNearestStorageWithFree(teamID, carriedResource, transform.position);

        if (jobType == CivilianJobType.Gatherer && carriedAmount > 0)
        {
            CraftingBuilding inputBuilding = CraftingJobManager.Instance?.FindNearestBuildingNeedingInput(teamID, carriedResource, transform.position);
            if (inputBuilding != null)
            {
                float craftingDistance = (inputBuilding.transform.position - transform.position).sqrMagnitude;
                float storageDistance = targetStorage != null
                    ? (targetStorage.transform.position - transform.position).sqrMagnitude
                    : float.MaxValue;

                if (craftingDistance <= storageDistance)
                {
                    targetCraftingBuilding = inputBuilding;
                    state = State.DeliveringCraftInput;
                    return;
                }
            }
        }

        if (targetStorage == null)
        {
            if (!TeamStorageManager.Instance.HasAnyStorage(teamID))
            {
                TeamResources.Instance?.Deposit(teamID, carriedResource, carriedAmount);
                carriedAmount = 0;
                state = (jobType == CivilianJobType.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            }
            return;
        }

        MoveTo(targetStorage.transform.position, targetStorage.transform, BuildingStopDistanceType.Storage);

        if (Arrived())
            state = State.Depositing;
    }

    void TickDeposit()
    {
        if (targetStorage == null || carriedAmount <= 0)
        {
            state = (jobType == CivilianJobType.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            return;
        }

        int accepted = targetStorage.Deposit(carriedResource, carriedAmount);
        carriedAmount -= accepted;

        if (carriedAmount > 0)
        {
            targetStorage = null;
            state = State.GoingToDepositStorage;
            return;
        }

        targetStorage = null;
        state = (jobType == CivilianJobType.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
    }

    // ---------- Builder ----------

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
        MoveTo(targetSite.transform.position, targetSite.transform, BuildingStopDistanceType.Construction);
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

        MoveTo(targetSite.transform.position, targetSite.transform, BuildingStopDistanceType.Construction);
        if (Arrived()) state = State.Building;
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

        MoveTo(targetStorage.transform.position, targetStorage.transform, BuildingStopDistanceType.Storage);

        if (Arrived())
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

        targetStorage = null;
        state = State.GoingToDeliverSite;
        MoveTo(targetSite.transform.position, targetSite.transform, BuildingStopDistanceType.Construction);
    }

    void TickGoDeliver()
    {
        if (targetSite == null)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        MoveTo(targetSite.transform.position, targetSite.transform, BuildingStopDistanceType.Construction);
        if (Arrived()) state = State.Delivering;
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

    void MoveTo(Vector3 destination, Transform destinationContext, BuildingStopDistanceType stopType = BuildingStopDistanceType.Default)
    {
        if (agent == null || !agent.enabled)
            return;

        agent.stoppingDistance = ResolveStopDistance(destinationContext, stopType);
        agent.SetDestination(destination);
    }

    bool Arrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance == Mathf.Infinity) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
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

    ResourceNode FindClosestResourceNode()
    {
        var nodes = FindObjectsOfType<ResourceNode>();
        ResourceNode best = null;
        float bestD = float.MaxValue;

        for (int i = 0; i < nodes.Length; i++)
        {
            var n = nodes[i];
            if (n == null || n.amount <= 0) continue;
            if (!n.TryReserveGatherSlot(this)) continue;

            if (TeamStorageManager.Instance != null && TeamStorageManager.Instance.GetTotalFreeInBuildings(teamID, n.resource) <= 0)
            {
                n.ReleaseGatherSlot(this);
                continue;
            }

            float d = (n.transform.position - transform.position).sqrMagnitude;
            if (d < bestD)
            {
                if (best != null)
                    best.ReleaseGatherSlot(this);

                bestD = d;
                best = n;
            }
            else
            {
                n.ReleaseGatherSlot(this);
            }
        }

        return best;
    }

    bool TrySetTargetNode(ResourceNode newNode)
    {
        if (newNode != null && !newNode.TryReserveGatherSlot(this))
            return false;

        if (targetNode == newNode)
            return true;

        if (targetNode != null)
            targetNode.ReleaseGatherSlot(this);

        targetNode = newNode;
        return true;
    }

    void SetTargetNode(ResourceNode newNode)
    {
        TrySetTargetNode(newNode);
    }

    void ReleaseNodeReservation()
    {
        SetTargetNode(null);
        forcedNode = null;
        CurrentReservedNode = null;
    }

    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        return raw.Replace("(Clone)", "").Trim();
    }

    ConstructionSite FindNearestConstructionSite(int team, Vector3 pos)
    {
        ConstructionSite[] sites = FindObjectsOfType<ConstructionSite>();
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
        if (jobType == CivilianJobType.Idle || jobType == CivilianJobType.Gatherer || jobType == CivilianJobType.Builder)
            SetJobType(CivilianJobType.Crafter);

        targetCraftingBuilding = building;
        manualCraftingAssignment = manual;
        state = State.FetchingCraftInput;
    }

    public void ClearCraftingAssignment()
    {
        if (targetCraftingBuilding != null)
            targetCraftingBuilding.ReleaseWorkPoint(this);

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

            MoveTo(targetStorage.transform.position, targetStorage.transform, BuildingStopDistanceType.Storage);
            if (Arrived())
            {
                int took = targetStorage.Withdraw(carriedResource, Mathf.Min(amount, GetCarryCapacity()));
                carriedAmount = took;
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
        MoveTo(slot, targetCraftingBuilding != null ? targetCraftingBuilding.transform : null, BuildingStopDistanceType.CraftInput);

        if (!Arrived()) return;

        if (carriedAmount <= 0)
        {
            state = State.FetchingCraftInput;
            return;
        }

        targetCraftingBuilding.TryDeliverInput(carriedResource, carriedAmount, out int accepted);
        carriedAmount -= accepted;

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

        MoveTo(targetWorkPoint.position, targetCraftingBuilding != null ? targetCraftingBuilding.transform : targetWorkPoint, BuildingStopDistanceType.CraftWork);
        if (Arrived())
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
        MoveTo(slot, targetCraftingBuilding != null ? targetCraftingBuilding.transform : null, BuildingStopDistanceType.CraftOutput);
        if (!Arrived()) return;

        carriedAmount = targetCraftingBuilding.CollectOutput(carriedResource, Mathf.Min(amount, GetCarryCapacity()));
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

        MoveTo(targetStorage.transform.position, targetStorage.transform, BuildingStopDistanceType.Storage);
        if (!Arrived()) return;

        int accepted = targetStorage.Deposit(carriedResource, carriedAmount);
        carriedAmount -= accepted;

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
            case State.SeekingFoodStorage: return "Seeking food";
            case State.Eating: return "Eating";
            case State.SeekingHouse: return "Seeking house";
            case State.Sleeping: return "Sleeping";
            case State.FetchingCraftInput: return "Fetching crafting inputs";
            case State.DeliveringCraftInput: return "Delivering crafting inputs";
            case State.GoingToWorkPoint: return "Moving to work point";
            case State.CraftingAtWorkPoint: return "Operating work point";
            case State.CollectingCraftOutput: return "Collecting outputs";
            case State.DeliveringCraftOutput: return "Delivering crafted goods";
            default: return state.ToString();
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
        return tirednessRatePerSecond * Mathf.Max(0.1f, 1f - reduction);
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
            currentHunger = Mathf.Max(0f, currentHunger - restore);
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
        if (!IsAlive) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if (agent != null) agent.isStopped = true;
            ClearAssignedHouse();
            Destroy(gameObject);
        }
    }

    // If your ITargetable interface expects something like this, you can keep it:
    public Transform GetTransform()
    {
        return transform;
    }
}
