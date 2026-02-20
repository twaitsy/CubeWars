using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UnitDefinitionIdAttribute))]
public class UnitDefinitionIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        UnitsDatabase unitsDatabase = ResolveUnitsDatabase();
        if (unitsDatabase == null || unitsDatabase.units == null || unitsDatabase.units.Count == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string currentId = property.stringValue ?? string.Empty;
        int selectedIndex = 0;
        string[] options = new string[unitsDatabase.units.Count + 1];
        options[0] = "(None)";

        for (int i = 0; i < unitsDatabase.units.Count; i++)
        {
            UnitDefinition definition = unitsDatabase.units[i];
            string display = definition == null
                ? "(Missing Unit Definition)"
                : $"{definition.displayName} [{definition.id}]";
            options[i + 1] = display;

            if (definition != null && definition.id == currentId)
                selectedIndex = i + 1;
        }

        int nextIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
        if (nextIndex == selectedIndex)
            return;

        property.stringValue = nextIndex <= 0 || unitsDatabase.units[nextIndex - 1] == null
            ? string.Empty
            : unitsDatabase.units[nextIndex - 1].id;
    }

    static UnitsDatabase ResolveUnitsDatabase()
    {
        GameDatabase loaded = GameDatabaseLoader.Loaded;
        if (loaded != null && loaded.units != null)
            return loaded.units;

        string[] gameDatabaseGuids = AssetDatabase.FindAssets("t:GameDatabase");
        for (int i = 0; i < gameDatabaseGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(gameDatabaseGuids[i]);
            GameDatabase database = AssetDatabase.LoadAssetAtPath<GameDatabase>(path);
            if (database != null && database.units != null)
                return database.units;
        }

        string[] unitsGuids = AssetDatabase.FindAssets("t:UnitsDatabase");
        for (int i = 0; i < unitsGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(unitsGuids[i]);
            UnitsDatabase units = AssetDatabase.LoadAssetAtPath<UnitsDatabase>(path);
            if (units != null)
                return units;
        }

        return null;
    }
}
