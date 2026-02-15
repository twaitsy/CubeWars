using UnityEditor;

public class BuildingsDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Buildings;

    [MenuItem("Tools/CubeWars/Database/Buildings Editor")]
    public static void Open()
    {
        OpenWindow<BuildingsDatabaseEditorWindow>("Buildings Database");
    }
}
