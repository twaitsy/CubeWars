using UnityEngine;

public class WorkerTaskGenerationSystem : MonoBehaviour
{
    [Min(0.1f)] public float scanTickSeconds = 1f;
    private float timer;

    private WorkerTaskDispatcher Dispatcher => WorkerTaskDispatcher.Instance;

    void OnEnable()
    {
        // Gathering
        ResourceRegistry.OnNodeRegistered += HandleNodeChanged;
        ResourceRegistry.OnNodeChanged += HandleNodeChanged;
        ResourceRegistry.OnNodeDepleted += HandleNodeChanged;

        // Construction
        ConstructionRegistry.OnSiteRegistered += HandleSiteChanged;
        ConstructionRegistry.OnSiteChanged += HandleSiteChanged;
        ConstructionRegistry.OnSiteCompleted += HandleSiteChanged;

        // Crafting
        CraftingRegistry.OnBuildingRegistered += HandleCraftingChanged;
        CraftingRegistry.OnBuildingChanged += HandleCraftingChanged;
        CraftingRegistry.OnBuildingCompleted += HandleCraftingChanged;
    }

    void OnDisable()
    {
        // Gathering
        ResourceRegistry.OnNodeRegistered -= HandleNodeChanged;
        ResourceRegistry.OnNodeChanged -= HandleNodeChanged;
        ResourceRegistry.OnNodeDepleted -= HandleNodeChanged;

        // Construction
        ConstructionRegistry.OnSiteRegistered -= HandleSiteChanged;
        ConstructionRegistry.OnSiteChanged -= HandleSiteChanged;
        ConstructionRegistry.OnSiteCompleted -= HandleSiteChanged;

        // Crafting
        CraftingRegistry.OnBuildingRegistered -= HandleCraftingChanged;
        CraftingRegistry.OnBuildingChanged -= HandleCraftingChanged;
        CraftingRegistry.OnBuildingCompleted -= HandleCraftingChanged;
    }

    void Update()
    {
        // Dispatcher must exist
        if (Dispatcher == null)
            return;

        // Timer is unused but kept for future timed logic
        timer += Time.deltaTime;
        if (timer < scanTickSeconds)
            return;

        timer = 0f;
    }

    // ========================================================================
    // EVENT-DRIVEN GATHERING
    // ========================================================================

    private void HandleNodeChanged(ResourceNode node)
    {
        if (Dispatcher == null)
            return;

        if (node == null || node.IsDepleted)
            return;

        int maxSlots = Mathf.Max(1, node.maxGatherers);
        int openSlots = Mathf.Max(0, maxSlots - node.ActiveGatherers);

        int queued = Dispatcher.GetQueuedGatherTaskCount(node);
        int tasksToQueue = Mathf.Max(0, openSlots - queued);

        for (int i = 0; i < tasksToQueue; i++)
            Dispatcher.QueueTask(WorkerTaskRequest.Gather(-1, node));
    }

    // ========================================================================
    // EVENT-DRIVEN CONSTRUCTION
    // ========================================================================

    private void HandleSiteChanged(ConstructionSite site)
    {
        if (Dispatcher == null)
            return;

        if (site == null || site.IsComplete)
            return;

        int teamID = site.teamID;

        // --- BUILD TASKS ----------------------------------------------------
        int maxBuilders = Mathf.Max(1, Dispatcher.GetRegisteredWorkerCount(teamID));
        int currentBuilders = site.AssignedBuilderCount;
        int queuedBuild = Dispatcher.GetQueuedBuildTaskCount(site, teamID);

        int buildTasksToQueue = Mathf.Max(0, maxBuilders - currentBuilders - queuedBuild);

        for (int i = 0; i < buildTasksToQueue; i++)
            Dispatcher.QueueTask(WorkerTaskRequest.Build(teamID, site));

        // --- HAUL TASKS -----------------------------------------------------
        if (!site.MaterialsComplete)
        {
            int queuedHaul = Dispatcher.GetQueuedHaulTaskCount(site, teamID);
            int haulTasksToQueue = Mathf.Max(0, maxBuilders - queuedHaul);

            for (int i = 0; i < haulTasksToQueue; i++)
                Dispatcher.QueueTask(WorkerTaskRequest.Haul(teamID, site));
        }
    }

    // ========================================================================
    // EVENT-DRIVEN CRAFTING
    // ========================================================================

    private void HandleCraftingChanged(CraftingBuilding building)
    {
        if (Dispatcher == null)
            return;

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
        int queuedCraft = Dispatcher.GetQueuedCraftTaskCount(building, building.teamID);

        int missingWorkers = Mathf.Max(0, maxWorkers - activeWorkers - queuedCraft);

        for (int w = 0; w < missingWorkers; w++)
            Dispatcher.QueueTask(WorkerTaskRequest.Craft(building.teamID, building));
    }
}