using UnityEngine;

[DisallowMultipleComponent]
public class TaskBoard : MonoBehaviour
{
    public static TaskBoard Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Publish(WorkerTaskRequest task)
    {
        WorkerTaskDispatcher.Instance?.QueueTask(task);
    }

    public bool TryAssign(Civilian civilian)
    {
        return WorkerTaskDispatcher.Instance != null && WorkerTaskDispatcher.Instance.TryAssignAnyTask(civilian);
    }
}
