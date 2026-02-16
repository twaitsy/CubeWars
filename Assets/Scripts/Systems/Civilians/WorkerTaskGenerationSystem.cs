using UnityEngine;

public class WorkerTaskGenerationSystem : MonoBehaviour
{
    [Min(0.1f)] public float scanTickSeconds = 1f;
    float timer;

    void Update()
    {
        if (WorkerTaskDispatcher.Instance == null)
            return;

        timer += Time.deltaTime;
        if (timer < scanTickSeconds)
            return;

        timer = 0f;
        ScanBuildingAndHauling();
        ScanCrafting();
        ScanGathering();
    }

    void ScanGathering()
    {
        ResourceNode[] nodes = FindObjectsOfType<ResourceNode>();
        for (int i = 0; i < nodes.Length; i++)
        {
            ResourceNode node = nodes[i];
            if (node == null || node.amount <= 0)
                continue;

            int maxSlots = Mathf.Max(1, node.maxGatherers);
            int openSlots = Mathf.Max(0, maxSlots - node.ActiveGatherers);
            int queuedSlots = WorkerTaskDispatcher.Instance.GetQueuedGatherTaskCount(node);
            int tasksToQueue = Mathf.Max(0, openSlots - queuedSlots);

            for (int slot = 0; slot < tasksToQueue; slot++)
                WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Gather(-1, node));
        }
    }

    void ScanBuildingAndHauling()
    {
        ConstructionSite[] sites = FindObjectsOfType<ConstructionSite>();
        for (int i = 0; i < sites.Length; i++)
        {
            ConstructionSite site = sites[i];
            if (site == null || site.IsComplete)
                continue;

            int maxBuilders = Mathf.Max(1, WorkerTaskDispatcher.Instance.GetRegisteredWorkerCount(site.teamID));
            int currentBuilders = site.AssignedBuilderCount;
            int queuedBuild = WorkerTaskDispatcher.Instance.GetQueuedBuildTaskCount(site, site.teamID);
            int buildTasksToQueue = Mathf.Max(0, maxBuilders - currentBuilders - queuedBuild);

            for (int b = 0; b < buildTasksToQueue; b++)
                WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Build(site.teamID, site));

            if (!site.MaterialsComplete)
            {
                int queuedHaul = WorkerTaskDispatcher.Instance.GetQueuedHaulTaskCount(site, site.teamID);
                int haulTasksToQueue = Mathf.Max(0, maxBuilders - queuedHaul);
                for (int h = 0; h < haulTasksToQueue; h++)
                    WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Haul(site.teamID, site));
            }
        }
    }

    void ScanCrafting()
    {
        CraftingBuilding[] buildings = FindObjectsOfType<CraftingBuilding>();
        for (int i = 0; i < buildings.Length; i++)
        {
            CraftingBuilding building = buildings[i];
            if (building == null || !building.isActiveAndEnabled)
                continue;
            bool needsWorker = building.State == CraftingBuilding.ProductionState.InputsReady;
            if (!building.NeedsAnyInput() && !building.HasAnyOutputQueued() && !needsWorker)
                continue;

            int maxWorkers = Mathf.Max(1, building.GetMaxWorkers());
            int activeWorkers = building.AssignedWorkers.Count;
            int queuedCraft = WorkerTaskDispatcher.Instance.GetQueuedCraftTaskCount(building, building.teamID);
            int missingWorkers = Mathf.Max(0, maxWorkers - activeWorkers - queuedCraft);

            for (int w = 0; w < missingWorkers; w++)
                WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Craft(building.teamID, building));
        }
    }
}
