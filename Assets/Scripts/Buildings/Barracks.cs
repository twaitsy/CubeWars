using UnityEngine;

public class Barracks : Building
{
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

    void OnDestroy()
    {
        if (queue != null)
            queue.OnUnitCompleted -= SpawnUnit;
    }

    void SpawnUnit(UnitProductionDefinition def)
    {
        if (def == null || def.unitPrefab == null)
            return;

        GameObject go = Instantiate(def.unitPrefab, transform.position + transform.forward * 2f, Quaternion.identity);
        TeamAssignmentUtility.ApplyTeamToHierarchy(go, teamID);
    }

    public bool CanQueue(UnitProductionDefinition def)
    {
        if (def == null || TeamResources.Instance == null)
            return false;

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
