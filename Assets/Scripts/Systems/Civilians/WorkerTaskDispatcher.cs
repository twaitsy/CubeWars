using System.Collections.Generic;
using UnityEngine;

public class WorkerTaskDispatcher : MonoBehaviour
{
    public static WorkerTaskDispatcher Instance;

    [Min(0.1f)] public float dispatchTickSeconds = 0.4f;

    readonly List<Civilian> workers = new List<Civilian>();
    readonly List<WorkerTaskRequest> queuedTasks = new List<WorkerTaskRequest>();
    float dispatchTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        dispatchTimer += Time.deltaTime;
        if (dispatchTimer < dispatchTickSeconds)
            return;

        dispatchTimer = 0f;
        DispatchQueuedTasks();
    }

    public void RegisterWorker(Civilian worker)
    {
        if (worker == null || workers.Contains(worker))
            return;

        workers.Add(worker);
    }

    public void UnregisterWorker(Civilian worker)
    {
        if (worker == null)
            return;

        workers.Remove(worker);
    }

    public void QueueTask(WorkerTaskRequest task)
    {
        queuedTasks.Add(task);
    }

    public int GetQueuedGatherTaskCount(ResourceNode node, int teamID = -1)
    {
        if (node == null)
            return 0;

        int count = 0;
        for (int i = 0; i < queuedTasks.Count; i++)
        {
            WorkerTaskRequest task = queuedTasks[i];
            if (task.taskType != WorkerTaskType.Gather)
                continue;
            if (task.resourceNode != node)
                continue;
            if (teamID >= 0 && task.teamID != teamID)
                continue;

            count++;
        }

        return count;
    }


    public int GetQueuedTaskCount(int teamID = -1)
    {
        if (teamID < 0)
            return queuedTasks.Count;

        int count = 0;
        for (int i = 0; i < queuedTasks.Count; i++)
            if (queuedTasks[i].teamID == teamID)
                count++;

        return count;
    }

    public int GetRegisteredWorkerCount(int teamID = -1)
    {
        if (teamID < 0)
            return workers.Count;

        int count = 0;
        for (int i = 0; i < workers.Count; i++)
        {
            Civilian worker = workers[i];
            if (worker != null && worker.teamID == teamID)
                count++;
        }

        return count;
    }
    public bool TryAssignTaskToWorker(Civilian worker, WorkerTaskRequest task)
    {
        if (worker == null || (task.teamID >= 0 && worker.teamID != task.teamID))
            return false;

        return worker.TryAssignTask(task);
    }

    void DispatchQueuedTasks()
    {
        for (int i = queuedTasks.Count - 1; i >= 0; i--)
        {
            WorkerTaskRequest task = queuedTasks[i];
            Civilian worker = FindBestWorker(task);
            if (worker == null)
                continue;

            if (!worker.TryAssignTask(task))
                continue;

            queuedTasks.RemoveAt(i);
        }
    }

    Civilian FindBestWorker(WorkerTaskRequest task)
    {
        Civilian best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < workers.Count; i++)
        {
            Civilian worker = workers[i];
            if (worker == null || (task.teamID >= 0 && worker.teamID != task.teamID))
                continue;
            if (!IsWorkerAvailable(worker))
                continue;
            if (!worker.CanPerform(task.requiredCapability))
                continue;

            float distance = (worker.transform.position - ResolveTaskPosition(task)).sqrMagnitude;
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            best = worker;
        }

        return best;
    }

    static bool IsWorkerAvailable(Civilian worker)
    {
        string state = worker.CurrentState;
        return state == "Idle" || state.StartsWith("Searching");
    }

    static Vector3 ResolveTaskPosition(WorkerTaskRequest task)
    {
        if (task.resourceNode != null) return task.resourceNode.transform.position;
        if (task.constructionSite != null) return task.constructionSite.transform.position;
        if (task.craftingBuilding != null) return task.craftingBuilding.transform.position;
        return Vector3.zero;
    }
}
