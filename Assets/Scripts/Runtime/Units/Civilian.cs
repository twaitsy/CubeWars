using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Civilian : MonoBehaviour, ITargetable, IHasHealth
{
    [Header("Team")]
    public int teamID;

    [Header("Role")]
    public CivilianRole role = CivilianRole.Gatherer;

    [Header("Job Specialization")]
    public CivilianJobType jobType = CivilianJobType.Generalist;

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

    // Compatibility fields referenced elsewhere
    public ResourceNode CurrentReservedNode { get; set; }
    public ConstructionSite CurrentAssignedSite { get; set; }
    public ConstructionSite CurrentDeliverySite { get; set; }

    // Expose carried for UI
    public ResourceType CarriedType => carriedType;
    public int CarriedAmount => carriedAmount;

    private float currentHealth;
    private NavMeshAgent agent;

    private ResourceType carriedType;
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

    // IHasHealth / ITargetable style properties
    public int TeamID => teamID;
    public bool IsAlive => currentHealth > 0f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private enum State
    {
        Idle,

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

        if (registeredWithJobManager)
            UnregisterFromJobManager();

        CraftingJobManager.Instance?.UnregisterCivilian(this);
    }

    void Start()
    {
        currentHealth = maxHealth;
        SetRole(role);

        started = true;
        RegisterWithJobManager();
        CraftingJobManager.Instance?.RegisterCivilian(this);
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

        agent.speed = speed * GetMovementSpeedMultiplier();
        agent.stoppingDistance = stopDistance;

        switch (state)
        {
            case State.Idle: break;

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

    public void SetRole(CivilianRole newRole)
    {
        role = newRole;

        if (newRole == CivilianRole.Blacksmith) jobType = CivilianJobType.Blacksmith;
        else if (newRole == CivilianRole.Carpenter) jobType = CivilianJobType.Carpenter;
        else if (newRole == CivilianRole.Farmer) jobType = CivilianJobType.Farmer;
        else if (newRole == CivilianRole.Cook) jobType = CivilianJobType.Cook;
        else if (newRole == CivilianRole.Engineer) jobType = CivilianJobType.Engineer;

        SetTargetNode(null);
        targetSite = null;
        targetStorage = null;

        CurrentReservedNode = null;
        CurrentAssignedSite = null;
        CurrentDeliverySite = null;
        ClearCraftingAssignment();

        if (carriedAmount > 0)
        {
            // keep carried as-is, we'll deposit next tick
            state = State.GoingToDepositStorage;
            return;
        }

        switch (role)
        {
            case CivilianRole.Gatherer: state = State.SearchingNode; break;
            case CivilianRole.Builder: state = State.SearchingBuildSite; break;
            case CivilianRole.Hauler: state = State.SearchingSupplySite; break;
            case CivilianRole.Crafter:
            case CivilianRole.Farmer:
            case CivilianRole.Technician:
            case CivilianRole.Scientist:
            case CivilianRole.Engineer:
            case CivilianRole.Blacksmith:
            case CivilianRole.Carpenter:
            case CivilianRole.Cook:
                state = State.FetchingCraftInput;
                break;
            default: state = State.Idle; break;
        }
    }

    /// <summary>
    /// Explicit job assignment from AI/job system.
    /// Forces this civilian into gatherer behaviour on a specific node.
    /// </summary>
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
            agent.SetDestination(targetNode.transform.position);
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
            agent.SetDestination(targetNode.transform.position);
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
            agent.SetDestination(worldPos);
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
            agent.SetDestination(targetNode.transform.position);
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
            agent.SetDestination(targetNode.transform.position);
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

        if (carriedAmount == 0) carriedType = targetNode.type;
        if (carriedType != targetNode.type)
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
            var s = TeamStorageManager.Instance.FindNearestStorageWithFree(teamID, carriedType, transform.position);
            if (s == null || s.GetFree(carriedType) <= 0)
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
            TeamResources.Instance?.Deposit(teamID, carriedType, carriedAmount);
            carriedAmount = 0;
            state = (role == CivilianRole.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            return;
        }

        if (targetStorage == null || targetStorage.teamID != teamID)
            targetStorage = TeamStorageManager.Instance.FindNearestStorageWithFree(teamID, carriedType, transform.position);

        if (targetStorage == null)
        {
            if (!TeamStorageManager.Instance.HasAnyPhysicalStorage(teamID))
            {
                TeamResources.Instance?.Deposit(teamID, carriedType, carriedAmount);
                carriedAmount = 0;
                state = (role == CivilianRole.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            }
            return;
        }

        agent.SetDestination(targetStorage.transform.position);

        if (Arrived())
            state = State.Depositing;
    }

    void TickDeposit()
    {
        if (targetStorage == null || carriedAmount <= 0)
        {
            state = (role == CivilianRole.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
            return;
        }

        int accepted = targetStorage.Deposit(carriedType, carriedAmount);
        carriedAmount -= accepted;

        // If storage was full, drop remaining (simple)
        if (carriedAmount > 0) carriedAmount = 0;

        targetStorage = null;
        state = (role == CivilianRole.Gatherer) ? State.SearchingNode : State.SearchingBuildSite;
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
        agent.SetDestination(targetSite.transform.position);
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

        agent.SetDestination(targetSite.transform.position);
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
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        if (!TryChooseNeededResource(targetSite, out carriedType))
        {
            targetSite = null;
            CurrentDeliverySite = null;
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        state = State.GoingToPickupStorage;
    }

    void TickGoPickup()
    {
        if (TeamStorageManager.Instance == null || targetSite == null)
        {
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        // Only pick up up to what is reserved for this site
        int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(targetSite.SiteKey, carriedType);
        if (reservedForSite <= 0)
        {
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        if (targetStorage == null)
            targetStorage = TeamStorageManager.Instance.FindNearestStorageWithStored(teamID, carriedType, transform.position);

        if (targetStorage == null)
            return;

        agent.SetDestination(targetStorage.transform.position);

        if (Arrived())
            state = State.PickingUp;
    }

    void TickPickup()
    {
        if (TeamStorageManager.Instance == null || targetStorage == null || targetSite == null)
        {
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int missing = targetSite.GetMissing(carriedType);
        int reservedForSite = TeamStorageManager.Instance.GetReservedForSite(targetSite.SiteKey, carriedType);

        int want = Mathf.Min(GetCarryCapacity(), missing);
        want = Mathf.Min(want, reservedForSite);

        if (want <= 0)
        {
            targetStorage = null;
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int took = targetStorage.Withdraw(carriedType, want);
        if (took <= 0)
        {
            targetStorage = null;
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        // Reduce reservation as soon as the resources are physically removed from storage
        TeamStorageManager.Instance.ConsumeReserved(teamID, targetSite.SiteKey, carriedType, took);

        carriedAmount = took;

        targetStorage = null;
        state = State.GoingToDeliverSite;
        agent.SetDestination(targetSite.transform.position);
    }

    void TickGoDeliver()
    {
        if (targetSite == null)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        agent.SetDestination(targetSite.transform.position);
        if (Arrived()) state = State.Delivering;
    }

    void TickDeliver()
    {
        if (targetSite == null || carriedAmount <= 0)
        {
            state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
            return;
        }

        int accepted = targetSite.ReceiveDelivery(carriedType, carriedAmount);
        carriedAmount -= accepted;

        if (carriedAmount > 0)
        {
            // if somehow couldn’t accept, put it back into storage
            state = State.GoingToDepositStorage;
            return;
        }

        state = (role == CivilianRole.Builder) ? State.SearchingBuildSite : State.SearchingSupplySite;
    }

    // ---------- Helpers ----------

    bool Arrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance == Mathf.Infinity) return false;
        return agent.remainingDistance <= agent.stoppingDistance + 0.15f;
    }

    int GetCarryCapacity()
    {
        if (carryCapacity > 0)
            return carryCapacity;

        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null && stats.CarryCapacity > 0) return stats.CarryCapacity;
        return 1;
    }

    int GetHarvestPerTick()
    {
        if (harvestPerTick > 0)
            return harvestPerTick;

        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null && stats.HarvestPerTick > 0) return stats.HarvestPerTick;
        return 1;
    }

    float GetBuildMultiplier()
    {
        CharacterStats stats = GetComponent<CharacterStats>();
        if (stats != null) return stats.BuildWorkMultiplier;
        return 1f;
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

            if (TeamStorageManager.Instance != null && TeamStorageManager.Instance.GetTotalFreeInBuildings(teamID, n.type) <= 0)
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

    bool TryChooseNeededResource(ConstructionSite site, out ResourceType neededType)
    {
        neededType = default;
        if (site == null) return false;

        ResourceCost[] costs = site.GetRequiredCosts();
        if (costs == null || costs.Length == 0) return false;

        int bestMissing = 0;
        ResourceType bestType = costs[0].type;

        for (int i = 0; i < costs.Length; i++)
        {
            int missing = site.GetMissing(costs[i].type);
            if (missing > bestMissing)
            {
                bestMissing = missing;
                bestType = costs[i].type;
            }
        }

        if (bestMissing <= 0) return false;

        neededType = bestType;
        return true;
    }


    public void SetJobType(CivilianJobType newJobType)
    {
        jobType = newJobType;
    }

    public bool CanTakeCraftingAssignment(CivilianJobType requiredType)
    {
        if (targetCraftingBuilding != null && !manualCraftingAssignment)
            return false;

        return requiredType == CivilianJobType.Generalist || jobType == requiredType;
    }

    public void AssignCraftingBuilding(CraftingBuilding building, bool manual = false)
    {
        targetCraftingBuilding = building;
        manualCraftingAssignment = manual;

        if (role == CivilianRole.Idle || role == CivilianRole.Gatherer || role == CivilianRole.Builder || role == CivilianRole.Hauler)
            role = CivilianRole.Crafter;

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

        if (carriedAmount > 0)
        {
            state = State.DeliveringCraftInput;
            return;
        }

        if (targetCraftingBuilding.TryGetInputRequest(transform.position, out carriedType, out int amount, out targetStorage))
        {
            if (targetStorage == null)
            {
                ProductionNotificationManager.Instance?.NotifyIfReady($"missing-storage-{targetCraftingBuilding.GetInstanceID()}", $"{targetCraftingBuilding.name}: no storage with {carriedType} found.");
                return;
            }

            agent.SetDestination(targetStorage.transform.position);
            if (Arrived())
            {
                int took = targetStorage.Withdraw(carriedType, Mathf.Min(amount, GetCarryCapacity()));
                carriedAmount = took;
                state = took > 0 ? State.DeliveringCraftInput : State.FetchingCraftInput;
            }
            return;
        }

        state = State.CollectingCraftOutput;
    }

    void TickDeliverCraftInput()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.GoingToDepositStorage;
            return;
        }

        Vector3 slot = targetCraftingBuilding.inputSlot != null ? targetCraftingBuilding.inputSlot.position : targetCraftingBuilding.transform.position;
        agent.SetDestination(slot);

        if (!Arrived()) return;

        if (carriedAmount <= 0)
        {
            state = State.FetchingCraftInput;
            return;
        }

        targetCraftingBuilding.TryDeliverInput(carriedType, carriedAmount, out int accepted);
        carriedAmount -= accepted;

        if (carriedAmount > 0)
            state = State.GoingToDepositStorage;
        else
            state = State.GoingToWorkPoint;
    }

    void TickGoWorkPoint()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (!targetCraftingBuilding.TryReserveWorkPoint(this, out targetWorkPoint) || targetWorkPoint == null)
            return;

        agent.SetDestination(targetWorkPoint.position);
        if (Arrived())
            state = State.CraftingAtWorkPoint;
    }

    void TickCraftAtWorkPoint()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForInputs)
            state = State.FetchingCraftInput;
        else if (targetCraftingBuilding.State == CraftingBuilding.ProductionState.OutputReady || targetCraftingBuilding.State == CraftingBuilding.ProductionState.WaitingForPickup)
            state = State.CollectingCraftOutput;
    }

    void TickCollectCraftOutput()
    {
        if (targetCraftingBuilding == null)
        {
            state = State.Idle;
            return;
        }

        if (!targetCraftingBuilding.TryGetOutputRequest(transform.position, out carriedType, out int amount, out targetStorage))
        {
            state = State.FetchingCraftInput;
            return;
        }

        Vector3 slot = targetCraftingBuilding.outputSlot != null ? targetCraftingBuilding.outputSlot.position : targetCraftingBuilding.transform.position;
        agent.SetDestination(slot);
        if (!Arrived()) return;

        carriedAmount = targetCraftingBuilding.CollectOutput(carriedType, Mathf.Min(amount, GetCarryCapacity()));
        state = carriedAmount > 0 ? State.DeliveringCraftOutput : State.FetchingCraftInput;
    }

    void TickDeliverCraftOutput()
    {
        if (carriedAmount <= 0)
        {
            state = State.FetchingCraftInput;
            return;
        }

        if (targetStorage == null)
            targetStorage = TeamStorageManager.Instance?.FindNearestStorageCached(teamID, carriedType, transform.position, true);

        if (targetStorage == null)
        {
            ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{teamID}-{carriedType}", $"No free storage for {carriedType}. Production stalled.");
            return;
        }

        agent.SetDestination(targetStorage.transform.position);
        if (!Arrived()) return;

        int accepted = targetStorage.Deposit(carriedType, carriedAmount);
        carriedAmount -= accepted;

        if (carriedAmount > 0)
            ProductionNotificationManager.Instance?.NotifyIfReady($"full-output-{targetStorage.GetInstanceID()}-{carriedType}", $"Storage full for {carriedType}. Output queue blocked.");

        if (carriedAmount > 0)
            state = State.DeliveringCraftOutput;
        else
            state = State.FetchingCraftInput;
    }

    float GetMovementSpeedMultiplier()
    {
        if (!useRoadBonus)
            return 1f;

        Vector3 probe = transform.position + Vector3.up * 0.2f;
        if (Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 2f))
        {
            if (hit.collider != null && hit.collider.CompareTag("Road"))
                return Mathf.Max(1f, roadSpeedMultiplier);
        }

        return 1f;
    }

    string BuildTaskLabel()
    {
        switch (state)
        {
            case State.FetchingCraftInput: return "Fetching crafting inputs";
            case State.DeliveringCraftInput: return "Delivering crafting inputs";
            case State.GoingToWorkPoint: return "Moving to work point";
            case State.CraftingAtWorkPoint: return "Operating work point";
            case State.CollectingCraftOutput: return "Collecting outputs";
            case State.DeliveringCraftOutput: return "Delivering crafted goods";
            default: return state.ToString();
        }
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            if (agent != null) agent.isStopped = true;
            Destroy(gameObject);
        }
    }

    // If your ITargetable interface expects something like this, you can keep it:
    public Transform GetTransform()
    {
        return transform;
    }
}
