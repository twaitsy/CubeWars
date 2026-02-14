using UnityEditor;
using UnityEngine;

public static class ListDrawer
{
    public static void DrawList(SerializedObject so, string listName, string label, string[] headerFields)
    {
        var list = so.FindProperty(listName);
        if (list == null)
        {
            EditorGUILayout.HelpBox($"List '{listName}' not found.", MessageType.Warning);
            return;
        }

        GUILayout.Space(6);
        GUILayout.Label(label, EditorStyles.boldLabel);

        EditorGUI.indentLevel++;

        for (int i = 0; i < list.arraySize; i++)
        {
            var element = list.GetArrayElementAtIndex(i);

            // Build header text from fields
            string header = $"Element {i}";
            foreach (var field in headerFields)
            {
                var prop = element.FindPropertyRelative(field);
                if (prop != null)
                {
                    if (prop.propertyType == SerializedPropertyType.String)
                        header += $" — {prop.stringValue}";
                }
            }

            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, header, true);

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("X", GUILayout.Width(24)))
            {
                list.DeleteArrayElementAtIndex(i);
                so.ApplyModifiedProperties();
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();

            if (element.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(element, true);
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }

        GUILayout.Space(4);
        if (GUILayout.Button($"Add New {label}"))
        {
            list.arraySize++;
            so.ApplyModifiedProperties();
        }

        EditorGUI.indentLevel--;
    }
}