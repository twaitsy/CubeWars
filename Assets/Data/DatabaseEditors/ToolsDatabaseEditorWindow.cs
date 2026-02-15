using UnityEditor;

public class ToolsDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Tools;

    [MenuItem("Tools/CubeWars/Database/Tools Editor")]
    public static void Open()
    {
        OpenWindow<ToolsDatabaseEditorWindow>("Tools Database");
    }
}
