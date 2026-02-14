using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ProductionRecipeDefinition))]
public class ProductionRecipeDefinitionDrawer : PropertyDrawer
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

        // Draw selector
        string currName = property.FindPropertyRelative("recipeName").stringValue;
        Rect selectRect = new Rect(position.x, position.y, position.width - 110, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(selectRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
        Rect btnRect = new Rect(selectRect.xMax + 2, position.y, 110, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(btnRect, "Select Recipe"))
        {
            ShowPicker(btnRect, property);
        }
        position.y += EditorGUIUtility.singleLineHeight + 2;

        // Draw child properties (always expanded)
        EditorGUI.indentLevel++;
        SerializedProperty iter = property.Copy();
        SerializedProperty end = iter.GetEndProperty();
        bool enter = true;
        while (iter.NextVisible(enter) && !SerializedProperty.EqualContents(iter, end))
        {
            float h = EditorGUI.GetPropertyHeight(iter, true);
            Rect fieldRect = new Rect(position.x, position.y, position.width, h);
            EditorGUI.PropertyField(fieldRect, iter, true);
            position.y += h + EditorGUIUtility.standardVerticalSpacing;
            enter = false;
        }
        EditorGUI.indentLevel--;

        EditorGUI.EndProperty();
    }

    private void ShowPicker(Rect activatorRect, SerializedProperty property)
    {
        var db = FindGameDatabase();
        if (db == null || db.recipes == null || db.recipes.recipes == null)
        {
            EditorUtility.DisplayDialog("No Recipes", "Your GameDatabase is missing a RecipesDatabase or the recipes list is null.", "OK");
            return;
        }
        var list = db.recipes.recipes;
        if (list == null || list.Count == 0)
        {
            EditorUtility.DisplayDialog("No Recipes", "Your RecipesDatabase contains no ProductionRecipeDefinitions.", "OK");
            return;
        }

        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((ProductionRecipeDefinition)r).recipeName,
            getCategory: r => ((ProductionRecipeDefinition)r).requiredJobType.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;

                CopyToProperty(property, (ProductionRecipeDefinition)selected);
                property.serializedObject.ApplyModifiedProperties();
            }
        );
    }

    private void CopyToProperty(SerializedProperty property, ProductionRecipeDefinition source)
    {
        property.FindPropertyRelative("recipeName").stringValue = source.recipeName;
        CopyResourceAmountArray(property.FindPropertyRelative("inputs"), source.inputs);
        CopyResourceAmountArray(property.FindPropertyRelative("outputs"), source.outputs);
        property.FindPropertyRelative("craftTimeSeconds").floatValue = source.craftTimeSeconds;
        property.FindPropertyRelative("batchSize").intValue = source.batchSize;
        property.FindPropertyRelative("requiredJobType").enumValueIndex = (int)source.requiredJobType;
        property.FindPropertyRelative("outputEfficiencyMultiplier").floatValue = source.outputEfficiencyMultiplier;
        property.FindPropertyRelative("inputEfficiencyMultiplier").floatValue = source.inputEfficiencyMultiplier;
        property.FindPropertyRelative("requiresPower").boolValue = source.requiresPower;
        property.FindPropertyRelative("requiresFuel").boolValue = source.requiresFuel;
    }

    private void CopyResourceAmountArray(SerializedProperty arrayProp, RecipeResourceAmount[] source)
    {
        if (source == null)
        {
            arrayProp.arraySize = 0;
            return;
        }

        arrayProp.arraySize = source.Length;
        for (int i = 0; i < source.Length; i++)
        {
            SerializedProperty elem = arrayProp.GetArrayElementAtIndex(i);
            RecipeResourceAmount src = source[i];

            elem.FindPropertyRelative("amount").intValue = src.amount;

            SerializedProperty resProp = elem.FindPropertyRelative("resource");
            if (src.resource != null)
            {
                resProp.FindPropertyRelative("id").stringValue = src.resource.id;
                resProp.FindPropertyRelative("displayName").stringValue = src.resource.displayName;
                resProp.FindPropertyRelative("icon").objectReferenceValue = src.resource.icon;
                resProp.FindPropertyRelative("category").enumValueIndex = (int)src.resource.category;
                resProp.FindPropertyRelative("weight").floatValue = src.resource.weight;
                resProp.FindPropertyRelative("baseValue").intValue = src.resource.baseValue;
            }
            else
            {
                // Clear if null
                resProp.FindPropertyRelative("id").stringValue = "";
                resProp.FindPropertyRelative("displayName").stringValue = "";
                resProp.FindPropertyRelative("icon").objectReferenceValue = null;
                resProp.FindPropertyRelative("category").enumValueIndex = 0;
                resProp.FindPropertyRelative("weight").floatValue = 0f;
                resProp.FindPropertyRelative("baseValue").intValue = 0;
            }
        }
    }

    private GameDatabase FindGameDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}