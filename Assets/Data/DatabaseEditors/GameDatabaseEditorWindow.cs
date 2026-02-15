using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameDatabaseEditorWindow : EditorWindow
{
    private GameDatabase db;
    private enum Tab
    {
        Buildings,
        Foods,
        Jobs,
        Recipes,
        Resources,
        Tools,
        Units
    }
    private Tab currentTab;
    private string search;
    private Vector2 listScroll;
    private Vector2 detailScroll;
    private int selectedIndex = -1;
    private const float RowHeight = 22f;
    private BuildingCategory? buildingFilter = null;
    private ResourceCategory? resourceFilter = null;

    [MenuItem("Tools/CubeWars/Database/Open Game Database Editor")]
    public static void Open()
    {
        GetWindow<GameDatabaseEditorWindow>("Game Database");
    }

    private void OnEnable()
    {
        db = FindDatabase();
    }

    private GameDatabase FindDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:GameDatabase");
        if (guids.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private void OnGUI()
    {
        if (db == null)
        {
            EditorGUILayout.HelpBox("GameDatabase not found.", MessageType.Error);
            if (GUILayout.Button("Retry")) db = FindDatabase();
            return;
        }
        DrawToolbar();
        DrawTabs();
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

    private void DrawTabs()
    {
        string[] labels =
        {
            "Buildings","Foods","Jobs","Recipes",
            "Resources","Tools","Units"
        };
        Tab newTab = (Tab)GUILayout.Toolbar((int)currentTab, labels);
        if (newTab != currentTab)
        {
            currentTab = newTab;
            selectedIndex = -1;
            listScroll = Vector2.zero;
            detailScroll = Vector2.zero;
        }
    }

    private void DrawSubTabs()
    {
        switch (currentTab)
        {
            case Tab.Buildings:
                DrawEnumToolbar(ref buildingFilter, typeof(BuildingCategory), "Building Category");
                break;
            case Tab.Resources:
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

    void DrawList()
    {
        // ADD NEW BUTTON at the top of the left panel
        if (GUILayout.Button("Add New", GUILayout.Height(25)))
        {
            UnityEngine.Object targetDB = null;
            switch (currentTab)
            {
                case Tab.Buildings: targetDB = db.buildings; break;
                case Tab.Foods: targetDB = db.foods; break;
                case Tab.Jobs: targetDB = db.jobs; break;
                case Tab.Recipes: targetDB = db.recipes; break;
                case Tab.Resources: targetDB = db.resources; break;
                case Tab.Tools: targetDB = db.tools; break;
                case Tab.Units: targetDB = db.units; break;
            }
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
        // Now draw the scrollable list
        listScroll = EditorGUILayout.BeginScrollView(listScroll);
        switch (currentTab)
        {
            case Tab.Buildings:
                DrawVirtualized(db.buildings != null ? db.buildings.buildings : null, DrawBuildingButton);
                break;
            case Tab.Foods:
                DrawVirtualized(db.foods != null ? db.foods.foods : null, DrawFoodButton);
                break;
            case Tab.Jobs:
                DrawVirtualized(db.jobs != null ? db.jobs.jobs : null, DrawJobButton);
                break;
            case Tab.Recipes:
                DrawVirtualized(db.recipes != null ? db.recipes.recipes : null, DrawRecipeButton);
                break;
            case Tab.Resources:
                DrawVirtualized(db.resources != null ? db.resources.resources : null, DrawResourceButton);
                break;
            case Tab.Tools:
                DrawVirtualized(db.tools != null ? db.tools.tools : null, DrawToolButton);
                break;
            case Tab.Units:
                DrawVirtualized(db.units != null ? db.units.units : null, DrawUnitButton);
                break;
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawVirtualized<T>(List<T> list, Action<T, int> drawButton)
    {
        if (list == null) return;
        var filtered = list
            .Select((item, index) => new { item, index })
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
        if (error) GUI.color = Color.red;
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
        string label = string.Format("{0} [{1}]", b.displayName, b.id);
        DrawButton(label, error, i);
    }

    private void DrawFoodButton(FoodDefinition f, int i)
    {
        if (f == null) return;
        string name = f.resource != null ? f.resource.displayName : "(None)";
        string id = f.resource != null ? f.resource.id : "";
        bool error = f.resource == null;
        string label = string.Format("{0} [{1}]", name, id);
        DrawButton(label, error, i);
    }

    private void DrawJobButton(JobDefinition j, int i)
    {
        if (j == null) return;
        bool error = string.IsNullOrEmpty(j.id);
        string label = string.Format("{0} [{1}]", j.displayName, j.id);
        DrawButton(label, error, i);
    }

    private void DrawRecipeButton(ProductionRecipeDefinition r, int i)
    {
        if (r == null) return;
        string label = r.recipeName;
        DrawButton(label, false, i);
    }

    private void DrawResourceButton(ResourceDefinition r, int i)
    {
        if (r == null) return;
        bool error = string.IsNullOrEmpty(r.id);
        string label = string.Format("{0} — {1}", r.displayName, r.category);
        DrawButton(label, error, i);
    }

    private void DrawToolButton(ToolDefinition t, int i)
    {
        if (t == null) return;
        bool error = string.IsNullOrEmpty(t.id);
        string label = string.Format("{0} [{1}]", t.displayName, t.id);
        DrawButton(label, error, i);
    }

    private void DrawUnitButton(UnitDefinition u, int i)
    {
        if (u == null) return;
        bool error = string.IsNullOrEmpty(u.id) || u.prefab == null;
        string label = string.Format("{0} [{1}]", u.displayName, u.id);
        DrawButton(label, error, i);
    }

    void DrawDetails()
    {
        detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
        // Determine which database asset we are editing
        UnityEngine.Object targetDB = null;
        switch (currentTab)
        {
            case Tab.Buildings: targetDB = db.buildings; break;
            case Tab.Foods: targetDB = db.foods; break;
            case Tab.Jobs: targetDB = db.jobs; break;
            case Tab.Recipes: targetDB = db.recipes; break;
            case Tab.Resources: targetDB = db.resources; break;
            case Tab.Tools: targetDB = db.tools; break;
            case Tab.Units: targetDB = db.units; break;
        }
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
        // BUTTON BAR
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
        // If nothing selected yet
        if (selectedIndex < 0 || selectedIndex >= list.arraySize)
        {
            GUILayout.Label("Select an item from the list.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndScrollView();
            return;
        }
        // ⭐ Draw the selected element WITHOUT foldout
        SerializedProperty element = list.GetArrayElementAtIndex(selectedIndex);
        GUILayout.Label("Details", EditorStyles.boldLabel);
        DrawReferenceSelectors();
        EditorGUI.indentLevel = 0;
        // ⭐ MANUAL CHILD PROPERTY ITERATION (no foldout ever)
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
        switch (currentTab)
        {
            case Tab.Units:
                DrawUnitReferenceSelectors();
                break;
            case Tab.Buildings:
                DrawBuildingReferenceSelectors();
                break;
            case Tab.Jobs:
                DrawJobReferenceSelectors();
                break;
        }
    }

    private void DrawUnitReferenceSelectors()
    {
        if (db.units == null || db.units.units == null || selectedIndex < 0 || selectedIndex >= db.units.units.Count)
            return;
        var unit = db.units.units[selectedIndex];
        if (unit == null) return;
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
        var building = db.buildings.buildings[selectedIndex];
        if (building == null) return;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Selectors", EditorStyles.boldLabel);
        building.upgradeTo = DrawPopup("Upgrade To", building.upgradeTo, db.buildings.buildings, x => x != null ? x.displayName : "(None)");
        EditorUtility.SetDirty(db.buildings);
    }

    private void DrawJobReferenceSelectors()
    {
        if (db.jobs == null || db.jobs.jobs == null || selectedIndex < 0 || selectedIndex >= db.jobs.jobs.Count)
            return;
        var job = db.jobs.jobs[selectedIndex];
        if (job == null || db.tools == null || db.tools.tools == null) return;
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Quick Selectors", EditorStyles.boldLabel);
        int length = Mathf.Max(0, EditorGUILayout.IntField("Required Tools Count", job.requiredTools != null ? job.requiredTools.Length : 0));
        if (job.requiredTools == null || job.requiredTools.Length != length)
            Array.Resize(ref job.requiredTools, length);
        for (int i = 0; i < length; i++)
            job.requiredTools[i] = DrawPopup($"Required Tool {i + 1}", job.requiredTools[i], db.tools.tools, x => x != null ? x.displayName : "(None)");
        EditorUtility.SetDirty(db.jobs);
    }

    private T DrawPopup<T>(string label, T current, List<T> source, Func<T, string> getName) where T : class
    {
        if (source == null)
        {
            EditorGUILayout.LabelField(label, "(No source list)");
            return current;
        }
        var options = new List<T> { null };
        options.AddRange(source.Where(x => x != null));
        int currentIndex = Mathf.Max(0, options.IndexOf(current));
        string[] names = options.Select(x => x == null ? "(None)" : getName(x)).ToArray();
        int newIndex = EditorGUILayout.Popup(label, currentIndex, names);
        return options[Mathf.Clamp(newIndex, 0, options.Count - 1)];
    }

    private SerializedProperty GetActiveList(SerializedObject so)
    {
        switch (currentTab)
        {
            case Tab.Buildings: return so.FindProperty("buildings");
            case Tab.Foods: return so.FindProperty("foods");
            case Tab.Jobs: return so.FindProperty("jobs");
            case Tab.Recipes: return so.FindProperty("recipes");
            case Tab.Resources: return so.FindProperty("resources");
            case Tab.Tools: return so.FindProperty("tools");
            case Tab.Units: return so.FindProperty("units");
        }
        return null;
    }

    private bool MatchesFilters(object obj)
    {
        if (obj == null) return false;
        if (!string.IsNullOrEmpty(search))
        {
            string text = GetDisplayName(obj);
            if (string.IsNullOrEmpty(text) ||
                text.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                return false;
        }
        if (currentTab == Tab.Resources && resourceFilter.HasValue)
        {
            ResourceDefinition r = obj as ResourceDefinition;
            if (r != null && r.category != resourceFilter.Value)
                return false;
        }
        if (currentTab == Tab.Buildings && buildingFilter.HasValue)
        {
            BuildingDefinition b = obj as BuildingDefinition;
            if (b != null && b.category != buildingFilter.Value)
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