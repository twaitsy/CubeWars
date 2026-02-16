#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(ResourceNode))]
[CanEditMultipleObjects]
public class ResourceNodeEditor : Editor
{
    private ResourceNode node;
    private ResourcesDatabase db;
    private List<ResourceDefinition> defs = new List<ResourceDefinition>();

    void OnEnable()
    {
        node = (ResourceNode)target;

        db = AssetDatabase.LoadAssetAtPath<ResourcesDatabase>(
            "Assets/Data/Databases/ResourcesDatabase.asset"
        );

        if (db == null || db.resources == null)
        {
            defs.Clear();
            return;
        }

        defs = db.resources.ToList();

        // This is the line that finally works reliably
        if (node.resource == null)
            node.EditorAutoAssignFromPrefabName();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── SEARCHABLE PICKER ─────────────────────────────────────────────
        ResourceDefinition current = node.resource;
        string buttonLabel = current != null ? current.displayName : "— None —";

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Resource");
        if (GUILayout.Button(buttonLabel, EditorStyles.popup, GUILayout.MinWidth(200)))
        {
            ShowSearchablePicker();
        }
        EditorGUILayout.EndHorizontal();

        if (current == null)
            EditorGUILayout.HelpBox("Auto-assign ran from prefab name. Use the button to pick manually.", MessageType.Info);

        // Other fields
        EditorGUILayout.PropertyField(serializedObject.FindProperty("remaining"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxGatherers"));

        DrawPropertiesExcluding(serializedObject, "resource", "remaining", "maxGatherers");

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowSearchablePicker()
    {
        List<object> items = defs.Cast<object>().ToList();

        SearchablePickerPopup.Show(
            new Rect(Event.current.mousePosition, Vector2.zero),
            items,
            item => (item as ResourceDefinition)?.displayName ?? "None",
            item => (item as ResourceDefinition)?.id ?? "",
            item =>
            {
                if (item is ResourceDefinition selected)
                {
                    Undo.RecordObject(node, "Change Resource");
                    node.resource = selected;
                    EditorUtility.SetDirty(node);
                    serializedObject.Update();
                }
            }
        );
    }
}
#endif