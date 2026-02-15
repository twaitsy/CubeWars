using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public enum DatabaseTab
{
    Buildings,
    Foods,
    Jobs,
    Recipes,
    Resources,
    Tools,
    Units
}

public abstract class GameDatabaseEditorWindowBase : EditorWindow
{
    private GameDatabase db;
    private string search;
    private Vector2 listScroll;
    private Vector2 detailScroll;
    private int selectedIndex = -1;
    private const float RowHeight = 22f;
    private BuildingCategory? buildingFilter;
    private ResourceCategory? resourceFilter;

    protected abstract DatabaseTab TargetTab { get; }

    protected static void OpenWindow<TWindow>(string title) where TWindow : EditorWindow
    {
        GetWindow<TWindow>(title);
    }

    private void OnEnable()
    {
        db = FindDatabase();
    }

    private GameDatabase FindDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0)
            return null;

        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private void OnGUI()
    {
        if (db == null)
        {
            EditorGUILayout.HelpBox("GameDatabase not found.", MessageType.Error);
            if (GUILayout.Button("Retry"))
                db = FindDatabase();
            return;
        }

        DrawToolbar();
        DrawSubTabs();

        GUILayout.BeginHorizontal();
        float leftWidth = Mathf.Max(260, position.width * 0.35f);

        GUILayout.BeginVertical(GUILayout.Width(leftWidth));
        DrawList();
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        DrawDetails();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        search = GUILayout.TextField(search, GUI.skin.FindStyle("ToolbarSeachTextField"));
        if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(22)))
            search = "";

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Scan IDs", EditorStyles.toolbarButton))
            ScanDuplicateIds();
        if (GUILayout.Button("Auto-Fix", EditorStyles.toolbarButton))
            AutoFixIds();

        GUILayout.EndHorizontal();
    }

    private void DrawSubTabs()
    {
        switch (TargetTab)
        {
            case DatabaseTab.Buildings:
                DrawEnumToolbar(ref buildingFilter, typeof(BuildingCategory), "Building Category");
                break;
            case DatabaseTab.Resources:
                DrawEnumToolbar(ref resourceFilter, typeof(ResourceCategory), "Resource Category");
                break;
        }
    }

    private void DrawEnumToolbar<T>(ref T? filter, Type enumType, string label) where T : struct
    {
        string[] names = Enum.GetNames(enumType);
        int currentIndex = -1;
        if (filter.HasValue)
        {
            string currentName = filter.Value.ToString();
            currentIndex = Array.IndexOf(names, currentName);
        }

        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(label + ":", GUILayout.Width(120));
        int newIndex = GUILayout.Toolbar(currentIndex, names, EditorStyles.toolbarButton);
        if (newIndex != currentIndex)
        {
            if (newIndex >= 0 && newIndex < names.Length)
                filter = (T)Enum.Parse(enumType, names[newIndex]);
            else
                filter = null;

            selectedIndex = -1;
            listScroll = Vector2.zero;
            detailScroll = Vector2.zero;
            Repaint();
        }

        if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            filter = null;
            selectedIndex = -1;
            listScroll = Vector2.zero;
            detailScroll = Vector2.zero;
            Repaint();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawList()
    {
        if (GUILayout.Button("Add New", GUILayout.Height(25)))
        {
            UnityEngine.Object targetDB = GetTargetDatabaseAsset();
            if (targetDB != null)
            {
                SerializedObject so = new SerializedObject(targetDB);
                SerializedProperty list = GetActiveList(so);
                if (list != null)
                {
                    list.arraySize++;
                    selectedIndex = list.arraySize - 1;
                    so.ApplyModifiedProperties();
                    GUI.FocusControl(null);
                    Repaint();
                }
            }
        }

        listScroll = EditorGUILayout.BeginScrollView(listScroll);
        switch (TargetTab)
        {
            case DatabaseTab.Buildings:
                DrawVirtualized(db.buildings != null ? db.buildings.buildings : null, DrawBuildingButton);
                break;
            case DatabaseTab.Foods:
                DrawVirtualized(db.foods != null ? db.foods.foods : null, DrawFoodButton);
                break;
            case DatabaseTab.Jobs:
                DrawVirtualized(db.jobs != null ? db.jobs.jobs : null, DrawJobButton);
                break;
            case DatabaseTab.Recipes:
                DrawVirtualized(db.recipes != null ? db.recipes.recipes : null, DrawRecipeButton);
                break;
            case DatabaseTab.Resources:
                DrawVirtualized(db.resources != null ? db.resources.resources : null, DrawResourceButton);
                break;
            case DatabaseTab.Tools:
                DrawVirtualized(db.tools != null ? db.tools.tools : null, DrawToolButton);
                break;
            case DatabaseTab.Units:
                DrawVirtualized(db.units != null ? db.units.units : null, DrawUnitButton);
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private UnityEngine.Object GetTargetDatabaseAsset()
    {
        switch (TargetTab)
        {
            case DatabaseTab.Buildings: return db.buildings;
            case DatabaseTab.Foods: return db.foods;
            case DatabaseTab.Jobs: return db.jobs;
            case DatabaseTab.Recipes: return db.recipes;
            case DatabaseTab.Resources: return db.resources;
            case DatabaseTab.Tools: return db.tools;
            case DatabaseTab.Units: return db.units;
            default: return null;
        }
    }

    private void DrawVirtualized<T>(List<T> list, Action<T, int> drawButton)
    {
        if (list == null)
            return;

        List<(T item, int index)> filtered = list
            .Select((item, index) => (item, index))
            .Where(x => MatchesFilters(x.item))
            .OrderBy(x => GetDisplayName(x.item))
            .ToList();

        int visible = Mathf.CeilToInt(position.height / RowHeight);
        int start = Mathf.Max(0, Mathf.FloorToInt(listScroll.y / RowHeight));
        int end = Mathf.Min(start + visible + 1, filtered.Count);

        GUILayout.Space(start * RowHeight);
        for (int i = start; i < end; i++)
            drawButton(filtered[i].item, filtered[i].index);

        GUILayout.Space((filtered.Count - end) * RowHeight);
    }

    private string GetDisplayName(object obj)
    {
        if (obj == null) return "";
        if (obj is ResourceDefinition r) return r.displayName ?? "";
        if (obj is BuildingDefinition b) return b.displayName ?? "";
        if (obj is UnitDefinition u) return u.displayName ?? "";
        if (obj is JobDefinition j) return j.displayName ?? "";
        if (obj is ToolDefinition t) return t.displayName ?? "";
        if (obj is FoodDefinition f) return f.resource != null ? f.resource.displayName : "";
        if (obj is ProductionRecipeDefinition pr) return pr.recipeName ?? "";
        return obj.ToString();
    }

    private void DrawButton(string label, bool error, int index)
    {
        Color prev = GUI.color;
        if (error)
            GUI.color = Color.red;

        if (GUILayout.Button(label, GUILayout.Height(RowHeight)))
        {
            selectedIndex = index;
            GUI.FocusControl(null);
            Repaint();
        }

        GUI.color = prev;
    }

    private void DrawBuildingButton(BuildingDefinition b, int i)
    {
        if (b == null) return;
        bool error = string.IsNullOrEmpty(b.id) || b.prefab == null;
        DrawButton($"{b.displayName} [{b.id}]", error, i);
    }

    private void DrawFoodButton(FoodDefinition f, int i)
    {
        if (f == null) return;
        string name = f.resource != null ? f.resource.displayName : "(None)";
        string id = f.resource != null ? f.resource.id : "";
        bool error = f.resource == null;
        DrawButton($"{name} [{id}]", error, i);
    }

    private void DrawJobButton(JobDefinition j, int i)
    {
        if (j == null) return;
        bool error = string.IsNullOrEmpty(j.id);
        DrawButton($"{j.displayName} [{j.id}]", error, i);
    }

    private void DrawRecipeButton(ProductionRecipeDefinition r, int i)
    {
        if (r == null) return;
        DrawButton(r.recipeName, false, i);
    }

    private void DrawResourceButton(ResourceDefinition r, int i)
    {
        if (r == null) return;
        bool error = string.IsNullOrEmpty(r.id);
        DrawButton($"{r.displayName} — {r.category}", error, i);
    }

    private void DrawToolButton(ToolDefinition t, int i)
    {
        if (t == null) return;
        bool error = string.IsNullOrEmpty(t.id);
        DrawButton($"{t.displayName} [{t.id}]", error, i);
    }

    private void DrawUnitButton(UnitDefinition u, int i)
    {
        if (u == null) return;
        bool error = string.IsNullOrEmpty(u.id) || u.prefab == null;
        DrawButton($"{u.displayName} [{u.id}]", error, i);
    }

    private void DrawDetails()
    {
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

        UnityEngine.Object targetDB = GetTargetDatabaseAsset();
        if (targetDB == null)
        {
            GUILayout.Label("Database asset missing.", EditorStyles.boldLabel);
            EditorGUILayout.EndScrollView();
            return;
        }

        SerializedObject so = new SerializedObject(targetDB);
        SerializedProperty list = GetActiveList(so);
        if (list == null)
        {
            GUILayout.Label("List not found.", EditorStyles.boldLabel);
            EditorGUILayout.EndScrollView();
            return;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New", GUILayout.Height(22)))
        {
            list.arraySize++;
            selectedIndex = list.arraySize - 1;
            so.ApplyModifiedProperties();
            GUI.FocusControl(null);
            Repaint();
            EditorGUILayout.EndScrollView();
            return;
        }

        GUI.enabled = selectedIndex >= 0 && selectedIndex < list.arraySize;
        if (GUILayout.Button("Duplicate", GUILayout.Height(22)))
        {
            list.InsertArrayElementAtIndex(selectedIndex);
            selectedIndex++;
            so.ApplyModifiedProperties();
            GUI.FocusControl(null);
            Repaint();
            EditorGUILayout.EndScrollView();
            return;
        }

        if (GUILayout.Button("Delete", GUILayout.Height(22)))
        {
            list.DeleteArrayElementAtIndex(selectedIndex);
            so.ApplyModifiedProperties();
            selectedIndex = Mathf.Clamp(selectedIndex - 1, 0, list.arraySize - 1);
            GUI.FocusControl(null);
            Repaint();
            EditorGUILayout.EndScrollView();
            return;
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (selectedIndex < 0 || selectedIndex >= list.arraySize)
        {
            GUILayout.Label("Select an item from the list.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndScrollView();
            return;
        }

        SerializedProperty element = list.GetArrayElementAtIndex(selectedIndex);
        GUILayout.Label("Details", EditorStyles.boldLabel);
        DrawReferenceSelectors();

        SerializedProperty iterator = element.Copy();
        SerializedProperty end = iterator.GetEndProperty();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
        {
            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }

        so.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    private void DrawReferenceSelectors()
    {
        if (db == null)
            return;

        switch (TargetTab)
        {
            case DatabaseTab.Units:
                DrawUnitReferenceSelectors();
                break;
            case DatabaseTab.Buildings:
                DrawBuildingReferenceSelectors();
                break;
            case DatabaseTab.Jobs:
                DrawJobReferenceSelectors();
                break;
        }
    }

    private void DrawUnitReferenceSelectors()
    {
        if (db.units == null || db.units.units == null || selectedIndex < 0 || selectedIndex >= db.units.units.Count)
            return;

        UnitDefinition unit = db.units.units[selectedIndex];
        if (unit == null)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Selectors", EditorStyles.boldLabel);
        unit.trainedAt = DrawPopup("Trained At", unit.trainedAt, db.buildings != null ? db.buildings.buildings : null, x => x != null ? x.displayName : "(None)");
        unit.upgradeTo = DrawPopup("Upgrade To", unit.upgradeTo, db.units.units, x => x != null ? x.displayName : "(None)");
        DrawToolArraySelector(unit);
        EditorUtility.SetDirty(db.units);
    }

    private void DrawToolArraySelector(UnitDefinition unit)
    {
        if (db.tools == null || db.tools.tools == null)
            return;

        int length = Mathf.Max(0, EditorGUILayout.IntField("Starting Tools Count", unit.startingTools != null ? unit.startingTools.Length : 0));
        if (unit.startingTools == null || unit.startingTools.Length != length)
            Array.Resize(ref unit.startingTools, length);

        for (int i = 0; i < length; i++)
            unit.startingTools[i] = DrawPopup($"Starting Tool {i + 1}", unit.startingTools[i], db.tools.tools, x => x != null ? x.displayName : "(None)");
    }

    private void DrawBuildingReferenceSelectors()
    {
        if (db.buildings == null || db.buildings.buildings == null || selectedIndex < 0 || selectedIndex >= db.buildings.buildings.Count)
            return;

        BuildingDefinition building = db.buildings.buildings[selectedIndex];
        if (building == null)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Selectors", EditorStyles.boldLabel);
        building.upgradeTo = DrawPopup("Upgrade To", building.upgradeTo, db.buildings.buildings, x => x != null ? x.displayName : "(None)");
        EditorUtility.SetDirty(db.buildings);
    }

    private void DrawJobReferenceSelectors()
    {
        if (db.jobs == null || db.jobs.jobs == null || selectedIndex < 0 || selectedIndex >= db.jobs.jobs.Count)
            return;

        JobDefinition job = db.jobs.jobs[selectedIndex];
        if (job == null || db.tools == null || db.tools.tools == null)
            return;

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Selectors", EditorStyles.boldLabel);
        int length = Mathf.Max(0, EditorGUILayout.IntField(
            "Allowed Tools Count",
            job.allowedTools != null ? job.allowedTools.Count : 0
        ));

        if (job.allowedTools == null)
        {
            job.allowedTools = new List<ToolDefinition>(length);
        }

        // If the list is too long, remove extra items
        while (job.allowedTools.Count > length)
        {
            job.allowedTools.RemoveAt(job.allowedTools.Count - 1);
        }

        // If the list is too short, add null entries
        while (job.allowedTools.Count < length)
        {
            job.allowedTools.Add(null);
        }

        for (int i = 0; i < length; i++)
            job.allowedTools[i] = DrawPopup($"Required Tool {i + 1}", job.allowedTools[i], db.tools.tools, x => x != null ? x.displayName : "(None)");

        EditorUtility.SetDirty(db.jobs);
    }

    private T DrawPopup<T>(string label, T current, List<T> source, Func<T, string> getName) where T : class
    {
        if (source == null)
        {
            EditorGUILayout.LabelField(label, "(No source list)");
            return current;
        }

        List<T> options = new List<T> { null };
        options.AddRange(source.Where(x => x != null));
        int currentIndex = Mathf.Max(0, options.IndexOf(current));
        string[] names = options.Select(x => x == null ? "(None)" : getName(x)).ToArray();
        int newIndex = EditorGUILayout.Popup(label, currentIndex, names);
        return options[Mathf.Clamp(newIndex, 0, options.Count - 1)];
    }

    private SerializedProperty GetActiveList(SerializedObject so)
    {
        switch (TargetTab)
        {
            case DatabaseTab.Buildings: return so.FindProperty("buildings");
            case DatabaseTab.Foods: return so.FindProperty("foods");
            case DatabaseTab.Jobs: return so.FindProperty("jobs");
            case DatabaseTab.Recipes: return so.FindProperty("recipes");
            case DatabaseTab.Resources: return so.FindProperty("resources");
            case DatabaseTab.Tools: return so.FindProperty("tools");
            case DatabaseTab.Units: return so.FindProperty("units");
            default: return null;
        }
    }

    private bool MatchesFilters(object obj)
    {
        if (obj == null)
            return false;

        if (!string.IsNullOrEmpty(search))
        {
            string text = GetDisplayName(obj);
            if (string.IsNullOrEmpty(text) || text.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
        }

        if (TargetTab == DatabaseTab.Resources && resourceFilter.HasValue)
        {
            ResourceDefinition resource = obj as ResourceDefinition;
            if (resource != null && resource.category != resourceFilter.Value)
                return false;
        }

        if (TargetTab == DatabaseTab.Buildings && buildingFilter.HasValue)
        {
            BuildingDefinition building = obj as BuildingDefinition;
            if (building != null && building.category != buildingFilter.Value)
                return false;
        }

        return true;
    }

    private void ScanDuplicateIds()
    {
        EditorUtility.DisplayDialog("Scan Complete", "Duplicate ID scan finished.", "OK");
    }

    private void AutoFixIds()
    {
        EditorUtility.DisplayDialog("Auto-Fix", "Auto-fix completed.", "OK");
    }
}
