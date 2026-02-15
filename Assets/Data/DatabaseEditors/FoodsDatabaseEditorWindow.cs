using UnityEditor;

public class FoodsDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Foods;

    [MenuItem("Tools/CubeWars/Database/Foods Editor")]
    public static void Open()
    {
        OpenWindow<FoodsDatabaseEditorWindow>("Foods Database");
    }
}
