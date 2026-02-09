using UnityEngine;

public class SixTeamBootstrap : MonoBehaviour
{
    [Header("Teams")]
    public int teamCount = 6;
    public int playerTeamID = 0;

    [Header("Prefabs")]
    public GameObject hqPrefab;
    public GameObject civilianPrefab;

    [Header("Spawn Layout")]
    public float spawnRadius = 40f;
    public float civilianOffset = 3.0f;

    [Header("Starting Civilians Per Team")]
    public int startingGatherersPerTeam = 1;
    public int startingBuildersPerTeam = 1;

    void Start()
    {
        SpawnTeams();
    }

    void SpawnTeams()
    {
        Vector3 center = Vector3.zero;

        for (int team = 0; team < teamCount; team++)
        {
            float a = (team / (float)teamCount) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spawnRadius;

            Quaternion rot = Quaternion.LookRotation((center - pos).normalized, Vector3.up);

            GameObject hq = Instantiate(hqPrefab, pos, rot);
            TeamAssignmentUtility.ApplyTeamToHierarchy(hq, team);

            // Spawn civilians near HQ
            Vector3 right = hq.transform.right;
            Vector3 forward = hq.transform.forward;

            int index = 0;

            for (int i = 0; i < startingGatherersPerTeam; i++)
            {
                Vector3 cpos = pos + right * (civilianOffset + index * 1.25f) + forward * 1.5f;
                SpawnCivilian(team, CivilianRole.Gatherer, cpos, rot);
                index++;
            }

            for (int i = 0; i < startingBuildersPerTeam; i++)
            {
                Vector3 cpos = pos - right * (civilianOffset + index * 1.25f) + forward * 1.5f;
                SpawnCivilian(team, CivilianRole.Builder, cpos, rot);
                index++;
            }
        }
    }

    void SpawnCivilian(int teamID, CivilianRole role, Vector3 pos, Quaternion rot)
    {
        if (civilianPrefab == null) return;

        GameObject c = Instantiate(civilianPrefab, pos, rot);
        TeamAssignmentUtility.ApplyTeamToHierarchy(c, teamID);

        Civilian civ = c.GetComponent<Civilian>();
        if (civ != null)
            civ.SetRole(role);
    }
}
