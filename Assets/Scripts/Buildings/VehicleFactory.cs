using UnityEngine;

public class VehicleFactory : Building
{
    [Header("Vehicle Production")]
    public GameObject vehiclePrefab;
    public float buildSeconds = 10f;
    public ResourceCost[] buildCost;

    private float timer;
    private bool building;

    public void StartBuildVehicle()
    {
        if (building) return;
        if (TeamResources.Instance == null) return;
        if (vehiclePrefab == null) return;

        if (!TeamResources.Instance.CanAfford(teamID, buildCost)) return;
        if (!TeamResources.Instance.Spend(teamID, buildCost)) return;

        building = true;
        timer = 0f;
    }

    void Update()
    {
        if (!IsAlive) return;
        if (!building) return;

        timer += Time.deltaTime;
        if (timer >= buildSeconds)
        {
            building = false;
            Vector3 pos = transform.position + transform.forward * 4f;
            GameObject vehicle = Instantiate(vehiclePrefab, pos, Quaternion.identity);
            TeamOwnershipUtility.ApplyTeamToHierarchy(vehicle, teamID);
        }
    }
}
