using UnityEngine;

public struct WorkerTaskRequest
{
    public WorkerTaskType taskType;
    public WorkerCapability requiredCapability;
    public int teamID;
    public ResourceNode resourceNode;
    public ConstructionSite constructionSite;
    public CraftingBuilding craftingBuilding;
    public CivilianJobType requiredCraftJobType;

    public static WorkerTaskRequest Gather(int teamID, ResourceNode node)
    {
        return new WorkerTaskRequest
        {
            taskType = WorkerTaskType.Gather,
            requiredCapability = WorkerCapability.Gather,
            teamID = teamID,
            resourceNode = node
        };
    }

    public static WorkerTaskRequest Build(int teamID, ConstructionSite site)
    {
        return new WorkerTaskRequest
        {
            taskType = WorkerTaskType.Build,
            requiredCapability = WorkerCapability.Build,
            teamID = teamID,
            constructionSite = site
        };
    }

    public static WorkerTaskRequest Haul(int teamID, ConstructionSite site)
    {
        return new WorkerTaskRequest
        {
            taskType = WorkerTaskType.Haul,
            requiredCapability = WorkerCapability.Haul,
            teamID = teamID,
            constructionSite = site
        };
    }

    public static WorkerTaskRequest Craft(int teamID, CraftingBuilding building)
    {
        return new WorkerTaskRequest
        {
            taskType = WorkerTaskType.Craft,
            requiredCapability = WorkerCapability.Craft,
            teamID = teamID,
            craftingBuilding = building,
            requiredCraftJobType = building != null && building.recipe != null
                ? building.recipe.requiredJobType
                : CivilianJobType.Generalist
        };
    }
}
