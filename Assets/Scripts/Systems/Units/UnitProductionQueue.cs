// ============================================================================
// UnitProductionQueue.cs
//
// PURPOSE:
// - Handles timed production of units inside Barracks or other production buildings.
// - Maintains a FIFO queue of production tasks.
// - Notifies Barracks when a unit is finished.
//
// DEPENDENCIES:
// - UnitProductionDefinition:
//      * Provides buildTime, unitPrefab, costs.
// - Barracks:
//      * Owns this component.
//      * Calls Enqueue(), CancelLast().
//      * Subscribes to OnUnitCompleted to spawn units.
// - UnitInspectorUI:
//      * Reads Progress01, CurrentBuildTime, QueueCount.
// - TeamResources / TeamStorageManager:
//      * Costs are spent BEFORE enqueueing (handled by Barracks).
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add production speed modifiers (tech, upgrades, buffs), apply them to timeRemaining.
// - If you add parallel production (multiple queues), split into multiple lanes.
// - If you add UI showing the full queue, expose queue contents safely.
// - If you add unit categories (infantry, vehicles), consider separate queues.
// - If you add cancel refunds, integrate with TeamResources.
//
// INSPECTOR REQUIREMENTS:
// - None; this component is created automatically by Barracks if missing.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

public class UnitProductionQueue : MonoBehaviour
{
    class QueueItem
    {
        public UnitProductionDefinition def;
        public float timeRemaining;
    }

    private readonly List<QueueItem> queue = new List<QueueItem>();
    private QueueItem current;

    public System.Action<UnitProductionDefinition> OnUnitCompleted;

    void Update()
    {
        if (current == null && queue.Count > 0)
            current = queue[0];

        if (current == null) return;

        current.timeRemaining -= Time.deltaTime;

        if (current.timeRemaining <= 0f)
        {
            OnUnitCompleted?.Invoke(current.def);
            queue.RemoveAt(0);
            current = null;
        }
    }

    public void Enqueue(UnitProductionDefinition def)
    {
        if (def == null) return;

        queue.Add(new QueueItem
        {
            def = def,
            timeRemaining = def.buildTime
        });
    }

    public void CancelLast()
    {
        if (queue.Count == 0) return;

        if (queue[0] == current)
            current = null;

        queue.RemoveAt(queue.Count - 1);
    }

    public bool IsBuilding => current != null;

    public float Progress01 =>
        current == null ? 0f : 1f - (current.timeRemaining / current.def.buildTime);

    public float CurrentBuildTime =>
        current == null ? 0f : current.def.buildTime;

    public int QueueCount => queue.Count;
}