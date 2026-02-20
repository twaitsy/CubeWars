using System.Collections.Generic;
using UnityEngine;

public class VehicleFactory : Building
{
    [Header("Vehicle Production")]
    public GameObject vehiclePrefab;
    public float buildSeconds = 10f;
    public ResourceCost[] buildCost;


    protected override void ApplyDefinition(BuildingDefinition def)
    {
        base.ApplyDefinition(def);
        if (def == null)
            return;

        if (def.buildTime > 0f)
            buildSeconds = def.buildTime;

        if (def.constructionCost != null && def.constructionCost.Count > 0)
        {
            List<ResourceCost> mapped = new();
            for (int i = 0; i < def.constructionCost.Count; i++)
            {
                var entry = def.constructionCost[i];
                if (entry == null || entry.resource == null || entry.amount <= 0)
                    continue;

                mapped.Add(new ResourceCost { resource = entry.resource, amount = entry.amount });
            }

            if (mapped.Count > 0)
                buildCost = mapped.ToArray();
        }
    }

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
            Instantiate(vehiclePrefab, pos, Quaternion.identity);
        }
    }
}
