using UnityEngine;

public class CivilianSpawner : MonoBehaviour
{
    [Header("Civilian Settings")]
    public GameObject civilianPrefab;
    public int teamID;
    public int civilianCount = 10;

    [Header("Spawn Area")]
    public Vector3 spawnArea = new(5f, 0f, 5f);

    void Start()
    {
        SpawnCivilians();
    }

    void SpawnCivilians()
    {
        for (int i = 0; i < civilianCount; i++)
        {
            Vector3 spawnPos = transform.position +
                new Vector3(
                    Random.Range(-spawnArea.x, spawnArea.x),
                    0.5f,
                    Random.Range(-spawnArea.z, spawnArea.z)
                );

            GameObject civ = Instantiate(civilianPrefab, spawnPos, Quaternion.identity);

            TeamAssignmentUtility.ApplyTeamToHierarchy(civ, teamID);

            Civilian civScript = civ.GetComponent<Civilian>();
            if (civScript != null)
                civScript.SetRole(CivilianRole.Gatherer);

            TeamColorManager.Instance.ApplyTeamColor(civ, teamID);
        }
    }
}
