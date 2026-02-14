using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SearchablePickerPopup : EditorWindow
{
    string search = "";
    Vector2 scroll;
    List<object> items;
    Func<object, string> getName;
    Func<object, string> getCategory;
    Action<object> onSelect;
    readonly Dictionary<string, List<object>> grouped = new Dictionary<string, List<object>>();

    public static void Show(
        Rect activatorRect,
        List<object> items,
        Func<object, string> getName,
        Func<object, string> getCategory,
        Action<object> onSelect)
    {
        var window = CreateInstance<SearchablePickerPopup>();
        if (window == null)
        {
            Debug.LogError("Failed to create SearchablePickerPopup instance.");
            return;
        }

        // Hard-guard everything
        window.items = items ?? new List<object>();
        window.getName = getName ?? (x => x != null ? x.ToString() : "");
        window.getCategory = getCategory ?? (x => "");
        window.onSelect = onSelect ?? (_ => { });
        window.BuildGroups();

        window.ShowAsDropDown(activatorRect, new Vector2(350, 400));
    }

    void BuildGroups()
    {
        grouped.Clear();
        IEnumerable<object> filtered = items ?? Enumerable.Empty<object>();
        if (!string.IsNullOrEmpty(search))
        {
            string s = search.Trim().ToLowerInvariant();
            filtered = filtered.Where(i =>
            {
                if (i == null) return false;
                string n = SafeName(i);
                string c = SafeCategory(i);
                return n.Contains(s) || c.Contains(s);
            });
        }
        foreach (var item in filtered)
        {
            if (item == null)
                continue;
            string cat = SafeCategory(item);
            if (string.IsNullOrEmpty(cat)) cat = "(Uncategorized)";
            if (!grouped.TryGetValue(cat, out var list))
            {
                list = new List<object>();
                grouped[cat] = list;
            }
            list.Add(item);
        }
        foreach (var kvp in grouped.ToList())
        {
            grouped[kvp.Key] = kvp.Value
                .Where(i => i != null)
                .OrderBy(SafeName)
                .ToList();
        }
    }

    string SafeName(object item)
    {
        if (item == null) return "";
        try { return (getName?.Invoke(item) ?? "").ToLowerInvariant(); }
        catch { return ""; }
    }

    string SafeCategory(object item)
    {
        if (item == null) return "";
        try { return (getCategory?.Invoke(item) ?? "").ToLowerInvariant(); }
        catch { return ""; }
    }

    void OnGUI()
    {
        DrawSearchBar();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var kvp in grouped.OrderBy(g => g.Key))
        {
            GUILayout.Label(kvp.Key, EditorStyles.boldLabel);
            foreach (var item in kvp.Value)
            {
                if (item == null) continue;
                string label = $"{getName?.Invoke(item) ?? ""} — {getCategory?.Invoke(item) ?? ""}";
                if (GUILayout.Button(label, GUILayout.Height(20)))
                {
                    onSelect?.Invoke(item);
                    Close();
                }
            }
            GUILayout.Space(4);
        }
        EditorGUILayout.EndScrollView();
    }

    void DrawSearchBar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        string newSearch = GUILayout.TextField(
            search,
            GUI.skin.FindStyle("ToolbarSearchTextField") ?? EditorStyles.toolbarTextField
        );
        if (newSearch != search)
        {
            search = newSearch;
            BuildGroups();
            Repaint();
        }
        if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(22)))
        {
            search = "";
            BuildGroups();
            Repaint();
        }
        GUILayout.EndHorizontal();
    }
}