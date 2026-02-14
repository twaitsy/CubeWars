using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ResourceAmount))]
public class ResourceAmountDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var resourceProp = property.FindPropertyRelative("resource");
        var amountProp = property.FindPropertyRelative("amount");

        Rect line1 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect line2 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        EditorGUI.LabelField(line1, label);

        float buttonWidth = 110f;
        float amountWidth = 60f;

        Rect resourceRect = new Rect(line2.x, line2.y, line2.width - buttonWidth - amountWidth - 6, line2.height);
        Rect pickRect = new Rect(resourceRect.xMax + 2, line2.y, buttonWidth, line2.height);
        Rect amountRect = new Rect(pickRect.xMax + 2, line2.y, amountWidth, line2.height);

        // Read current embedded ResourceDefinition fields
        string displayName = "";
        string categoryName = "";

        if (resourceProp != null && resourceProp.propertyType == SerializedPropertyType.Generic)
        {
            var displayNameProp = resourceProp.FindPropertyRelative("displayName");
            var categoryProp = resourceProp.FindPropertyRelative("category");

            if (displayNameProp != null)
                displayName = displayNameProp.stringValue;

            if (categoryProp != null)
                categoryName = categoryProp.enumDisplayNames.Length > categoryProp.enumValueIndex
                    ? categoryProp.enumDisplayNames[categoryProp.enumValueIndex]
                    : "";
        }

        string display = string.IsNullOrEmpty(displayName)
            ? "(None)"
            : $"{displayName} — {categoryName}";

        EditorGUI.LabelField(resourceRect, display);

        if (GUI.Button(pickRect, "Select Resource"))
        {
            ShowResourcePicker(pickRect, resourceProp);
        }

        EditorGUI.PropertyField(amountRect, amountProp, GUIContent.none);

        EditorGUI.EndProperty();
    }

    void ShowResourcePicker(Rect activatorRect, SerializedProperty resourceProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.resources == null || db.resources.resources == null)
        {
            EditorUtility.DisplayDialog(
                "No Resources",
                "Your GameDatabase is missing a ResourcesDatabase or the resources list is null.",
                "OK"
            );
            return;
        }

        var list = db.resources.resources;
        if (list == null || list.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Resources",
                "Your ResourcesDatabase contains no ResourceDefinitions.",
                "OK"
            );
            return;
        }

        var menu = new GenericMenu();

        for (int i = 0; i < list.Count; i++)
        {
            var r = list[i];
            int index = i;

            if (r == null)
            {
                menu.AddDisabledItem(new GUIContent("(null)"));
                continue;
            }

            var label = new GUIContent($"{r.displayName} — {r.category}");
            menu.AddItem(label, false, () =>
            {
                var selected = list[index];
                if (selected == null)
                    return;

                resourceProp.FindPropertyRelative("id").stringValue = selected.id;
                resourceProp.FindPropertyRelative("displayName").stringValue = selected.displayName;
                resourceProp.FindPropertyRelative("icon").objectReferenceValue = selected.icon;
                resourceProp.FindPropertyRelative("category").enumValueIndex = (int)selected.category;
                resourceProp.FindPropertyRelative("weight").floatValue = selected.weight;
                resourceProp.FindPropertyRelative("baseValue").intValue = selected.baseValue;

                resourceProp.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.DropDown(activatorRect);
    }

    GameDatabase FindGameDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}