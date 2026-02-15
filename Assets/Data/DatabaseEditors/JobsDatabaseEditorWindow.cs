using UnityEditor;

public class JobsDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Jobs;

    [MenuItem("Tools/CubeWars/Database/Jobs Editor")]
    public static void Open()
    {
        OpenWindow<JobsDatabaseEditorWindow>("Jobs Database");
    }
}
