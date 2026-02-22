using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TeamBootstrap
/// Initializes HQs, civilians, units, buildings, and AI subsystems for each team.
/// Cleaned version: reduced branching, unified helpers, flattened logic.
/// </summary>
public class TeamBootstrap : MonoBehaviour
{
    // ---------------------------------------------------------
    // DATA CLASSES
    // ---------------------------------------------------------

    [System.Serializable]
    public class StartingCivilianGroup
    {
        public GameObject prefab;
        [Min(0)] public int count = 1;
        public CivilianJobType jobType = CivilianJobType.Gatherer;
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
        public GameObject prefab;
        public Vector2Int gridCoord;
        [Range(0, 3)] public int quarterTurns;
        public float yOffset;
    }

    // ---------------------------------------------------------
    // INSPECTOR FIELDS
    // ---------------------------------------------------------

    [Header("Prefabs")]
    public GameObject hqPrefab;
    public GameObject workerPrefab;

    [Header("Starting Civilians")]
    public bool spawnOneCivilianPerJob = true;
    public List<CivilianJobType> startingJobTypes = new()
    {
        CivilianJobType.Gatherer, CivilianJobType.Builder, CivilianJobType.Hauler,
        CivilianJobType.Crafter, CivilianJobType.Farmer, CivilianJobType.Technician,
        CivilianJobType.Scientist, CivilianJobType.Engineer, CivilianJobType.Blacksmith,
        CivilianJobType.Carpenter, CivilianJobType.Cook
    };

    public List<StartingCivilianGroup> civilianGroups = new();
    public int startingWorkers = 3;
    public CivilianJobType startingWorkerJob = CivilianJobType.Gatherer;

    [Header("Starter Specialists")]
    public bool spawnStarterSpecialists = true;
    public List<CivilianJobType> starterSpecialistJobs = new()
    {
        CivilianJobType.Carpenter, CivilianJobType.Blacksmith, CivilianJobType.Cook,
        CivilianJobType.Farmer, CivilianJobType.Engineer, CivilianJobType.Crafter
    };

    [Header("Starting Combat Units")]
    public List<StartingUnitGroup> startingUnitGroups = new();
    public GameObject startingUnitPrefab;
    public int startingUnits = 1;

    [Header("Starting Buildings")]
    public List<StartingBuildingSpawn> startingBuildings = new();

    [Header("Starter Housing")]
    public bool autoAddStarterHouses = true;
    public GameObject starterHousePrefab;
    public List<Vector2Int> starterHouseGridCoords = new()
    {
        new Vector2Int(7,7), new Vector2Int(9,7), new Vector2Int(7,9)
    };

    [Header("Spawn Offsets")]
    public Vector3 hqSpawnOffset = Vector3.zero;
    public Vector3 unitSpawnOffset = new(2f, 0f, 2f);

    // ---------------------------------------------------------
    // UNITY ENTRY
    // ---------------------------------------------------------

    void Start()
    {
        Team[] teams = FindObjectsByType<Team>(FindObjectsSortMode.None);

        foreach (var team in teams)
        {
            SetupHQ(team);
            SetupCivilians(team);
            SetupUnits(team);
            SetupAI(team);
        }

        StartCoroutine(SetupBuildingsAfterGridReady(teams));
    }

    // ---------------------------------------------------------
    // HQ SETUP
    // ---------------------------------------------------------

    void SetupHQ(Team team)
    {
        if (team.hqRoot == null || hqPrefab == null || team.hqRoot.childCount > 0)
            return;

        Vector3 pos = team.hqRoot.position + hqSpawnOffset;

        BuildGridManager grid = FindFirstObjectByType<BuildGridManager>();
        if (grid != null)
        {
            float cell = Mathf.Max(0.1f, grid.cellSize);
            pos.x = Mathf.Round(pos.x / cell) * cell;
            pos.z = Mathf.Round(pos.z / cell) * cell;
            pos.y = grid.yOffset + hqSpawnOffset.y;
        }

        GameObject hq = Instantiate(hqPrefab, pos, Quaternion.identity, team.hqRoot);
        TeamAssignmentUtility.ApplyTeamToHierarchy(hq, team.teamID);
    }

    // ---------------------------------------------------------
    // CIVILIANS
    // ---------------------------------------------------------

    void SetupCivilians(Team team)
    {
        if (!TryGetHQ(team, out Transform hq))
            return;

        int index = 0;

        // 1. One-per-job mode
        if (spawnOneCivilianPerJob && workerPrefab != null && startingJobTypes.Count > 0)
        {
            foreach (var job in startingJobTypes)
            {
                if (job is CivilianJobType.Generalist or CivilianJobType.Idle)
                    continue;

                SpawnCivilian(team, hq, workerPrefab, job, index++);
            }
        }
        // 2. Detailed groups
        else if (civilianGroups.Count > 0)
        {
            foreach (var g in civilianGroups)
            {
                if (g.prefab == null || g.count <= 0)
                    continue;

                for (int i = 0; i < g.count; i++)
                    SpawnCivilian(team, hq, g.prefab, g.jobType, index++);
            }
        }
        // 3. Legacy fallback
        else if (workerPrefab != null && startingWorkers > 0)
        {
            for (int i = 0; i < startingWorkers; i++)
                SpawnCivilian(team, hq, workerPrefab, startingWorkerJob, index++);
        }

        // 4. Starter specialists
        if (spawnStarterSpecialists && workerPrefab != null)
        {
            foreach (var job in starterSpecialistJobs)
            {
                if (job is CivilianJobType.Generalist or CivilianJobType.Idle)
                    continue;

                SpawnCivilian(team, hq, workerPrefab, job, index++);
            }
        }
    }

    void SpawnCivilian(Team team, Transform hq, GameObject prefab, CivilianJobType job, int index)
    {
        Vector3 pos = RingPos(hq.position, index, 1.5f);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, team.unitsRoot);
        TeamAssignmentUtility.ApplyTeamToHierarchy(go, team.teamID);

        Civilian civ = go.GetComponentInChildren<Civilian>();
        if (civ != null)
        {
            civ.SetJobType(job == CivilianJobType.Generalist ? CivilianJobType.Gatherer : job);
            civ.GrantStartingToolForCurrentJob();
        }
    }

    // ---------------------------------------------------------
    // UNITS
    // ---------------------------------------------------------

    void SetupUnits(Team team)
    {
        if (!TryGetHQ(team, out Transform hq))
            return;

        int index = 0;

        if (startingUnitGroups.Count > 0)
        {
            foreach (var g in startingUnitGroups)
            {
                if (g.prefab == null || g.count <= 0)
                    continue;

                for (int i = 0; i < g.count; i++)
                    SpawnUnit(team, hq, g.prefab, index++);
            }
        }
        else if (startingUnitPrefab != null && startingUnits > 0)
        {
            for (int i = 0; i < startingUnits; i++)
                SpawnUnit(team, hq, startingUnitPrefab, index++);
        }
    }

    void SpawnUnit(Team team, Transform hq, GameObject prefab, int index)
    {
        Vector3 pos = RingPos(hq.position, index, 2.25f);
        GameObject go = Instantiate(prefab, pos, Quaternion.identity, team.unitsRoot);
        TeamAssignmentUtility.ApplyTeamToHierarchy(go, team.teamID);
    }

    // ---------------------------------------------------------
    // BUILDINGS
    // ---------------------------------------------------------

    IEnumerator SetupBuildingsAfterGridReady(Team[] teams)
    {
        EnsureStarterHouses();

        if (startingBuildings.Count == 0)
            yield break;

        yield return null;
        yield return null;

        BuildGridManager grid = FindFirstObjectByType<BuildGridManager>();
        if (grid == null)
            yield break;

        foreach (var team in teams)
            SetupBuildings(team, grid);

        AssignStarterCrafterRecipes(teams);
        grid.RefreshPlayerGridVisibility();
    }

    void EnsureStarterHouses()
    {
        if (!autoAddStarterHouses || starterHousePrefab == null)
            return;

        foreach (var coord in starterHouseGridCoords)
        {
            bool exists = startingBuildings.Exists(b =>
                b.prefab == starterHousePrefab && b.gridCoord == coord);

            if (!exists)
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

    void SetupBuildings(Team team, BuildGridManager grid)
    {
        Transform parent = team.buildingsRoot != null ? team.buildingsRoot : team.transform;

        foreach (var spawn in startingBuildings)
        {
            if (spawn.prefab == null)
                continue;

            if (!TryResolveFootprint(team.teamID, spawn, grid, out var pos, out var rot, out var cells))
                continue;

            GameObject building = Instantiate(spawn.prefab, pos, rot, parent);
            TeamAssignmentUtility.ApplyTeamToHierarchy(building, team.teamID);

            if (!building.TryGetComponent(out BuildGridOccupant occ))
                occ = building.AddComponent<BuildGridOccupant>();

            occ.SetOccupiedCells(cells, building);
        }
    }

    bool TryResolveFootprint(int teamID, StartingBuildingSpawn spawn, BuildGridManager grid,
        out Vector3 pos, out Quaternion rot, out List<BuildGridCell> cells)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;
        cells = null;

        BuildingFootprint fp = BuildingFootprint.FindOnPrefab(spawn.prefab);
        Vector2Int dims = fp != null ? fp.GetDimensionsForQuarterTurns(spawn.quarterTurns) : Vector2Int.one;

        if (!grid.TryGetCenteredFootprintCells(teamID, spawn.gridCoord, dims, out cells, out Vector3 center))
            return false;

        if (cells == null || cells.Count == 0 || AnyOccupied(cells))
            return false;

        rot = Quaternion.Euler(0f, spawn.quarterTurns * 90f + (fp != null ? fp.extraYRotation : 0f), 0f);
        pos = center + new Vector3(0f, spawn.yOffset, 0f);

        if (fp != null)
            pos += rot * fp.worldOffset;

        return true;
    }

    bool AnyOccupied(List<BuildGridCell> cells)
    {
        foreach (var c in cells)
        {
            if (c == null || c.isOccupied)
                return true;

            if (c.TryGetComponent(out BuildCellReservation r) && r.blockBuildingPlacement)
                return true;
        }
        return false;
    }

    // ---------------------------------------------------------
    // RECIPES
    // ---------------------------------------------------------

    void AssignStarterCrafterRecipes(Team[] teams)
    {
        GameDatabase db = GameDatabaseLoader.ResolveLoaded();
        if (db == null || db.recipes == null || db.recipes.recipes.Count == 0)
            return;

        var recipes = db.recipes.recipes;

        foreach (var team in teams)
        {
            var buildings = FindObjectsByType<CraftingBuilding>(FindObjectsSortMode.None);
            int assigned = 0;

            foreach (var b in buildings)
            {
                if (b.teamID != team.teamID)
                    continue;

                b.SetRecipe(recipes[assigned % recipes.Count]);
                if (++assigned >= 6)
                    break;
            }
        }
    }

    // ---------------------------------------------------------
    // AI
    // ---------------------------------------------------------

    void SetupAI(Team team)
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

    void AddIfMissing<T>(GameObject obj) where T : Component
    {
        if (!obj.TryGetComponent(out T _))
            obj.AddComponent<T>();
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

    bool TryGetHQ(Team team, out Transform hq)
    {
        hq = null;
        if (team.hqRoot == null || team.hqRoot.childCount == 0)
            return false;

        hq = team.hqRoot.GetChild(0);
        return true;
    }

    Vector3 RingPos(Vector3 center, int index, float spacing)
    {
        int row = index / 4;
        int col = index % 4;

        Vector3 rowOffset = (row + 1) * spacing * unitSpawnOffset.normalized;
        Vector3 sideOffset = new((col - 1.5f) * spacing, 0f, 0f);

        return center + rowOffset + sideOffset;
    }
}