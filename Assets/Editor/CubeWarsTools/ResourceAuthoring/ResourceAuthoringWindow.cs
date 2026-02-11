#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ResourceAuthoringWindow : EditorWindow
{
    ResourceType resourceType = ResourceType.Food;
    int startAmount = 500;
    int maxGatherers = 2;
    string prefabName = "ResourceNode_New";
    string saveFolder = "Assets/Prefabs/Resources";

    [MenuItem("CubeWars/Tools/Resource Authoring")]
    static void Open()
    {
        GetWindow<ResourceAuthoringWindow>("Resource Authoring");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Create Resource Node Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool creates a ResourceNode prefab configured with a resource type, starting amount, and gather slot limit.",
            MessageType.Info);

        resourceType = (ResourceType)EditorGUILayout.EnumPopup("Resource Type", resourceType);
        startAmount = EditorGUILayout.IntField("Starting Amount", startAmount);
        maxGatherers = EditorGUILayout.IntField("Max Gatherers", maxGatherers);
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);
        saveFolder = EditorGUILayout.TextField("Save Folder", saveFolder);

        startAmount = Mathf.Max(1, startAmount);
        maxGatherers = Mathf.Max(1, maxGatherers);

        GUILayout.Space(8f);

        if (GUILayout.Button("Create Resource Prefab", GUILayout.Height(30f)))
            CreatePrefab();
    }

    void CreatePrefab()
    {
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            EditorUtility.DisplayDialog("Resource Authoring", "Please provide a prefab name.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(saveFolder))
        {
            EditorUtility.DisplayDialog("Resource Authoring", $"Folder does not exist: {saveFolder}", "OK");
            return;
        }

        GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = prefabName;
        root.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);

        ResourceNode node = root.AddComponent<ResourceNode>();
        node.type = resourceType;
        node.remaining = startAmount;
        node.maxGatherers = maxGatherers;

        string path = AssetDatabase.GenerateUniqueAssetPath($"{saveFolder}/{prefabName}.prefab");
        PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        EditorUtility.DisplayDialog("Resource Authoring", $"Created prefab:\n{path}", "OK");
    }
}
#endif
