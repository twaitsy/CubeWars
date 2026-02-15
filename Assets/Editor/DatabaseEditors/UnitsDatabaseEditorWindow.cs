using UnityEditor;

public class UnitsDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Units;

    [MenuItem("Tools/CubeWars/Database/Units Editor")]
    public static void Open()
    {
        OpenWindow<UnitsDatabaseEditorWindow>("Units Database");
    }
}
