using UnityEngine;

public class TeamResources : MonoBehaviour
{
    public static TeamResources Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int GetAvailable(int teamID, ResourceDefinition resource)
    {
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetAvailable(teamID, resource);
    }

    public bool CanAfford(int teamID, ResourceCost[] costs)
    {
        if (RTSGameSettings.IsCheatActive(c => c.infiniteResources || c.unlockAllBuilds))
            return true;

        if (TeamStorageManager.Instance == null) return false;
        return TeamStorageManager.Instance.CanAffordAvailable(teamID, costs);
    }

    public bool Spend(int teamID, ResourceCost[] costs)
    {
        if (!CanAfford(teamID, costs))
            return false;

        foreach (var c in costs)
            SpendResource(teamID, c.resource, c.amount);

        return true;
    }

    public bool SpendResource(int teamID, ResourceDefinition resource, int amount)
    {
        if (RTSGameSettings.IsCheatActive(c => c.infiniteResources))
            return true;

        if (TeamStorageManager.Instance == null) return false;
        if (amount <= 0) return true;

        int taken = TeamStorageManager.Instance.Withdraw(teamID, resource, amount);
        return taken == amount;
    }

    public int Deposit(int teamID, ResourceDefinition resource, int amount)
    {
        if (TeamStorageManager.Instance == null) return 0;
        if (amount <= 0) return 0;

        return TeamStorageManager.Instance.Deposit(teamID, resource, amount);
    }

    public int GetFreeCapacity(int teamID, ResourceDefinition resource)
    {
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetTotalFree(teamID, resource);
    }

    public int GetResource(int teamID, ResourceDefinition resource)
    {
        if (TeamStorageManager.Instance == null) return 0;
        return TeamStorageManager.Instance.GetTotalStored(teamID, resource);
    }
}
