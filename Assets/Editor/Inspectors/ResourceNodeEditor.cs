#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceNode))]
public class ResourceNodeEditor : Editor
{
    private ResourceNode node;
    private ResourcesDatabase db;
    private ResourceDefinition[] defs;
    private string[] names;
    private int index;

    void OnEnable()
    {
        node = (ResourceNode)target;

        // Load your database from the exact path you specified
        db = AssetDatabase.LoadAssetAtPath<ResourcesDatabase>(
            "Assets/Data/Databases/ResourcesDatabase.asset"
        );

        if (db == null)
        {
            defs = new ResourceDefinition[0];
            names = new[] { "NO DATABASE FOUND" };
            return;
        }

        // Adjust this line to match your actual DB field name
        defs = db.resources.ToArray();

        names = new string[defs.Length];

        for (int i = 0; i < defs.Length; i++)
        {
            names[i] = defs[i].displayName; // <-- correct field
            if (node.resource == defs[i])
                index = i;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (defs.Length == 0)
        {
            EditorGUILayout.HelpBox("No ResourceDefinitions found in database.", MessageType.Warning);
        }
        else
        {
            int newIndex = EditorGUILayout.Popup("Resource", index, names);
            if (newIndex != index)
            {
                Undo.RecordObject(node, "Change Resource");
                index = newIndex;
                node.resource = defs[index];
                EditorUtility.SetDirty(node);
            }
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("remaining"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxGatherers"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif