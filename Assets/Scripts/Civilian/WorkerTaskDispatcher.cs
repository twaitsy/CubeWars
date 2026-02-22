using System.Collections.Generic;
using UnityEngine;

public class WorkerTaskDispatcher : MonoBehaviour
{
    public static WorkerTaskDispatcher Instance;

    readonly HashSet<Civilian> workers = new();
    readonly List<WorkerTaskRequest> queuedTasks = new();
    readonly Dictionary<int, TeamTaskQueues> teamQueues = new();

    sealed class TeamTaskQueues
    {
        public readonly Dictionary<WorkerTaskType, Queue<int>> indicesByType = new();

        public Queue<int> GetQueue(WorkerTaskType type)
        {
            if (!indicesByType.TryGetValue(type, out Queue<int> queue))
            {
                queue = new Queue<int>();
                indicesByType[type] = queue;
            }

            return queue;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterWorker(Civilian worker)
    {
        if (worker == null)
            return;

        workers.Add(worker);
    }

    public void UnregisterWorker(Civilian worker)
    {
        if (worker == null)
            return;

        workers.Remove(worker);
    }

    public bool TryAssignAnyTask(Civilian worker)
    {
        if (worker == null || !workers.Contains(worker))
            return false;

        TeamTaskQueues queues = GetTeamQueues(worker.teamID);
        if (TryAssignFromQueue(worker, queues.GetQueue(WorkerTaskType.Craft))) return true;
        if (TryAssignFromQueue(worker, queues.GetQueue(WorkerTaskType.Haul))) return true;
        if (TryAssignFromQueue(worker, queues.GetQueue(WorkerTaskType.Build))) return true;
        if (TryAssignFromQueue(worker, queues.GetQueue(WorkerTaskType.Gather))) return true;

        // Global tasks can be assigned to any team.
        TeamTaskQueues globalQueues = GetTeamQueues(-1);
        if (TryAssignFromQueue(worker, globalQueues.GetQueue(WorkerTaskType.Craft))) return true;
        if (TryAssignFromQueue(worker, globalQueues.GetQueue(WorkerTaskType.Haul))) return true;
        if (TryAssignFromQueue(worker, globalQueues.GetQueue(WorkerTaskType.Build))) return true;
        if (TryAssignFromQueue(worker, globalQueues.GetQueue(WorkerTaskType.Gather))) return true;

        return false;
    }

    bool TryAssignFromQueue(Civilian worker, Queue<int> queue)
    {
        int attempts = queue.Count;
        for (int i = 0; i < attempts; i++)
        {
            int index = queue.Dequeue();
            if (index < 0 || index >= queuedTasks.Count)
                continue;

            WorkerTaskRequest task = queuedTasks[index];
            if (!IsTaskStillValid(task))
                continue;

            bool canPerform = task.taskType == WorkerTaskType.Craft && task.requiredCraftJobType == CivilianJobType.Hauler
                ? worker.CanPerform(WorkerCapability.Haul)
                : worker.CanPerform(task.requiredCapability);

            if (!canPerform)
            {
                queue.Enqueue(index);
                continue;
            }

            if (!worker.TryAssignTask(task))
            {
                queue.Enqueue(index);
                continue;
            }

            queuedTasks[index] = default;
            return true;
        }

        return false;
    }

    public void QueueTask(WorkerTaskRequest task)
    {
        int index = queuedTasks.Count;
        queuedTasks.Add(task);
        GetTeamQueues(task.teamID).GetQueue(task.taskType).Enqueue(index);
    }

    TeamTaskQueues GetTeamQueues(int teamID)
    {
        if (!teamQueues.TryGetValue(teamID, out TeamTaskQueues queues))
        {
            queues = new TeamTaskQueues();
            teamQueues[teamID] = queues;
        }

        return queues;
    }

    public void RemoveQueuedCraftTasks(CraftingBuilding building, CivilianJobType requiredJobType = CivilianJobType.Generalist)
    {
        if (building == null)
            return;

        for (int i = 0; i < queuedTasks.Count; i++)
        {
            WorkerTaskRequest task = queuedTasks[i];
            if (task.taskType != WorkerTaskType.Craft)
                continue;

            if (task.craftingBuilding != building)
                continue;

            if (requiredJobType != CivilianJobType.Generalist && task.requiredCraftJobType != requiredJobType)
                continue;

            queuedTasks[i] = default;
        }
    }

    public int GetQueuedGatherTaskCount(ResourceNode node, int teamID = -1) => CountQueued(task => task.taskType == WorkerTaskType.Gather && task.resourceNode == node && (teamID < 0 || task.teamID == teamID));
    public int GetQueuedBuildTaskCount(ConstructionSite site, int teamID = -1) => CountQueued(task => task.taskType == WorkerTaskType.Build && task.constructionSite == site && (teamID < 0 || task.teamID == teamID));
    public int GetQueuedHaulTaskCount(ConstructionSite site, int teamID = -1) => CountQueued(task => task.taskType == WorkerTaskType.Haul && task.constructionSite == site && (teamID < 0 || task.teamID == teamID));
    public int GetQueuedCraftTaskCount(CraftingBuilding building, int teamID = -1) => CountQueued(task => task.taskType == WorkerTaskType.Craft && task.craftingBuilding == building && (teamID < 0 || task.teamID == teamID));
    public int GetQueuedTaskCount(int teamID = -1) => CountQueued(task => teamID < 0 || task.teamID == teamID);

    public int GetQueuedTaskCountByType(WorkerTaskType taskType, int teamID = -1)
    {
        return CountQueued(task => task.taskType == taskType && (teamID < 0 || task.teamID == teamID));
    }

    int CountQueued(System.Predicate<WorkerTaskRequest> predicate)
    {
        int count = 0;
        for (int i = 0; i < queuedTasks.Count; i++)
        {
            WorkerTaskRequest task = queuedTasks[i];
            if (task.taskType == 0 && task.requiredCapability == 0 && task.teamID == 0 && task.resourceNode == null && task.constructionSite == null && task.craftingBuilding == null)
                continue;

            if (predicate(task))
                count++;
        }

        return count;
    }

    public int GetRegisteredWorkerCount(int teamID = -1)
    {
        if (teamID < 0)
            return workers.Count;

        int count = 0;
        foreach (Civilian worker in workers)
            if (worker != null && worker.teamID == teamID)
                count++;

        return count;
    }

    public List<WorkerTaskRequest> GetQueuedTasksSnapshot(int teamID = -1)
    {
        var snapshot = new List<WorkerTaskRequest>();

        for (int i = 0; i < queuedTasks.Count; i++)
        {
            WorkerTaskRequest task = queuedTasks[i];
            if (task.resourceNode == null && task.constructionSite == null && task.craftingBuilding == null)
                continue;
            if (teamID >= 0 && task.teamID != teamID)
                continue;

            snapshot.Add(task);
        }

        return snapshot;
    }

    public List<Civilian> GetRegisteredWorkersSnapshot(int teamID = -1)
    {
        var snapshot = new List<Civilian>();

        foreach (Civilian worker in workers)
        {
            if (worker == null)
                continue;
            if (teamID >= 0 && worker.teamID != teamID)
                continue;

            snapshot.Add(worker);
        }

        return snapshot;
    }

    public bool TryAssignTaskToWorker(Civilian worker, WorkerTaskRequest task)
    {
        if (worker == null || (task.teamID >= 0 && worker.teamID != task.teamID))
            return false;

        return worker.TryAssignTask(task);
    }

    static bool IsTaskStillValid(WorkerTaskRequest task)
    {
        switch (task.taskType)
        {
            case WorkerTaskType.Gather:
                return task.resourceNode != null && !task.resourceNode.IsDepleted;
            case WorkerTaskType.Build:
                return task.constructionSite != null && !task.constructionSite.IsComplete;
            case WorkerTaskType.Haul:
                return task.constructionSite != null && !task.constructionSite.IsComplete && !task.constructionSite.MaterialsComplete;
            case WorkerTaskType.Craft:
                if (task.craftingBuilding == null || !task.craftingBuilding.isActiveAndEnabled)
                    return false;

                if (task.requiredCraftJobType == CivilianJobType.Hauler)
                    return task.craftingBuilding.requireHaulerLogistics
                        && (task.craftingBuilding.NeedsAnyInput() || task.craftingBuilding.HasAnyOutputQueued());

                bool needsWorker = task.craftingBuilding.State == CraftingBuilding.ProductionState.InputsReady
                    || task.craftingBuilding.State == CraftingBuilding.ProductionState.InProgress;

                return needsWorker || task.craftingBuilding.NeedsAnyInput() || task.craftingBuilding.HasAnyOutputQueued();
            default:
                return false;
        }
    }
}
