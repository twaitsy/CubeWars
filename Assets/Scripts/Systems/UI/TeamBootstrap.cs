using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TeamBootstrap
///
/// PURPOSE:
/// - Initialize each Team object at runtime.
/// - Spawn HQs for teams missing one.
/// - Spawn configurable starting civilians, units, and buildings.
/// - Attach AI subsystems to AI teams.
/// </summary>
public class TeamBootstrap : MonoBehaviour
{
    [System.Serializable]
    public class StartingCivilianGroup
    {
        public GameObject prefab;
        [Min(0)] public int count = 1;
        public CivilianRole role = CivilianRole.Gatherer;
        public CivilianJobType jobType = CivilianJobType.Generalist;
    }

    [System.Serializable]
    public class StartingUnitGroup
    {
        public GameObject prefab;
        [Min(0)] public int count = 1;
    }

    [System.Serializable]
    public class StartingBuildingSpawn
    {
        [Tooltip("Building prefab to place on the build grid.")]
        public GameObject prefab;

        [Tooltip("Grid coordinate (relative to this team's HQ grid) used as the footprint anchor.")]
        public Vector2Int gridCoord;

        [Range(0, 3)]
        [Tooltip("Rotation in quarter turns (0=0°, 1=90°, 2=180°, 3=270°).")]
        public int quarterTurns;

        [Tooltip("Optional additional world-space Y offset.")]
        public float yOffset;
    }

    [Header("Prefabs")]
    [Tooltip("HQ building prefab for each team.")]
    public GameObject hqPrefab;

    [Tooltip("Legacy/default worker prefab. Used when civilianGroups is empty.")]
    public GameObject workerPrefab;

    [Header("Starting Civilians")]
    [Tooltip("Detailed civilian setup. If empty, legacy worker fields are used.")]
    public List<StartingCivilianGroup> civilianGroups = new List<StartingCivilianGroup>();

    [Tooltip("Legacy fallback: how many workers each team starts with when civilianGroups is empty.")]
    public int startingWorkers = 3;

    [Tooltip("Legacy fallback role assigned to spawned workers when civilianGroups is empty.")]
    public CivilianRole startingWorkerRole = CivilianRole.Gatherer;

    [Header("Starting Combat Units")]
    [Tooltip("Detailed starting combat unit setup. If empty, legacy unit fields are used.")]
    public List<StartingUnitGroup> startingUnitGroups = new List<StartingUnitGroup>();

    [Tooltip("Legacy fallback combat unit prefab when startingUnitGroups is empty.")]
    public GameObject startingUnitPrefab;

    [Tooltip("Legacy fallback number of combat units when startingUnitGroups is empty.")]
    public int startingUnits = 1;

    [Header("Starting Buildings")]
    [Tooltip("Starting building prefabs to place directly on team build-grid cells.")]
    public List<StartingBuildingSpawn> startingBuildings = new List<StartingBuildingSpawn>();

    [Header("Starter Housing")]
    [Tooltip("When enabled, automatically attempts to place a few houses on each team's grid if not already listed in startingBuildings.")]
    public bool autoAddStarterHouses = true;

    [Tooltip("House prefab used for auto starter housing.")]
    public GameObject starterHousePrefab;

    [Tooltip("Grid coordinates used when auto-adding houses.")]
    public List<Vector2Int> starterHouseGridCoords = new List<Vector2Int>
    {
        new Vector2Int(7, 7),
        new Vector2Int(9, 7),
        new Vector2Int(7, 9),
    };

    [Header("Spawn Offsets")]
    [Tooltip("Offset applied when spawning HQs.")]
    public Vector3 hqSpawnOffset = Vector3.zero;

    [Tooltip("Base offset used for civilian/unit spawn rings around HQ.")]
    public Vector3 unitSpawnOffset = new Vector3(2f, 0f, 2f);

    void Start()
    {
        Team[] teams = FindObjectsOfType<Team>();

        foreach (var team in teams)
        {
            SetupHQ(team);
            SetupCivilians(team);
            SetupStartingUnits(team);
            SetupAI(team);
        }

        StartCoroutine(SetupStartingBuildingsAfterGridReady(teams));
    }

    private IEnumerator SetupStartingBuildingsAfterGridReady(Team[] teams)
    {
        EnsureStarterHousesInBuildList();

        if (startingBuildings == null || startingBuildings.Count == 0)
            yield break;

        // BuildGridManager performs delayed setup; wait until grids exist.
        yield return null;
        yield return null;

        BuildGridManager grid = FindObjectOfType<BuildGridManager>();
        if (grid == null)
        {
            Debug.LogWarning("[TeamBootstrap] Cannot place starting buildings: no BuildGridManager found.");
            yield break;
        }

        foreach (var team in teams)
            SetupStartingBuildings(team, grid);

        grid.RefreshPlayerGridVisibility();
    }

    void EnsureStarterHousesInBuildList()
    {
        if (!autoAddStarterHouses || starterHousePrefab == null || starterHouseGridCoords == null || starterHouseGridCoords.Count == 0)
            return;

        if (startingBuildings == null)
            startingBuildings = new List<StartingBuildingSpawn>();

        for (int i = 0; i < starterHouseGridCoords.Count; i++)
        {
            Vector2Int coord = starterHouseGridCoords[i];
            bool alreadyListed = false;

            for (int j = 0; j < startingBuildings.Count; j++)
            {
                StartingBuildingSpawn existing = startingBuildings[j];
                if (existing == null)
                    continue;

                if (existing.prefab == starterHousePrefab && existing.gridCoord == coord)
                {
                    alreadyListed = true;
                    break;
                }
            }

            if (!alreadyListed)
            {
                startingBuildings.Add(new StartingBuildingSpawn
                {
                    prefab = starterHousePrefab,
                    gridCoord = coord,
                    quarterTurns = 0,
                    yOffset = 0f
                });
            }
        }
    }

    private void SetupHQ(Team team)
    {
        if (team.hqRoot == null)
        {
            Debug.LogWarning($"Team {team.teamID} has no HQ root. Expected: Team_X -> HQ.");
            return;
        }

        if (team.hqRoot.childCount > 0 || hqPrefab == null)
            return;

        Vector3 spawnPos = team.hqRoot.position + hqSpawnOffset;
        BuildGridManager grid = FindObjectOfType<BuildGridManager>();
        if (grid != null)
        {
            float cellSize = Mathf.Max(0.1f, grid.cellSize);
            spawnPos.x = Mathf.Round(spawnPos.x / cellSize) * cellSize;
            spawnPos.z = Mathf.Round(spawnPos.z / cellSize) * cellSize;
            spawnPos.y = grid.yOffset;
        }

        GameObject hq = Instantiate(hqPrefab, spawnPos, Quaternion.identity);
        hq.transform.SetParent(team.hqRoot);

        TeamAssignmentUtility.ApplyTeamToHierarchy(hq, team.teamID);
    }

    private void SetupCivilians(Team team)
    {
        if (team.unitsRoot == null || team.hqRoot == null || team.hqRoot.childCount == 0)
            return;

        Transform hq = team.hqRoot.GetChild(0);
        int spawnIndex = 0;

        if (civilianGroups != null && civilianGroups.Count > 0)
        {
            for (int g = 0; g < civilianGroups.Count; g++)
            {
                StartingCivilianGroup group = civilianGroups[g];
                if (group == null || group.prefab == null || group.count <= 0)
                    continue;

                for (int i = 0; i < group.count; i++)
                {
                    SpawnCivilian(team, hq, group.prefab, group.role, group.jobType, spawnIndex);
                    spawnIndex++;
                }
            }
        }
        else if (workerPrefab != null && startingWorkers > 0)
        {
            for (int i = 0; i < startingWorkers; i++)
            {
                SpawnCivilian(team, hq, workerPrefab, startingWorkerRole, CivilianJobRegistry.ToJobType(startingWorkerRole), spawnIndex);
                spawnIndex++;
            }
        }
    }

    private void SpawnCivilian(Team team, Transform hq, GameObject prefab, CivilianRole role, CivilianJobType jobType, int spawnIndex)
    {
        Vector3 spawnPos = GetSpawnPosition(hq.position, spawnIndex, 1.5f);
        GameObject worker = Instantiate(prefab, spawnPos, Quaternion.identity);
        worker.transform.SetParent(team.unitsRoot);

        TeamAssignmentUtility.ApplyTeamToHierarchy(worker, team.teamID);

        Civilian civ = worker.GetComponentInChildren<Civilian>();
        if (civ != null)
        {
            civ.SetRole(role);
            civ.SetJobType(jobType == CivilianJobType.Generalist ? CivilianJobRegistry.ToJobType(role) : jobType);
        }
    }

    private void SetupStartingUnits(Team team)
    {
        if (team.unitsRoot == null || team.hqRoot == null || team.hqRoot.childCount == 0)
            return;

        Transform hq = team.hqRoot.GetChild(0);
        int spawnIndex = 0;

        if (startingUnitGroups != null && startingUnitGroups.Count > 0)
        {
            for (int g = 0; g < startingUnitGroups.Count; g++)
            {
                StartingUnitGroup group = startingUnitGroups[g];
                if (group == null || group.prefab == null || group.count <= 0)
                    continue;

                for (int i = 0; i < group.count; i++)
                {
                    SpawnUnit(team, hq, group.prefab, spawnIndex);
                    spawnIndex++;
                }
            }
        }
        else if (startingUnitPrefab != null && startingUnits > 0)
        {
            for (int i = 0; i < startingUnits; i++)
            {
                SpawnUnit(team, hq, startingUnitPrefab, spawnIndex);
                spawnIndex++;
            }
        }
    }

    private void SpawnUnit(Team team, Transform hq, GameObject prefab, int spawnIndex)
    {
        Vector3 spawnPos = GetSpawnPosition(hq.position, spawnIndex, 2.25f);
        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);
        unit.transform.SetParent(team.unitsRoot);
        TeamAssignmentUtility.ApplyTeamToHierarchy(unit, team.teamID);
    }

    private Vector3 GetSpawnPosition(Vector3 hqPos, int spawnIndex, float spacing)
    {
        int row = spawnIndex / 4;
        int col = spawnIndex % 4;

        Vector3 rowOffset = unitSpawnOffset.normalized * spacing * (row + 1);
        Vector3 sideOffset = new Vector3((col - 1.5f) * spacing, 0f, 0f);
        return hqPos + rowOffset + sideOffset;
    }

    private void SetupStartingBuildings(Team team, BuildGridManager grid)
    {
        if (startingBuildings == null || startingBuildings.Count == 0)
            return;

        Transform parent = team.buildingsRoot != null ? team.buildingsRoot : team.transform;

        for (int i = 0; i < startingBuildings.Count; i++)
        {
            StartingBuildingSpawn spawn = startingBuildings[i];
            if (spawn == null || spawn.prefab == null)
                continue;

            BuildingFootprint footprint = BuildingFootprint.FindOnPrefab(spawn.prefab);
            Vector2Int dimensions = footprint != null
                ? footprint.GetDimensionsForQuarterTurns(spawn.quarterTurns)
                : Vector2Int.one;

            if (!grid.TryGetCenteredFootprintCells(team.teamID, spawn.gridCoord, dimensions, out List<BuildGridCell> cells, out Vector3 footprintCenter) || cells == null || cells.Count == 0)
            {
                Debug.LogWarning($"[TeamBootstrap] Team {team.teamID} building '{spawn.prefab.name}' failed: grid footprint out of bounds at {spawn.gridCoord}.");
                continue;
            }

            if (AnyCellOccupied(cells))
            {
                Debug.LogWarning($"[TeamBootstrap] Team {team.teamID} building '{spawn.prefab.name}' failed: one or more footprint cells are already occupied.");
                continue;
            }

            Quaternion rot = Quaternion.Euler(0f, spawn.quarterTurns * 90f + (footprint != null ? footprint.extraYRotation : 0f), 0f);
            Vector3 pos = footprintCenter + new Vector3(0f, spawn.yOffset, 0f);
            if (footprint != null)
                pos += rot * footprint.worldOffset;

            GameObject building = Instantiate(spawn.prefab, pos, rot);
            building.transform.SetParent(parent);
            TeamAssignmentUtility.ApplyTeamToHierarchy(building, team.teamID);

            BuildGridOccupant occupant = building.GetComponent<BuildGridOccupant>();
            if (occupant == null)
                occupant = building.AddComponent<BuildGridOccupant>();

            occupant.SetOccupiedCells(cells, building);
        }
    }

    private bool AnyCellOccupied(List<BuildGridCell> cells)
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] == null || cells[i].isOccupied)
                return true;

            BuildCellReservation reservation = cells[i].GetComponent<BuildCellReservation>();
            if (reservation != null && reservation.blockBuildingPlacement)
                return true;
        }

        return false;
    }

    private void SetupAI(Team team)
    {
        if (team.teamType != TeamType.AI)
            return;

        AddIfMissing<AIPlayer>(team.gameObject);
        AddIfMissing<AIEconomy>(team.gameObject);
        AddIfMissing<AIMilitary>(team.gameObject);
        AddIfMissing<AIBuilder>(team.gameObject);
        AddIfMissing<AIResourceManager>(team.gameObject);
        AddIfMissing<AIThreatDetector>(team.gameObject);
    }

    private void AddIfMissing<T>(GameObject obj) where T : Component
    {
        if (obj.GetComponent<T>() == null)
            obj.AddComponent<T>();
    }
}
