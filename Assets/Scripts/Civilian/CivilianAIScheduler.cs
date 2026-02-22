using System.Collections.Generic;
using UnityEngine;

public class CivilianAIScheduler : MonoBehaviour
{
    public static CivilianAIScheduler Instance { get; private set; }

    [Header("Tick Rates")]
    [Min(1f)] public float aiTicksPerSecond = 10f;
    [Min(0.1f)] public float needsTickSeconds = 1f;

    [Header("Batching")]
    [Min(1)] public int maxAIBatchSize = 64;

    [Header("LOD Distances")]
    [Min(1f)] public float fullSimulationDistance = 60f;
    [Min(1f)] public float reducedSimulationDistance = 140f;

    readonly List<Civilian> civilians = new();
    int nextIndex;
    float aiTimer;
    float needsTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Register(Civilian civilian)
    {
        if (civilian == null || civilians.Contains(civilian))
            return;

        civilians.Add(civilian);
    }

    public void Unregister(Civilian civilian)
    {
        if (civilian == null)
            return;

        civilians.Remove(civilian);
        if (nextIndex >= civilians.Count)
            nextIndex = 0;
    }

    void Update()
    {
        for (int i = civilians.Count - 1; i >= 0; i--)
        {
            Civilian civ = civilians[i];
            if (civ == null)
                civilians.RemoveAt(i);
        }

        float dt = Time.deltaTime;
        aiTimer += dt;
        needsTimer += dt;

        bool tickNeeds = needsTimer >= needsTickSeconds;
        if (tickNeeds)
            needsTimer = 0f;

        float aiInterval = 1f / Mathf.Max(1f, aiTicksPerSecond);
        if (aiTimer < aiInterval)
        {
            TickMovementAll(dt);
            return;
        }

        aiTimer = 0f;
        int batchCount = Mathf.Min(maxAIBatchSize, civilians.Count);
        for (int i = 0; i < batchCount; i++)
        {
            if (civilians.Count == 0)
                break;

            if (nextIndex >= civilians.Count)
                nextIndex = 0;

            Civilian civ = civilians[nextIndex++];
            if (civ == null)
                continue;

            CivilianAILodTier lod = ResolveLodTier(civ);
            civ.SchedulerTickAI(dt, tickNeeds, lod);
        }

        TickMovementAll(dt);
    }

    void TickMovementAll(float dt)
    {
        for (int i = 0; i < civilians.Count; i++)
            civilians[i]?.SchedulerTickMovement(dt);
    }

    CivilianAILodTier ResolveLodTier(Civilian civ)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return CivilianAILodTier.Full;

        float sqrDistance = (cam.transform.position - civ.transform.position).sqrMagnitude;
        if (sqrDistance <= fullSimulationDistance * fullSimulationDistance)
            return CivilianAILodTier.Full;
        if (sqrDistance <= reducedSimulationDistance * reducedSimulationDistance)
            return CivilianAILodTier.Reduced;
        return CivilianAILodTier.Coarse;
    }
}

public enum CivilianAILodTier
{
    Full,
    Reduced,
    Coarse
}
