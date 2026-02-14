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
}