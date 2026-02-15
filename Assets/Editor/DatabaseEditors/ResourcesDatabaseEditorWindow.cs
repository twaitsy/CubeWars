using UnityEditor;

public class ResourcesDatabaseEditorWindow : GameDatabaseEditorWindowBase
{
    protected override DatabaseTab TargetTab => DatabaseTab.Resources;

    [MenuItem("Tools/CubeWars/Database/Resources Editor")]
    public static void Open()
    {
        OpenWindow<ResourcesDatabaseEditorWindow>("Resources Database");
    }
}
