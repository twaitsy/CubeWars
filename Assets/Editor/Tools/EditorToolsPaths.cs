using UnityEditor;

public static class EditorToolsPaths
{
    public const string Root = "Assets/Documentation/EditorToolsOutput";
    public const string ScriptIndex = Root + "/ScriptIndex";
    public const string Analysis = Root + "/Analysis";
    public const string Reports = Root + "/Reports";
    public const string Exports = Root + "/Exports";

    public static void EnsureFolder(string path)
    {
        string[] segments = path.Split('/');
        if (segments.Length < 2)
            return;

        string current = segments[0];
        for (int i = 1; i < segments.Length; i++)
        {
            string next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }
}
