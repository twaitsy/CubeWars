#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ResourceAuthoringWindow : EditorWindow
{
    ResourceDefinition resource;
    int startAmount = 500;
    int maxGatherers = 2;
    string prefabName = "ResourceNode_New";
    string saveFolder = "Assets/Prefabs/Resources";

    [MenuItem("CubeWars/Tools/Resource Authoring")]
    static void Open() => GetWindow<ResourceAuthoringWindow>("Resource Authoring");

    void OnGUI()
    {
        resource = (ResourceDefinition)EditorGUILayout.ObjectField("Resource", resource, typeof(ResourceDefinition), false);
        startAmount = Mathf.Max(1, EditorGUILayout.IntField("Starting Amount", startAmount));
        maxGatherers = Mathf.Max(1, EditorGUILayout.IntField("Max Gatherers", maxGatherers));
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        if (GUILayout.Button("Create Resource Prefab")) CreatePrefab();
    }

    void CreatePrefab()
    {
        if (resource == null || string.IsNullOrWhiteSpace(prefabName) || !AssetDatabase.IsValidFolder(saveFolder)) return;
        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = prefabName;
        ResourceNode node = root.AddComponent<ResourceNode>();
        node.resource = resource;
        node.remaining = startAmount;
        node.maxGatherers = maxGatherers;
        string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{prefabName}.prefab");
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        AssetDatabase.SaveAssets();
    }
}
#endif
