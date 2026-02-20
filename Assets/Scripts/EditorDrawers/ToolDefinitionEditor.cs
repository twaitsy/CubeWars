using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ToolDefinition))]
public class ToolDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Tool Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("durability"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseEfficiency"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Bonuses", EditorStyles.boldLabel);

        SerializedProperty bonuses = serializedObject.FindProperty("bonuses");
        EditorGUILayout.PropertyField(bonuses, true);

        serializedObject.ApplyModifiedProperties();
    }
}