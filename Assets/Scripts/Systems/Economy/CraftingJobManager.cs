using System.Collections.Generic;
using UnityEngine;

public class CraftingJobManager : MonoBehaviour
{
    public static CraftingJobManager Instance;

    [Min(0.25f)] public float assignmentTickSeconds = 1f;

    readonly List<Civilian> civilians = new List<Civilian>();
    readonly List<CraftingBuilding> buildings = new List<CraftingBuilding>();
    float timer;

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
        timer += Time.deltaTime;
        if (timer < assignmentTickSeconds)
            return;

        timer = 0f;
        AutoAssignWorkers();
    }

    public void RegisterCivilian(Civilian civilian)
    {
        if (civilian == null || civilians.Contains(civilian)) return;
        civilians.Add(civilian);
    }

    public void UnregisterCivilian(Civilian civilian)
    {
        if (civilian == null) return;
        civilians.Remove(civilian);
    }

    public void RegisterBuilding(CraftingBuilding building)
    {
        if (building == null || buildings.Contains(building)) return;
        buildings.Add(building);
    }

    public void UnregisterBuilding(CraftingBuilding building)
    {
        if (building == null) return;
        buildings.Remove(building);
    }

    public bool TryAssignManually(Civilian civilian, CraftingBuilding building)
    {
        if (civilian == null || building == null) return false;
        if (!building.TryAssignWorker(civilian, true)) return false;
        civilian.AssignCraftingBuilding(building, true);
        return true;
    }

    public CraftingBuilding FindNearestBuildingNeedingInput(int teamID, ResourceType type, Vector3 position)
    {
        CraftingBuilding best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < buildings.Count; i++)
        {
            CraftingBuilding building = buildings[i];
            if (building == null) continue;
            if (building.teamID != teamID) continue;
            if (!building.isActiveAndEnabled) continue;
            if (!building.NeedsInput(type)) continue;

            float distance = (building.transform.position - position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = building;
            }
        }

        return best;
    }


    public CraftingBuilding FindNearestBuildingWithInputPriority(int teamID, Vector3 position)
    {
        CraftingBuilding bestInput = null;
        float bestInputDistance = float.MaxValue;

        for (int i = 0; i < buildings.Count; i++)
        {
            CraftingBuilding building = buildings[i];
            if (building == null) continue;
            if (building.teamID != teamID) continue;
            if (!building.isActiveAndEnabled) continue;
            if (!building.NeedsAnyInput()) continue;

            float distance = (building.transform.position - position).sqrMagnitude;
            if (distance < bestInputDistance)
            {
                bestInputDistance = distance;
                bestInput = building;
            }
        }

        if (bestInput != null)
            return bestInput;

        CraftingBuilding bestOutput = null;
        float bestOutputDistance = float.MaxValue;

        for (int i = 0; i < buildings.Count; i++)
        {
            CraftingBuilding building = buildings[i];
            if (building == null) continue;
            if (building.teamID != teamID) continue;
            if (!building.isActiveAndEnabled) continue;
            if (!building.HasAnyOutputQueued()) continue;

            float distance = (building.transform.position - position).sqrMagnitude;
            if (distance < bestOutputDistance)
            {
                bestOutputDistance = distance;
                bestOutput = building;
            }
        }

        return bestOutput;
    }
    void AutoAssignWorkers()
    {
        buildings.Sort((a, b) => b.assignmentPriority.CompareTo(a.assignmentPriority));

        for (int i = 0; i < buildings.Count; i++)
        {
            var building = buildings[i];
            if (building == null || !building.allowAutomaticAssignment)
                continue;

            while (building.AssignedWorkers.Count < building.GetMaxWorkers())
            {
                Civilian candidate = FindBestCandidate(building);
                if (candidate == null) break;

                if (!building.TryAssignWorker(candidate))
                    break;

                candidate.AssignCraftingBuilding(building);
            }
        }
    }

    Civilian FindBestCandidate(CraftingBuilding building)
    {
        bool needsHauler = building.requireHaulerLogistics && !building.HasAssignedHauler();

        Civilian best = FindBestCandidate(building, c => !needsHauler || c.role == CivilianRole.Hauler);
        if (best != null)
            return best;

        return FindBestCandidate(building, c => true);
    }

    Civilian FindBestCandidate(CraftingBuilding building, System.Predicate<Civilian> extraFilter)
    {
        Civilian best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < civilians.Count; i++)
        {
            var civ = civilians[i];
            if (civ == null) continue;
            if (civ.teamID != building.teamID) continue;
            if (!extraFilter(civ)) continue;
            if (!civ.CanTakeCraftingAssignment(building.recipe?.requiredJobType ?? CivilianJobType.Generalist)) continue;

            float d = (civ.transform.position - building.transform.position).sqrMagnitude;
            if (d < bestDistance)
            {
                bestDistance = d;
                best = civ;
            }
        }

        return best;
    }
}
