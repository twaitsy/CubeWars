using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance;

    void Awake()
    {
        Instance = this;
    }

    // ---------------------------------------------------------
    // DEPENDENCIES:
    // - TeamResources: must implement SpendResource(teamID, type, amount)
    // - TeamStorageManager: used for GetTotalStored(teamID, type)
    // - TeamInventory: must implement AddTool(teamID, item, amount)
    // - ToolItem: must contain craftCost[]
    // ---------------------------------------------------------

    public bool CraftTool(int teamID, ToolItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        if (TeamResources.Instance == null || TeamInventory.Instance == null) return false;

        // Check resource availability
        if (item.craftCost != null)
        {
            for (int i = 0; i < item.craftCost.Length; i++)
            {
                int need = item.craftCost[i].amount * amount;

                // NEW: use TeamStorageManager for stored resources
                int stored = TeamStorageManager.Instance.GetTotalStored(teamID, item.craftCost[i].resource);
                if (stored < need)
                    return false;
            }

            // Spend resources
            for (int i = 0; i < item.craftCost.Length; i++)
            {
                int need = item.craftCost[i].amount * amount;
                TeamResources.Instance.SpendResource(teamID, item.craftCost[i].resource, need);
            }
        }

        TeamInventory.Instance.AddTool(teamID, item, amount);
        return true;
    }
}