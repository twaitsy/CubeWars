using UnityEngine;

public class WorkerTaskGenerationSystem : MonoBehaviour
{
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

        bool hasCraftWork = building.NeedsAnyInput() || building.HasAnyOutputQueued() || needsWorker;

        if (!hasCraftWork)
        {
            Dispatcher.RemoveQueuedCraftTasks(building);
            return;
        }

        int workPointCapacity = Mathf.Max(1, building.GetWorkPointCapacity());
        int productionWorkerCapacity = Mathf.Min(Mathf.Max(1, building.GetMaxWorkers()), workPointCapacity);
        int maxHaulers = building.requireHaulerLogistics ? Mathf.Max(0, building.maxAssignedHaulers) : 0;

        if (!building.requireHaulerLogistics)
            Dispatcher.RemoveQueuedCraftTasks(building, CivilianJobType.Hauler);

        int activeProductionWorkers = 0;
        int activeHaulers = 0;
        for (int i = 0; i < building.AssignedWorkers.Count; i++)
        {
            Civilian worker = building.AssignedWorkers[i];
            if (worker == null)
                continue;

            if (worker.JobType == CivilianJobType.Hauler)
            {
                activeHaulers++;
                continue;
            }

            activeProductionWorkers++;
        }

        int queuedProductionWorkers = 0;
        int queuedHaulers = 0;

        var queuedTasks = Dispatcher.GetQueuedTasksSnapshot(building.teamID);
        for (int i = 0; i < queuedTasks.Count; i++)
        {
            WorkerTaskRequest task = queuedTasks[i];
            if (task.taskType != WorkerTaskType.Craft || task.craftingBuilding != building)
                continue;

            if (task.requiredCraftJobType == CivilianJobType.Hauler)
                queuedHaulers++;
            else
                queuedProductionWorkers++;
        }

        int missingWorkers = Mathf.Max(0, productionWorkerCapacity - activeProductionWorkers - queuedProductionWorkers);
        for (int i = 0; i < missingWorkers; i++)
            Dispatcher.QueueTask(WorkerTaskRequest.Craft(building.teamID, building, building.recipe != null ? building.recipe.requiredJobType : CivilianJobType.Generalist));

        int missingHaulers = Mathf.Max(0, maxHaulers - activeHaulers - queuedHaulers);
        for (int i = 0; i < missingHaulers; i++)
            Dispatcher.QueueTask(WorkerTaskRequest.Craft(building.teamID, building, CivilianJobType.Hauler));
    }
}
