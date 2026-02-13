using UnityEditor;
using UnityEngine;

public static class CreateDatabaseMenu
{
    [MenuItem("CubeWars/Create/Jobs Database")]
    public static void CreateJobs()
    {
        CreateAsset<JobsDatabase>("JobsDatabase.asset");
    }

    [MenuItem("CubeWars/Create/Tools Database")]
    public static void CreateTools()
    {
        CreateAsset<ToolsDatabase>("ToolsDatabase.asset");
    }

    [MenuItem("CubeWars/Create/Buildings Database")]
    public static void CreateBuildings()
    {
        CreateAsset<BuildingsDatabase>("BuildingsDatabase.asset");
    }

    [MenuItem("CubeWars/Create/Resources Database")]
    public static void CreateResources()
    {
        CreateAsset<ResourcesDatabase>("BuildingsDatabase.asset");
    }

    private static void CreateAsset<T>(string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, "Assets/Data/Databases/" + name);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}