using UnityEditor;

public class RecipesDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Recipes;

    [MenuItem("Tools/CubeWars/Database/Recipes Editor")]
    public static void Open()
    {
        OpenWindow<RecipesDatabaseEditorWindow>("Recipes Database");
    }
}
