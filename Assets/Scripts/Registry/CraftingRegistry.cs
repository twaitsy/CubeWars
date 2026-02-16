using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftingRegistry : MonoBehaviour
{
    public static CraftingRegistry Instance { get; private set; }

    // --- Events -------------------------------------------------------------

    public static event Action<CraftingBuilding> OnBuildingRegistered;
    public static event Action<CraftingBuilding> OnBuildingUnregistered;
    public static event Action<CraftingBuilding> OnBuildingChanged;
    public static event Action<CraftingBuilding> OnBuildingCompleted;

    // --- Data ---------------------------------------------------------------

    readonly HashSet<CraftingBuilding> allBuildings = new();
    readonly Dictionary<int, List<CraftingBuilding>> buildingsByTeam = new();

    // --- Lifecycle ----------------------------------------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CraftingRegistry instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Registration -------------------------------------------------------

    public void Register(CraftingBuilding building)
    {
        if (building == null)
            return;

        if (!allBuildings.Add(building))
            return;

        if (!buildingsByTeam.TryGetValue(building.teamID, out var list))
        {
            list = new List<CraftingBuilding>();
            buildingsByTeam[building.teamID] = list;
        }

        list.Add(building);

        OnBuildingRegistered?.Invoke(building);
    }

    public void Unregister(CraftingBuilding building)
    {
        if (building == null)
            return;

        if (allBuildings.Remove(building))
        {
            if (buildingsByTeam.TryGetValue(building.teamID, out var list))
                list.Remove(building);

            OnBuildingUnregistered?.Invoke(building);
        }
    }

    // --- Change Notifications ----------------------------------------------

    public void NotifyBuildingChanged(CraftingBuilding building)
    {
        if (building == null)
            return;

        if (building.State == CraftingBuilding.ProductionState.OutputReady)
            OnBuildingCompleted?.Invoke(building);

        OnBuildingChanged?.Invoke(building);
    }

    // --- Queries ------------------------------------------------------------

    public IReadOnlyCollection<CraftingBuilding> AllBuildings => allBuildings;

    public IReadOnlyList<CraftingBuilding> GetBuildingsForTeam(int teamID)
    {
        if (buildingsByTeam.TryGetValue(teamID, out var list))
            return list;

        return Array.Empty<CraftingBuilding>();
    }
}