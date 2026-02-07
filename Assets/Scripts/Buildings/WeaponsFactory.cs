using UnityEngine;

public class WeaponsFactory : Building
{
    [Header("Crafting Output (adds to TeamInventory as tools/weapons)")]
    public ToolItem weaponTool;
    public float craftSeconds = 8f;

    private float timer;
    private bool crafting;

    public void StartCraft()
    {
        if (crafting) return;
        if (weaponTool == null) return;
        if (CraftingSystem.Instance == null) return;

        // Spend immediately via CraftingSystem (uses ToolItem.craftCost)
        bool ok = CraftingSystem.Instance.CraftTool(teamID, weaponTool, 1);
        if (!ok) return;

        // Optional: make it take time (we already spent cost)
        crafting = true;
        timer = 0f;
    }

    void Update()
    {
        if (!IsAlive) return;
        if (!crafting) return;

        timer += Time.deltaTime;
        if (timer >= craftSeconds)
        {
            crafting = false;
            // Tool already added by CraftingSystem; this timer is just “manufacturing time”
        }
    }
}
