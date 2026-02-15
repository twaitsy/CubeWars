#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ResourceNode))]
public class ResourceNodeEditor : Editor
{
    ResourceNode node;
    ResourcesDatabase db;
    ResourceDefinition[] defs;
    string[] names;
    int index;

    void OnEnable()
    {
        node = (ResourceNode)target;

        db = AssetDatabase.LoadAssetAtPath<ResourcesDatabase>(
            "Assets/Data/Databases/ResourcesDatabase.asset"
        );

        if (db == null || db.resources == null || db.resources.Count == 0)
        {
            defs = new ResourceDefinition[0];
            names = new[] { "NO DATABASE FOUND" };
            index = -1;
            return;
        }

        defs = db.resources.ToArray();
        names = new string[defs.Length];
        index = -1;

        for (int i = 0; i < defs.Length; i++)
        {
            ResourceDefinition def = defs[i];
            names[i] = def != null ? def.displayName : "(Missing Entry)";
            if (node.resource == def)
                index = i;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (defs == null || defs.Length == 0)
        {
            EditorGUILayout.HelpBox("No ResourceDefinitions found in ResourcesDatabase.asset.", MessageType.Warning);
        }
        else
        {
            if (node.resource != null && index < 0)
                EditorGUILayout.HelpBox($"Current resource '{node.resource.displayName}' is not from ResourcesDatabase.asset. Re-select to migrate.", MessageType.Info);

            int displayedIndex = Mathf.Clamp(index, 0, defs.Length - 1);
            int newIndex = EditorGUILayout.Popup("Resource", displayedIndex, names);
            if (newIndex != index && newIndex >= 0 && newIndex < defs.Length)
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
