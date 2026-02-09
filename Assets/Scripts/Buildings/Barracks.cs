// ============================================================================
// Barracks.cs
//
// PURPOSE:
// - Produces units over time using UnitProductionQueue.
// - Acts as a factory for infantry or other unit types.
// - Integrates with UI, AI, and resource systems.
//
// DEPENDENCIES:
// - UnitProductionDefinition:
//      * Defines unit prefab, cost, buildTime.
// - UnitProductionQueue:
//      * Handles timed production.
//      * Barracks subscribes to OnUnitCompleted to spawn units.
// - TeamResources / TeamStorageManager:
//      * Used for CanAfford + Spend before enqueueing.
// - Unit:
//      * Spawned units must have teamID applied.
// - UnitInspectorUI:
//      * Reads:
//          producibleUnits[]
//          CanQueue()
//          QueueUnit()
//          CancelLast()
//          CurrentBuildTime
//          CurrentProgress
//          QueueCount
// - AIEconomy / AIBuilder:
//      * Uses QueueCount, EnqueueUnit(), buildTime proxy.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add new unit types, assign them in producibleUnits.
// - If you add tech upgrades, modify buildTime or costs before enqueueing.
// - If you add rally points, spawn units at rally location instead of forward offset.
// - If you add multi-queue buildings, instantiate multiple UnitProductionQueue components.
// - Keep dependency header updated when related systems change.
//
// INSPECTOR REQUIREMENTS:
// - producibleUnits: list of units this building can produce.
// - queue: auto-added if missing.
// ============================================================================

using UnityEngine;

public class Barracks : MonoBehaviour
{
    [Header("Owner")]
    public int teamID;

    [Header("Producible Units")]
    public UnitProductionDefinition[] producibleUnits;

    [Header("Queue System")]
    public UnitProductionQueue queue;

    void Awake()
    {
        if (queue == null)
            queue = gameObject.AddComponent<UnitProductionQueue>();

        queue.OnUnitCompleted += SpawnUnit;
    }

    void SpawnUnit(UnitProductionDefinition def)
    {
        if (def == null || def.unitPrefab == null)
            return;

        GameObject go = Instantiate(
            def.unitPrefab,
            transform.position + transform.forward * 2f,
            Quaternion.identity
        );

        TeamOwnershipUtility.ApplyTeamToHierarchy(go, teamID);
    }

    // ---------- UI / AI API ----------

    public bool CanQueue(UnitProductionDefinition def)
    {
        if (def == null) return false;
        if (TeamResources.Instance == null) return false;

        return TeamResources.Instance.CanAfford(teamID, def.costs);
    }

    public void QueueUnit(UnitProductionDefinition def)
    {
        if (!CanQueue(def))
            return;

        TeamResources.Instance.Spend(teamID, def.costs);
        queue.Enqueue(def);
    }

    public void EnqueueUnit()
    {
        if (producibleUnits == null || producibleUnits.Length == 0)
            return;

        QueueUnit(producibleUnits[0]);
    }

    public float buildTime =>
        (producibleUnits == null || producibleUnits.Length == 0)
            ? 0f
            : producibleUnits[0].buildTime;

    public void CancelLast() => queue.CancelLast();

    public float CurrentBuildTime => queue.CurrentBuildTime;

    public float CurrentProgress => queue.Progress01;

    public int QueueCount => queue != null ? queue.QueueCount : 0;
}