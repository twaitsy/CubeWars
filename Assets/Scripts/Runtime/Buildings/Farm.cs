using UnityEngine;

public class Farm : Building
{
    public ResourceDefinition foodResource;
    public ResourceDefinition goldResource;
    public int level = 1;
    public int maxLevel = 3;
    public float productionInterval = 2f;
    public int[] foodPerTickByLevel = new int[] { 2, 5, 10 };
    public int[] goldCostByNextLevel = new int[] { 20, 50, 120 };
    public bool autoUpgrade = false;
    public float autoUpgradeCheckInterval = 2f;


    protected override void ApplyDefinition(BuildingDefinition def)
    {
        base.ApplyDefinition(def);
        if (def == null)
            return;

        if (def.upkeepInterval > 0f)
            productionInterval = def.upkeepInterval;
    }

    float productionTimer;
    float upgradeTimer;

    protected override void Start() { base.Start(); level = Mathf.Clamp(level, 1, maxLevel); }

    void Update()
    {
        if (!IsAlive || TeamResources.Instance == null || foodResource == null)
            return;
        productionTimer += Time.deltaTime;
        if (productionTimer >= productionInterval)
        {
            productionTimer = 0f;
            int idx = Mathf.Clamp(level - 1, 0, foodPerTickByLevel.Length - 1);
            int amount = foodPerTickByLevel[idx];
            if (TeamResources.Instance.GetFreeCapacity(teamID, foodResource) > 0)
                TeamResources.Instance.Deposit(teamID, foodResource, amount);
        }

        if (!autoUpgrade) return;
        upgradeTimer += Time.deltaTime;
        if (upgradeTimer >= autoUpgradeCheckInterval) { upgradeTimer = 0f; TryUpgrade(); }
    }

    public bool TryUpgrade()
    {
        if (TeamResources.Instance == null || goldResource == null || level >= maxLevel) return false;
        int cost = goldCostByNextLevel[Mathf.Clamp(level - 1, 0, goldCostByNextLevel.Length - 1)];
        if (!TeamResources.Instance.SpendResource(teamID, goldResource, cost)) return false;
        level++;
        return true;
    }
}
