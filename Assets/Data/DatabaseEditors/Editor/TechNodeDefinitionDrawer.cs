using System.Linq;
using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(TechNodeDefinition))]
public class TechNodeDefinitionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight * 2 + 4; // Base for header + fields
        height += GetManualFieldHeights(property);
        return height;
    }
    private float GetManualFieldHeights(SerializedProperty property)
    {
        float height = 0;
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // id
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // displayName
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // icon
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("description"), true) + EditorGUIUtility.standardVerticalSpacing; // description
        height += GetListHeight(property.FindPropertyRelative("researchCost")); // researchCost
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // researchTime
        height += GetListHeight(property.FindPropertyRelative("prerequisites")); // prerequisites
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // requiredBuilding
        height += GetListHeight(property.FindPropertyRelative("unlockBuildings")); // unlockBuildings
        height += GetListHeight(property.FindPropertyRelative("unlockUnits")); // unlockUnits
        height += GetListHeight(property.FindPropertyRelative("unlockRecipes")); // unlockRecipes
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // globalProductionSpeedMultiplier
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // globalCombatDamageMultiplier
        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // workerEfficiencyMultiplier
        return height;
    }
    private float GetListHeight(SerializedProperty listProp)
    {
        float h = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // For list label
        if (listProp.isExpanded)
        {
            h += EditorGUIUtility.singleLineHeight * (listProp.arraySize + 1); // One line per element + add button
            h += EditorGUIUtility.standardVerticalSpacing * (listProp.arraySize + 1);
        }
        return h;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(rect, label);
        position.y += EditorGUIUtility.singleLineHeight + 2;
        EditorGUI.indentLevel++;
        string currentId = property.FindPropertyRelative("id").stringValue;
        DrawStringField(property.FindPropertyRelative("id"), ref position, "ID");
        DrawStringField(property.FindPropertyRelative("displayName"), ref position, "Display Name");
        DrawObjectField(property.FindPropertyRelative("icon"), ref position, "Icon");
        DrawTextAreaField(property.FindPropertyRelative("description"), ref position, "Description");
        DrawResourceAmountList(property.FindPropertyRelative("researchCost"), ref position, "Research Cost");
        DrawFloatField(property.FindPropertyRelative("researchTime"), ref position, "Research Time");
        DrawPrerequisiteList(property.FindPropertyRelative("prerequisites"), ref position, "Prerequisites", currentId);
        DrawBuildingDropdown(property.FindPropertyRelative("requiredBuilding"), ref position, "Required Building");
        DrawUnlockBuildingsList(property.FindPropertyRelative("unlockBuildings"), ref position, "Unlock Buildings");
        DrawUnlockUnitsList(property.FindPropertyRelative("unlockUnits"), ref position, "Unlock Units");
        DrawUnlockRecipesList(property.FindPropertyRelative("unlockRecipes"), ref position, "Unlock Recipes");
        DrawFloatField(property.FindPropertyRelative("globalProductionSpeedMultiplier"), ref position, "Global Production Speed Multiplier");
        DrawFloatField(property.FindPropertyRelative("globalCombatDamageMultiplier"), ref position, "Global Combat Damage Multiplier");
        DrawFloatField(property.FindPropertyRelative("workerEfficiencyMultiplier"), ref position, "Worker Efficiency Multiplier");
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }
    private void DrawStringField(SerializedProperty prop, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(rect, prop, new GUIContent(label));
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
    private void DrawFloatField(SerializedProperty prop, ref Rect position, string label)
    {
        DrawStringField(prop, ref position, label);
    }
    private void DrawObjectField(SerializedProperty prop, ref Rect position, string label)
    {
        DrawStringField(prop, ref position, label);
    }
    private void DrawTextAreaField(SerializedProperty prop, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(prop, true));
        EditorGUI.PropertyField(rect, prop, new GUIContent(label));
        position.y += rect.height + EditorGUIUtility.standardVerticalSpacing;
    }
    private void DrawResourceAmountList(SerializedProperty listProp, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        listProp.isExpanded = EditorGUI.Foldout(rect, listProp.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (listProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                SerializedProperty resProp = elem.FindPropertyRelative("resource");
                string currName = resProp.FindPropertyRelative("displayName").stringValue;
                Rect elemRect = new Rect(position.x, position.y, position.width - 120, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(elemRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
                Rect btnRect = new Rect(elemRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(btnRect, "Select"))
                {
                    ShowResourcePicker(btnRect, elem);
                }
                Rect amountRect = new Rect(btnRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(amountRect, elem.FindPropertyRelative("amount"), GUIContent.none);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            Rect addRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "Add Cost"))
            {
                listProp.arraySize++;
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel--;
        }
    }
    private void ShowResourcePicker(Rect activatorRect, SerializedProperty amountProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.resources == null || db.resources.resources == null)
        {
            EditorUtility.DisplayDialog("No Resources", "ResourcesDatabase missing or empty.", "OK");
            return;
        }
        var list = db.resources.resources;
        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((ResourceDefinition)r).displayName,
            getCategory: r => ((ResourceDefinition)r).category.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;
                SerializedProperty resProp = amountProp.FindPropertyRelative("resource");
                CopyResourceToProperty(resProp, (ResourceDefinition)selected);
                amountProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }
    private void CopyResourceToProperty(SerializedProperty property, ResourceDefinition source)
    {
        property.FindPropertyRelative("id").stringValue = source.id;
        property.FindPropertyRelative("displayName").stringValue = source.displayName;
        property.FindPropertyRelative("icon").objectReferenceValue = source.icon;
        property.FindPropertyRelative("category").enumValueIndex = (int)source.category;
        property.FindPropertyRelative("weight").floatValue = source.weight;
        property.FindPropertyRelative("baseValue").intValue = source.baseValue;
    }
    private void DrawPrerequisiteList(SerializedProperty listProp, ref Rect position, string label, string currentId)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        listProp.isExpanded = EditorGUI.Foldout(rect, listProp.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (listProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                string currName = elem.FindPropertyRelative("displayName").stringValue;
                Rect elemRect = new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(elemRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
                Rect btnRect = new Rect(elemRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(btnRect, "Select"))
                {
                    ShowTechPicker(btnRect, elem, currentId);
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            Rect addRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "Add Prerequisite"))
            {
                listProp.arraySize++;
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel--;
        }
    }
    private void ShowTechPicker(Rect activatorRect, SerializedProperty techProp, string currentId)
    {
        var db = FindGameDatabase();
        if (db == null || db.techTree == null || db.techTree.techNodes == null)
        {
            EditorUtility.DisplayDialog("No Tech Nodes", "TechTreeDatabase missing or empty.", "OK");
            return;
        }
        var list = db.techTree.techNodes.Where(n => n.id != currentId).ToList();
        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((TechNodeDefinition)r).displayName,
            getCategory: r => "",
            onSelect: selected =>
            {
                if (selected == null) return;
                CopyTechToProperty(techProp, (TechNodeDefinition)selected);
                techProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }
    private void CopyTechToProperty(SerializedProperty property, TechNodeDefinition source)
    {
        property.FindPropertyRelative("id").stringValue = source.id;
        property.FindPropertyRelative("displayName").stringValue = source.displayName;
        property.FindPropertyRelative("icon").objectReferenceValue = source.icon;
        property.FindPropertyRelative("description").stringValue = source.description;
        property.FindPropertyRelative("researchTime").floatValue = source.researchTime;
        property.FindPropertyRelative("globalProductionSpeedMultiplier").floatValue = source.globalProductionSpeedMultiplier;
        property.FindPropertyRelative("globalCombatDamageMultiplier").floatValue = source.globalCombatDamageMultiplier;
        property.FindPropertyRelative("workerEfficiencyMultiplier").floatValue = source.workerEfficiencyMultiplier;
        // Clear nested lists to break depth
        property.FindPropertyRelative("researchCost").arraySize = 0;
        property.FindPropertyRelative("prerequisites").arraySize = 0;
        property.FindPropertyRelative("unlockBuildings").arraySize = 0;
        property.FindPropertyRelative("unlockUnits").arraySize = 0;
        property.FindPropertyRelative("unlockRecipes").arraySize = 0;
        property.FindPropertyRelative("requiredBuilding").objectReferenceValue = null;
    }
    private void DrawBuildingDropdown(SerializedProperty prop, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width - 110, EditorGUIUtility.singleLineHeight);
        string currName = prop.FindPropertyRelative("displayName").stringValue;
        EditorGUI.LabelField(rect, label + ": " + (string.IsNullOrEmpty(currName) ? "(None)" : currName));
        Rect btnRect = new Rect(rect.xMax + 2, position.y, 110, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(btnRect, "Select"))
        {
            ShowBuildingPicker(btnRect, prop);
        }
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }
    private void ShowBuildingPicker(Rect activatorRect, SerializedProperty buildingProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.buildings == null || db.buildings.buildings == null)
        {
            EditorUtility.DisplayDialog("No Buildings", "BuildingsDatabase missing or empty.", "OK");
            return;
        }
        var list = db.buildings.buildings;
        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((BuildingDefinition)r).displayName,
            getCategory: r => ((BuildingDefinition)r).category.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;
                CopyBuildingToProperty(buildingProp, (BuildingDefinition)selected);
                buildingProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }
    private void CopyBuildingToProperty(SerializedProperty property, BuildingDefinition source)
    {
        property.FindPropertyRelative("id").stringValue = source.id;
        property.FindPropertyRelative("displayName").stringValue = source.displayName;
        property.FindPropertyRelative("icon").objectReferenceValue = source.icon;
        property.FindPropertyRelative("prefab").objectReferenceValue = source.prefab;
        property.FindPropertyRelative("category").enumValueIndex = (int)source.category;
        property.FindPropertyRelative("subCategory").stringValue = source.subCategory;
        property.FindPropertyRelative("buildTime").floatValue = source.buildTime;
        property.FindPropertyRelative("maxHealth").intValue = source.maxHealth;
        property.FindPropertyRelative("upkeepInterval").floatValue = source.upkeepInterval;
        property.FindPropertyRelative("maxResidents").intValue = source.maxResidents;
        property.FindPropertyRelative("comfortLevel").intValue = source.comfortLevel;
        property.FindPropertyRelative("isProducer").boolValue = source.isProducer;
        property.FindPropertyRelative("workerSlots").intValue = source.workerSlots;
        property.FindPropertyRelative("isStorage").boolValue = source.isStorage;
        property.FindPropertyRelative("isMilitary").boolValue = source.isMilitary;
        property.FindPropertyRelative("attackDamage").intValue = source.attackDamage;
        property.FindPropertyRelative("attackRange").floatValue = source.attackRange;
        property.FindPropertyRelative("garrisonSlots").intValue = source.garrisonSlots;
        property.FindPropertyRelative("powerConsumption").intValue = source.powerConsumption;
        property.FindPropertyRelative("powerGeneration").intValue = source.powerGeneration;
        property.FindPropertyRelative("waterConsumption").intValue = source.waterConsumption;
        // Clear nested to break depth
        property.FindPropertyRelative("constructionCost").arraySize = 0;
        property.FindPropertyRelative("upkeepCost").arraySize = 0;
        property.FindPropertyRelative("recipes").arraySize = 0;
        property.FindPropertyRelative("storageSettings").FindPropertyRelative("capacity").intValue = 0;
        property.FindPropertyRelative("storageSettings").FindPropertyRelative("allowedCategories").arraySize = 0;
        property.FindPropertyRelative("upgradeTo").objectReferenceValue = null;
    }
    private void DrawUnlockBuildingsList(SerializedProperty listProp, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        listProp.isExpanded = EditorGUI.Foldout(rect, listProp.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (listProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                string currName = elem.FindPropertyRelative("displayName").stringValue;
                Rect elemRect = new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(elemRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
                Rect btnRect = new Rect(elemRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(btnRect, "Select"))
                {
                    ShowBuildingPicker(btnRect, elem);
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            Rect addRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "Add Building"))
            {
                listProp.arraySize++;
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel--;
        }
    }
    private void DrawUnlockUnitsList(SerializedProperty listProp, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        listProp.isExpanded = EditorGUI.Foldout(rect, listProp.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (listProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                string currName = elem.FindPropertyRelative("displayName").stringValue;
                Rect elemRect = new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(elemRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
                Rect btnRect = new Rect(elemRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(btnRect, "Select"))
                {
                    ShowUnitPicker(btnRect, elem);
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            Rect addRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "Add Unit"))
            {
                listProp.arraySize++;
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel--;
        }
    }
    private void ShowUnitPicker(Rect activatorRect, SerializedProperty unitProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.units == null || db.units.units == null)
        {
            EditorUtility.DisplayDialog("No Units", "UnitsDatabase missing or empty.", "OK");
            return;
        }
        var list = db.units.units;
        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((UnitDefinition)r).displayName,
            getCategory: r => ((UnitDefinition)r).jobType.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;
                CopyUnitToProperty(unitProp, (UnitDefinition)selected);
                unitProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }
    private void CopyUnitToProperty(SerializedProperty property, UnitDefinition source)
    {
        property.FindPropertyRelative("id").stringValue = source.id;
        property.FindPropertyRelative("displayName").stringValue = source.displayName;
        property.FindPropertyRelative("icon").objectReferenceValue = source.icon;
        property.FindPropertyRelative("prefab").objectReferenceValue = source.prefab;
        property.FindPropertyRelative("jobType").enumValueIndex = (int)source.jobType;
        property.FindPropertyRelative("isCombatUnit").boolValue = source.isCombatUnit;
        property.FindPropertyRelative("maxHealth").intValue = source.maxHealth;
        property.FindPropertyRelative("moveSpeed").floatValue = source.moveSpeed;
        property.FindPropertyRelative("attackDamage").intValue = source.attackDamage;
        property.FindPropertyRelative("attackRange").floatValue = source.attackRange;
        property.FindPropertyRelative("attackCooldown").floatValue = source.attackCooldown;
        property.FindPropertyRelative("armor").intValue = source.armor;
        property.FindPropertyRelative("carryCapacity").intValue = source.carryCapacity;
        property.FindPropertyRelative("gatherSpeed").floatValue = source.gatherSpeed;
        property.FindPropertyRelative("buildSpeed").floatValue = source.buildSpeed;
        property.FindPropertyRelative("trainingTime").floatValue = source.trainingTime;
        // Clear nested
        property.FindPropertyRelative("trainingCost").arraySize = 0;
        property.FindPropertyRelative("startingTools").arraySize = 0;
        property.FindPropertyRelative("trainedAt").objectReferenceValue = null;
        property.FindPropertyRelative("upgradeTo").objectReferenceValue = null;
    }
    private void DrawUnlockRecipesList(SerializedProperty listProp, ref Rect position, string label)
    {
        Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        listProp.isExpanded = EditorGUI.Foldout(rect, listProp.isExpanded, label, true);
        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        if (listProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty elem = listProp.GetArrayElementAtIndex(i);
                string currName = elem.FindPropertyRelative("recipeName").stringValue;
                Rect elemRect = new Rect(position.x, position.y, position.width - 60, EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(elemRect, string.IsNullOrEmpty(currName) ? "(None)" : currName);
                Rect btnRect = new Rect(elemRect.xMax + 2, position.y, 60, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(btnRect, "Select"))
                {
                    ShowRecipePicker(btnRect, elem);
                }
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            Rect addRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(addRect, "Add Recipe"))
            {
                listProp.arraySize++;
            }
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel--;
        }
    }
    private void ShowRecipePicker(Rect activatorRect, SerializedProperty recipeProp)
    {
        var db = FindGameDatabase();
        if (db == null || db.recipes == null || db.recipes.recipes == null)
        {
            EditorUtility.DisplayDialog("No Recipes", "RecipesDatabase missing or empty.", "OK");
            return;
        }
        var list = db.recipes.recipes;
        SearchablePickerPopup.Show(
            activatorRect,
            list.Cast<object>().ToList(),
            getName: r => ((ProductionRecipeDefinition)r).recipeName,
            getCategory: r => ((ProductionRecipeDefinition)r).requiredJobType.ToString(),
            onSelect: selected =>
            {
                if (selected == null) return;
                CopyRecipeToProperty(recipeProp, (ProductionRecipeDefinition)selected);
                recipeProp.serializedObject.ApplyModifiedProperties();
            }
        );
    }
    private void CopyRecipeToProperty(SerializedProperty property, ProductionRecipeDefinition source)
    {
        property.FindPropertyRelative("recipeName").stringValue = source.recipeName;
        property.FindPropertyRelative("craftTimeSeconds").floatValue = source.craftTimeSeconds;
        property.FindPropertyRelative("batchSize").intValue = source.batchSize;
        property.FindPropertyRelative("requiredJobType").enumValueIndex = (int)source.requiredJobType;
        property.FindPropertyRelative("outputEfficiencyMultiplier").floatValue = source.outputEfficiencyMultiplier;
        property.FindPropertyRelative("inputEfficiencyMultiplier").floatValue = source.inputEfficiencyMultiplier;
        property.FindPropertyRelative("requiresPower").boolValue = source.requiresPower;
        property.FindPropertyRelative("requiresFuel").boolValue = source.requiresFuel;
        // Clear nested arrays to break depth
        property.FindPropertyRelative("inputs").arraySize = 0;
        property.FindPropertyRelative("outputs").arraySize = 0;
    }
    private GameDatabase FindGameDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
}