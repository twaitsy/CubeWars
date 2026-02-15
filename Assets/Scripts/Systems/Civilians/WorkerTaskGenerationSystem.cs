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
        ScanGathering();
        ScanBuildingAndHauling();
        ScanCrafting();
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

            WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Build(site.teamID, site));
            if (!site.MaterialsComplete)
                WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Haul(site.teamID, site));
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
            if (!building.NeedsAnyInput() && !building.HasAnyOutputQueued())
                continue;

            WorkerTaskDispatcher.Instance.QueueTask(WorkerTaskRequest.Craft(building.teamID, building));
        }
    }
}
