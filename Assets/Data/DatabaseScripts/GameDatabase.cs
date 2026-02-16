using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Game Database")]
public class GameDatabase : ScriptableObject
{
    public JobsDatabase jobs;
    public ToolsDatabase tools;
    public BuildingsDatabase buildings;
    public ResourcesDatabase resources;
    public UnitsDatabase units;
    public RecipesDatabase recipes;
    public FoodDatabase foods;
    public TechTreeDatabase techTree;
    public NeedsDatabase needs;

    public bool TryGetUnitById(string id, out UnitDefinition definition)
    {
        definition = null;
        return units != null && units.TryGetById(id, out definition);
    }

    public bool TryGetBuildingById(string id, out BuildingDefinition definition)
    {
        definition = null;
        if (buildings == null || buildings.buildings == null || string.IsNullOrWhiteSpace(id))
            return false;

        string target = id.Trim().ToLowerInvariant();
        foreach (var item in buildings.buildings)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.id))
                continue;

            if (item.id.Trim().ToLowerInvariant() == target)
            {
                definition = item;
                return true;
            }
        }

        return false;
    }
}
