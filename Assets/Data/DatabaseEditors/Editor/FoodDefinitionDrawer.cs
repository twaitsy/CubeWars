using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FoodDefinition))]
public class FoodDefinitionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        property.isExpanded = true; // Force expanded
        float height = EditorGUIUtility.singleLineHeight * 2 + 4; // Header + selector line
        if (property.isExpanded)
        {
            SerializedProperty iter = property.Copy();
            SerializedProperty end = iter.GetEndProperty();
            bool enter = true;
            while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
            {
                if (iter.name == "resource") continue; // Skip resource since we handle it separately
                height += EditorGUI.GetPropertyHeight(iter, true) + EditorGUIUtility.standardVerticalSpacing;
                enter = false;
            }
        }
        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label (no foldout)
        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label);
        position.y += labelRect.height + 2;

        // Draw resource selector
        SerializedProperty resourceProp = property.FindPropertyRelative("resource");
        string currName = resourceProp.FindPropertyRelative("displayName").stringValue;
        string currCat = resourceProp.FindPropertyRelative("category").enumDisplayNames[resourceProp.FindPropertyRelative("category").enumValueIndex];
        string display = string.IsNullOrEmpty(currName) ? "(None)" : $"{currName} — {currCat}";
        Rect selectRect = new Rect(position.x, position.y, position.width - 110, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(selectRect, display);
        Rect btnRect = new Rect(selectRect.xMax + 2, position.y, 110, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(btnRect, "Select Resource"))
        {
            ShowPicker(btnRect, resourceProp);
        }
        position.y += EditorGUIUtility.singleLineHeight + 2;

        // Draw other child properties (always expanded)
        EditorGUI.indentLevel++;
        SerializedProperty iter = property.Copy();
        SerializedProperty end = iter.GetEndProperty();
        bool enter = true;
        while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
        {
            if (iter.name == "resource") { enter = false; continue; } // Skip resource
            float h = EditorGUI.GetPropertyHeight(iter, true);
            Rect fieldRect = new Rect(position.x, position.y, position.width, h);
            EditorGUI.PropertyField(fieldRect, iter, true);
            position.y += h + EditorGUIUtility.standardVerticalSpacing;
            enter = false;
        }
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    private void ShowPicker(Rect activatorRect, SerializedProperty resourceProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.resources == null || db.resources.resources == null)
        {
            EditorUtility.DisplayDialog("No Resources", "Your GameDatabase is missing a ResourcesDatabase or the resources list is null.", "OK");
            return;
        }
        var list = db.resources.resources;
        if (list == null || list.Count == 0)
        {
            EditorUtility.DisplayDialog("No Resources", "Your ResourcesDatabase contains no ResourceDefinitions.", "OK");
            return;
        }

        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((ResourceDefinition)r).displayName,
            getCategory: r => ((ResourceDefinition)r).category.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;

                CopyToProperty(resourceProp, (ResourceDefinition)selected);
                resourceProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }

    private void CopyToProperty(SerializedProperty property, ResourceDefinition source)
    {
        property.FindPropertyRelative("id").stringValue = source.id;
        property.FindPropertyRelative("displayName").stringValue = source.displayName;
        property.FindPropertyRelative("icon").objectReferenceValue = source.icon;
        property.FindPropertyRelative("category").enumValueIndex = (int)source.category;
        property.FindPropertyRelative("weight").floatValue = source.weight;
        property.FindPropertyRelative("baseValue").intValue = source.baseValue;
    }

    private GameDatabase FindGameDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}