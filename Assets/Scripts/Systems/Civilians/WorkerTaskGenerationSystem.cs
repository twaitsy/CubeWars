using UnityEngine;

public class WorkerTaskGenerationSystem : MonoBehaviour
{
    [Min(0.1f)] public float scanTickSeconds = 1f;
    float timer;

    void OnEnable()
    {
        // --- Gathering events ---
        ResourceRegistry.OnNodeRegistered += HandleNodeChanged;
        ResourceRegistry.OnNodeChanged += HandleNodeChanged;
        ResourceRegistry.OnNodeDepleted += HandleNodeChanged;

        // --- Construction events ---
        ConstructionRegistry.OnSiteRegistered += HandleSiteChanged;
        ConstructionRegistry.OnSiteChanged += HandleSiteChanged;
        ConstructionRegistry.OnSiteCompleted += HandleSiteChanged;

        // --- Crafting events ---
        CraftingRegistry.OnBuildingRegistered += HandleCraftingChanged;
        CraftingRegistry.OnBuildingChanged += HandleCraftingChanged;
        CraftingRegistry.OnBuildingCompleted += HandleCraftingChanged;
    }

    void OnDisable()
    {
        // --- Gathering events ---
        ResourceRegistry.OnNodeRegistered -= HandleNodeChanged;
        ResourceRegistry.OnNodeChanged -= HandleNodeChanged;
        ResourceRegistry.OnNodeDepleted -= HandleNodeChanged;

        // --- Construction events ---
        ConstructionRegistry.OnSiteRegistered -= HandleSiteChanged;
        ConstructionRegistry.OnSiteChanged -= HandleSiteChanged;
        ConstructionRegistry.OnSiteCompleted -= HandleSiteChanged;

        // --- Crafting events ---
        CraftingRegistry.OnBuildingRegistered -= HandleCraftingChanged;
        CraftingRegistry.OnBuildingChanged -= HandleCraftingChanged;
        CraftingRegistry.OnBuildingCompleted -= HandleCraftingChanged;
    }

    void Update()
    {
        // Dispatcher must exist
        if (WorkerTaskDispatcher.Instance == null)
            return;

        // Timer is no longer used for scanning, but keep it in case you add timed logic later
        timer += Time.deltaTime;
        if (timer < scanTickSeconds)
            return;

        timer = 0f;
    }

    // ========================================================================
    // EVENT-DRIVEN GATHERING
    // ========================================================================

    void HandleNodeChanged(ResourceNode node)
    {
        if (node == null || node.Amount <= 0)
            return;

        int maxSlots = Mathf.Max(1, node.maxGatherers);
        int openSlots = Mathf.Max(0, maxSlots - node.ActiveGatherers);
        int queuedSlots = WorkerTaskDispatcher.Instance.GetQueuedGatherTaskCount(node);
        int tasksToQueue = Mathf.Max(0, openSlots - queuedSlots);

        for (int i = 0; i < tasksToQueue; i++)
            WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Gather(-1, node));
    }

    // ========================================================================
    // EVENT-DRIVEN CONSTRUCTION (BUILDING + HAULING)
    // ========================================================================

    void HandleSiteChanged(ConstructionSite site)
    {
        if (site == null || site.IsComplete)
            return;

        int teamID = site.teamID;

        // --- BUILD TASKS ----------------------------------------------------
        int maxBuilders = Mathf.Max(1, WorkerTaskDispatcher.Instance.GetRegisteredWorkerCount(teamID));
        int currentBuilders = site.AssignedBuilderCount;
        int queuedBuild = WorkerTaskDispatcher.Instance.GetQueuedBuildTaskCount(site, teamID);
        int buildTasksToQueue = Mathf.Max(0, maxBuilders - currentBuilders - queuedBuild);

        for (int i = 0; i < buildTasksToQueue; i++)
            WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Build(teamID, site));

        // --- HAUL TASKS -----------------------------------------------------
        if (!site.MaterialsComplete)
        {
            int queuedHaul = WorkerTaskDispatcher.Instance.GetQueuedHaulTaskCount(site, teamID);
            int haulTasksToQueue = Mathf.Max(0, maxBuilders - queuedHaul);

            for (int i = 0; i < haulTasksToQueue; i++)
                WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Haul(teamID, site));
        }
    }

    // ========================================================================
    // EVENT-DRIVEN CRAFTING
    // ========================================================================

    void HandleCraftingChanged(CraftingBuilding building)
    {
        if (building == null || !building.isActiveAndEnabled)
            return;

        bool needsWorker =
            building.State == CraftingBuilding.ProductionState.InputsReady ||
            building.State == CraftingBuilding.ProductionState.InProgress;

        // If nothing is needed, skip
        if (!building.NeedsAnyInput() &&
            !building.HasAnyOutputQueued() &&
            !needsWorker)
            return;

        int maxWorkers = Mathf.Max(1, building.GetMaxWorkers());
        int activeWorkers = building.AssignedWorkers.Count;
        int queuedCraft = WorkerTaskDispatcher.Instance.GetQueuedCraftTaskCount(building, building.teamID);
        int missingWorkers = Mathf.Max(0, maxWorkers - activeWorkers - queuedCraft);

        for (int w = 0; w < missingWorkers; w++)
            WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Craft(building.teamID, building));
    }
}