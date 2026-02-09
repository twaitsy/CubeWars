using UnityEngine;

/// <summary>
/// TeamBootstrap
///
/// PURPOSE:
/// - Initialize each Team object at runtime.
/// - Spawn HQs for teams missing one.
/// - Spawn starting workers.
/// - Attach AI subsystems to AI teams.
///
/// DEPENDENCIES:
/// - Team.cs:
///     * Provides teamID, teamType, hqRoot, unitsRoot.
/// - Building.cs:
///     * HQ prefab must contain a Building component for teamID assignment.
/// - Unit.cs:
///     * Worker prefab must contain a Unit component for teamID assignment.
/// - AIPlayer / AIEconomy / AIMilitary / AIBuilder / AIResourceManager / AIThreatDetector:
///     * Added automatically to AI teams if missing.
/// - GameManager:
///     * Detects teams after bootstrap runs.
/// - TeamColorManager (optional):
///     * HQ and workers may use TeamVisual to apply colors.
/// - TeamVisual (optional):
///     * If present on prefabs, will apply team colors automatically.
///
/// NOTES FOR FUTURE MAINTENANCE:
/// - This script NEVER deletes teams.
/// - This script NEVER enforces a maximum team count.
/// - This script NEVER modifies teamIDs.
/// - This script NEVER spawns UI, camera, or global systems.
/// - HQ and worker prefabs MUST have correct components (Building / Unit).
/// - If you add new AI subsystems, include them in SetupAI().
/// - If you add new team roots (e.g., Vehicles), extend SetupWorkers().
///
/// ARCHITECTURE:
/// - Runs once at scene start.
/// - Safe to use with any number of Team objects.
/// - Designed to work with your multi-team RTS architecture.
/// </summary>
public class TeamBootstrap : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("HQ building prefab for each team.")]
    public GameObject hqPrefab;

    [Tooltip("Worker unit prefab spawned at game start.")]
    public GameObject workerPrefab;

    [Header("Starting Units")]
    [Tooltip("How many workers each team starts with.")]
    public int startingWorkers = 3;

    [Header("Spawn Offsets")]
    [Tooltip("Offset applied when spawning HQs.")]
    public Vector3 hqSpawnOffset = Vector3.zero;

    [Tooltip("Offset applied when spawning workers relative to HQ.")]
    public Vector3 workerSpawnOffset = new Vector3(2f, 0f, 2f);

    void Start()
    {
        Team[] teams = FindObjectsOfType<Team>();

        foreach (var team in teams)
        {
            SetupHQ(team);
            SetupWorkers(team);
            SetupAI(team);
        }
    }

    // -------------------------------------------------------------
    // 1. HQ SETUP
    // -------------------------------------------------------------
    private void SetupHQ(Team team)
    {
        if (team.hqRoot == null)
        {
            Debug.LogWarning($"Team {team.teamID} has no HQ root. Expected: Team_X -> HQ.");
            return;
        }

        // Skip if HQ already exists
        if (team.hqRoot.childCount > 0)
            return;

        // Spawn HQ
        Vector3 spawnPos = team.hqRoot.position + hqSpawnOffset;
        GameObject hq = Instantiate(hqPrefab, spawnPos, Quaternion.identity);
        hq.transform.SetParent(team.hqRoot);

        TeamOwnershipUtility.ApplyTeamToHierarchy(hq, team.teamID);

        Debug.Log($"Spawned HQ for Team {team.teamID}");
    }

    // -------------------------------------------------------------
    // 2. WORKER SETUP
    // -------------------------------------------------------------
    private void SetupWorkers(Team team)
    {
        if (workerPrefab == null)
            return;

        if (team.unitsRoot == null)
        {
            Debug.LogWarning($"Team {team.teamID} has no Units root. Expected: Team_X -> Units.");
            return;
        }

        // Spawn workers near HQ
        Transform hq = team.hqRoot.childCount > 0 ? team.hqRoot.GetChild(0) : null;
        if (hq == null)
            return;

        for (int i = 0; i < startingWorkers; i++)
        {
            Vector3 offset = workerSpawnOffset * i;
            Vector3 spawnPos = hq.position + offset;

            GameObject worker = Instantiate(workerPrefab, spawnPos, Quaternion.identity);
            worker.transform.SetParent(team.unitsRoot);

            TeamOwnershipUtility.ApplyTeamToHierarchy(worker, team.teamID);
        }

        Debug.Log($"Spawned {startingWorkers} workers for Team {team.teamID}");
    }

    // -------------------------------------------------------------
    // 3. AI SETUP
    // -------------------------------------------------------------
    private void SetupAI(Team team)
    {
        if (team.teamType != TeamType.AI)
            return;

        // Add AI components ONLY if missing
        AddIfMissing<AIPlayer>(team.gameObject);
        AddIfMissing<AIEconomy>(team.gameObject);
        AddIfMissing<AIMilitary>(team.gameObject);
        AddIfMissing<AIBuilder>(team.gameObject);
        AddIfMissing<AIResourceManager>(team.gameObject);
        AddIfMissing<AIThreatDetector>(team.gameObject);

        Debug.Log($"AI scripts assigned to Team {team.teamID}");
    }

    private void AddIfMissing<T>(GameObject obj) where T : Component
    {
        if (obj.GetComponent<T>() == null)
            obj.AddComponent<T>();
    }
}