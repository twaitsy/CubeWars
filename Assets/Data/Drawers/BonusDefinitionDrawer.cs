using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BonusDefinition))]
public class BonusDefinitionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        // id + targetType + resource/category + multiplier + spacing
        return EditorGUIUtility.singleLineHeight * 5 + 10;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null)
            return;

        EditorGUI.BeginProperty(position, label, property);

        // Foldout
        Rect fold = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(fold, property.isExpanded, label, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;
        float line = EditorGUIUtility.singleLineHeight + 2;
        Rect r = new Rect(position.x, position.y + line, position.width, EditorGUIUtility.singleLineHeight);

        // -------------------------
        // ID
        // -------------------------
        var idProp = property.FindPropertyRelative("id");
        if (idProp != null)
        {
            EditorGUI.PropertyField(r, idProp);
            r.y += line;
        }

        // -------------------------
        // Target Type
        // -------------------------
        var targetTypeProp = property.FindPropertyRelative("targetType");
        if (targetTypeProp != null)
        {
            EditorGUI.PropertyField(r, targetTypeProp);
            r.y += line;
        }

        // -------------------------
        // Resource or Category
        // -------------------------
        if (targetTypeProp != null &&
            (BonusTargetType)targetTypeProp.enumValueIndex == BonusTargetType.Resource)
        {
            var resourceProp = property.FindPropertyRelative("resource");
            DrawResourceSelector(ref r, resourceProp);
        }
        else
        {
            var categoryProp = property.FindPropertyRelative("category");
            if (categoryProp != null)
            {
                EditorGUI.PropertyField(r, categoryProp);
                r.y += line;
            }
        }

        // -------------------------
        // Multiplier
        // -------------------------
        var multiplierProp = property.FindPropertyRelative("multiplier");
        if (multiplierProp != null)
        {
            EditorGUI.PropertyField(r, multiplierProp);
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    // ============================================================
    // RESOURCE PICKER (same pattern as RecipeResourceAmountDrawer)
    // ============================================================
    private void DrawResourceSelector(ref Rect r, SerializedProperty resourceProp)
    {
        float line = EditorGUIUtility.singleLineHeight + 2;

        if (resourceProp == null)
        {
            EditorGUI.LabelField(r, "(Invalid Resource)");
            r.y += line;
            return;
        }

        var nameProp = resourceProp.FindPropertyRelative("displayName");
        var categoryProp = resourceProp.FindPropertyRelative("category");

        string name = nameProp != null ? nameProp.stringValue : "(None)";
        string cat = "(None)";

        if (categoryProp != null &&
            categoryProp.enumValueIndex >= 0 &&
            categoryProp.enumValueIndex < categoryProp.enumDisplayNames.Length)
        {
            cat = categoryProp.enumDisplayNames[categoryProp.enumValueIndex];
        }

        string display = $"{name} — {cat}";

        float buttonWidth = 120f;
        Rect labelRect = new Rect(r.x, r.y, r.width - buttonWidth - 4, r.height);
        Rect buttonRect = new Rect(labelRect.xMax + 2, r.y, buttonWidth, r.height);

        EditorGUI.LabelField(labelRect, display);

        if (GUI.Button(buttonRect, "Select Resource"))
        {
            ShowResourcePicker(buttonRect, resourceProp);
        }

        r.y += line;
    }

    private void ShowResourcePicker(Rect activatorRect, SerializedProperty resourceProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.resources == null || db.resources.resources == null)
        {
            EditorUtility.DisplayDialog("No Resources",
                "ResourcesDatabase missing or empty.",
                "OK");
            return;
        }

        var list = db.resources.resources;
        var menu = new GenericMenu();

        foreach (var r in list)
        {
            if (r == null)
            {
                menu.AddDisabledItem(new GUIContent("(null)"));
                continue;
            }

            var content = new GUIContent($"{r.displayName} — {r.category}");
            menu.AddItem(content, false, () =>
            {
                CopyResource(resourceProp, r);
                resourceProp.serializedObject.ApplyModifiedProperties();
            });
        }

        menu.DropDown(activatorRect);
    }

    private void CopyResource(SerializedProperty prop, ResourceDefinition src)
    {
        if (prop == null || src == null)
            return;

        var id = prop.FindPropertyRelative("id");
        if (id != null) id.stringValue = src.id;

        var dn = prop.FindPropertyRelative("displayName");
        if (dn != null) dn.stringValue = src.displayName;

        var icon = prop.FindPropertyRelative("icon");
        if (icon != null) icon.objectReferenceValue = src.icon;

        var cat = prop.FindPropertyRelative("category");
        if (cat != null) cat.enumValueIndex = (int)src.category;

        var weight = prop.FindPropertyRelative("weight");
        if (weight != null) weight.floatValue = src.weight;

        var baseValue = prop.FindPropertyRelative("baseValue");
        if (baseValue != null) baseValue.intValue = src.baseValue;
    }

    private GameDatabase FindGameDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}