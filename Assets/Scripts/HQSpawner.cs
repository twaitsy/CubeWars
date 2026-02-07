using UnityEngine;

/// <summary>
/// Spawns an HQ for every Team in the scene that does not already have one.
/// This is a lightweight bootstrap component that ONLY handles HQ spawning.
/// It does NOT spawn UI, camera, systems, or anything else.
/// </summary>
public class HQSpawner : MonoBehaviour
{
    [Header("HQ Prefab")]
    [Tooltip("The HQ building prefab that will be spawned for each team.")]
    public GameObject hqPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Optional offset applied when spawning HQs. Useful if HQRoot is at 0,0,0.")]
    public Vector3 spawnOffset = Vector3.zero;

    void Start()
    {
        // Find all Team components in the scene
        Team[] teams = FindObjectsOfType<Team>();

        foreach (var team in teams)
        {
            // --- SAFETY CHECK 1: Ensure the team has an HQ root ---
            if (team.hqRoot == null)
            {
                Debug.LogWarning(
                    $"Team {team.teamID} has no HQ root child object. " +
                    $"Expected: Team_X -> HQ. Skipping HQ spawn."
                );
                continue;
            }

            // --- SAFETY CHECK 2: Skip if HQ already exists ---
            if (team.hqRoot.childCount > 0)
            {
                // HQ already placed manually or by another system
                continue;
            }

            // --- SPAWN HQ ---
            Vector3 spawnPos = team.hqRoot.position + spawnOffset;
            GameObject hq = Instantiate(hqPrefab, spawnPos, Quaternion.identity);

            // Parent under the HQ root for clean hierarchy
            hq.transform.SetParent(team.hqRoot);

            // --- ASSIGN TEAM OWNERSHIP ---
            // Headquarters inherits from Building, so we assign teamID via Building
            Building building = hq.GetComponent<Building>();
            if (building != null)
            {
                building.teamID = team.teamID;
            }
            else
            {
                Debug.LogWarning(
                    $"HQ prefab '{hqPrefab.name}' has no Building component. " +
                    $"Cannot assign team ownership."
                );
            }

            Debug.Log($"Spawned HQ for Team {team.teamID}");
        }
    }
}