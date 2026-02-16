using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CubeWars/Database/Recipes")]
public class RecipesDatabase : ScriptableObject
{
    public List<ProductionRecipeDefinition> recipes = new();
}