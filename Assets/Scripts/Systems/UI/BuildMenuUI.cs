// ============================================================================
// BuildMenuUI.cs
//
// PURPOSE:
// - Provides the entire in-game Build Menu UI.
// - Displays categories, items, costs, search, and affordability.
// - Syncs visibility with the build grid.
// - Allows the player to select a BuildItemDefinition for placement.
//
// DEPENDENCIES:
// - BuildItemDefinition:
//      * Provides displayName, icon, costs, category.
// - BuildingsDatabase (optional but preferred):
//      * Used as primary source when autoDiscover is on.
//      * If missing, falls back to Resources discovery.
// - BuildPlacementManager:
//      * Receives SetSelected(item) when the player clicks a build button.
//      * Clears selection when menu closes.
// - BuildGridManager (indirect):
//      * Visibility sync via TrySyncGrid().
// - TeamStorageManager / TeamResources:
//      * Used to check affordability (CanAffordAvailable / CanAfford).
// - IMGUIInputBlocker:
//      * Prevents clicks through UI.
// - Resources folder:
//      * Auto-discovery fallback loads BuildItemDefinition assets from Resources/BuildItems.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add a new UI system (Unity UI Toolkit, uGUI), this script may be replaced.
// - If you add hotkeys for categories or items, integrate them in Update().
// - If you add multi-page categories, extend the tab system.
// - If you add building previews in UI, integrate with BuildPlacementManager.ShowPreviewAt().
// - If you add tech-tree requirements, filter items based on unlock state.
// - If you add AI debugging tools, expose category and item lists.
//
// INSPECTOR REQUIREMENTS:
// - playerTeamID: used for affordability checks.
// - toggleKey: key to open/close the build menu.
// - resourcesPath: folder for auto-discovery.
// - categories: used when autoDiscover = false.
// - showCosts / showAffordability: UI toggles.
// - trySyncBuildGridVisibility: syncs grid visibility with menu.
// ============================================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BuildMenuUI : MonoBehaviour
{
    class RuntimeBuildEntry
    {
        public BuildItemDefinition item;
        public BuildingDefinition detailedDefinition;
    }

    [Serializable]
    public class BuildCategory
    {
        public string name = "Category";
        public Texture2D icon;
        public BuildItemDefinition[] items;
    }

    [Header("Player")]
    public int playerTeamID = 0;

    [Header("Toggle")]
    public bool show = false;
    public KeyCode toggleKey = KeyCode.B;

    [Header("Panel Layout (Top Right)")]
    public int panelWidth = 460;
    public int panelHeight = 420;
    public int marginRight = 12;
    public int marginTop = 12;

    [Header("Catalog Source")]
    public bool autoDiscover = true;
    public string resourcesPath = "BuildItems";
    [Tooltip("Preferred source for build items. If assigned, this takes priority over Resources path discovery.")]
    public BuildingsDatabase buildingsDatabase;
    [Tooltip("Editor-only path used to auto-assign the database when not explicitly assigned.")]
    public string buildingsDatabaseAssetPath = "Assets/Data/Databases/BuildingsDatabase.asset";
    public bool useManualCatalogWhenAutoDiscoverOff = true;

    [Header("Manual Catalog (optional)")]
    public BuildCategory[] categories;

    [Header("Optional Sync With Grid")]
    public bool trySyncBuildGridVisibility = true;

    [Header("UI")]
    public bool showCosts = true;
    public bool showAffordability = true;

    private int selectedCategoryIndex = 0;
    private Vector2 scroll;
    private string search = "";
    private readonly StringBuilder sb = new StringBuilder(256);

    private readonly StringBuilder costSb = new StringBuilder(128);
    private readonly StringBuilder detailsSb = new StringBuilder(96);
    private readonly List<BuildItemDefinition> runtimeGeneratedItems = new List<BuildItemDefinition>();
    private readonly Dictionary<BuildItemDefinition, BuildingDefinition> detailedByItem = new Dictionary<BuildItemDefinition, BuildingDefinition>();

    public bool IsVisible => show;

    public void SetVisible(bool visible)
    {
        show = visible;

        if (!show)
        {
            if (BuildPlacementManager.Instance != null)
                BuildPlacementManager.Instance.SetSelected(null);
        }

        if (trySyncBuildGridVisibility)
            TrySyncGrid(show);
    }

    public void ToggleVisible() => SetVisible(!show);

    public void RebuildCatalog()
    {
        if (autoDiscover)
        {
            categories = DiscoverCatalog();
            selectedCategoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, Mathf.Max(0, categories.Length - 1));
            scroll = Vector2.zero;
        }
    }

    void Start()
    {
        AutoAssignPlayerTeam();
        if (autoDiscover) categories = DiscoverCatalog();
        else if (!useManualCatalogWhenAutoDiscoverOff) categories = new BuildCategory[0];

        selectedCategoryIndex = (categories != null && categories.Length > 0)
            ? Mathf.Clamp(selectedCategoryIndex, 0, categories.Length - 1)
            : 0;

        if (trySyncBuildGridVisibility)
            TrySyncGrid(show);
    }

    BuildCategory[] DiscoverCatalog()
    {
        BuildingsDatabase db = ResolveBuildingsDatabase();
        if (db != null)
            return DiscoverFromDatabase(db);

        return DiscoverFromResources(resourcesPath);
    }

    BuildingsDatabase ResolveBuildingsDatabase()
    {
        if (buildingsDatabase != null)
            return buildingsDatabase;

        if (GameDatabaseLoader.Loaded != null && GameDatabaseLoader.Loaded.buildings != null)
        {
            buildingsDatabase = GameDatabaseLoader.Loaded.buildings;
            return buildingsDatabase;
        }

#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(buildingsDatabaseAssetPath))
        {
            buildingsDatabase = AssetDatabase.LoadAssetAtPath<BuildingsDatabase>(buildingsDatabaseAssetPath);
            if (buildingsDatabase != null)
                return buildingsDatabase;
        }
#endif

        return null;
    }

    BuildCategory[] DiscoverFromDatabase(BuildingsDatabase db)
    {
        if (db == null)
            return new BuildCategory[0];

        List<RuntimeBuildEntry> catalogEntries = BuildRuntimeEntries(db);
        if (catalogEntries.Count == 0)
            return new BuildCategory[0];

        List<BuildItemDefinition> items = new List<BuildItemDefinition>(catalogEntries.Count);
        for (int i = 0; i < catalogEntries.Count; i++)
        {
            RuntimeBuildEntry entry = catalogEntries[i];
            if (entry == null || entry.item == null)
                continue;

            items.Add(entry.item);
            if (entry.detailedDefinition != null)
                detailedByItem[entry.item] = entry.detailedDefinition;
        }

        return BuildCategoriesFromItems(items, db.categoryOrder);
    }


    void AutoAssignPlayerTeam()
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.playerTeam != null)
            playerTeamID = gm.playerTeam.teamID;
    }
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleVisible();
    }

    void OnGUI()
    {
        if (!show) return;

        float scale = RTSGameSettings.UIScale;
        float left = Screen.width - (panelWidth * scale) - marginRight;
        float top = marginTop;

        Rect panelRect = new Rect(left, top, panelWidth * scale, panelHeight * scale);
        IMGUIInputBlocker.Register(panelRect);

        GUI.Box(panelRect, "BUILD MENU | RTS");

        float x = panelRect.x + 10 * scale;
        float y = panelRect.y + 24 * scale;

        if (categories == null || categories.Length == 0)
        {
            GUI.Label(new Rect(x, y, panelWidth - 20, 20), "No build items found.");
            y += 20;

            if (autoDiscover)
            {
                BuildingsDatabase db = ResolveBuildingsDatabase();
                if (db != null)
                    GUI.Label(new Rect(x, y, panelWidth - 20, 20), $"Database: {db.name}");
                else
                    GUI.Label(new Rect(x, y, panelWidth - 20, 20), $"Resources path: \"{resourcesPath}\"");

                y += 20;
                if (GUI.Button(new Rect(x, y, 160, 22), "Rebuild Catalog"))
                    RebuildCatalog();
            }
            return;
        }

        GUI.Label(new Rect(x, y, 70 * scale, 20 * scale), "Search:");
        search = GUI.TextField(new Rect(x + 62 * scale, y, panelRect.width - 20 * scale - 62 * scale, 20 * scale), search ?? "");
        y += 26 * scale;

        float tabH = 28f * scale;
        float tabW = Mathf.Max(90f * scale, (panelRect.width - 20 * scale) / Mathf.Max(1, Mathf.Min(categories.Length, 4)));
        int maxTabsPerRow = Mathf.Max(1, Mathf.FloorToInt((panelRect.width - 20 * scale) / tabW));

        int tabIndex = 0;
        while (tabIndex < categories.Length)
        {
            int rowCount = Mathf.Min(maxTabsPerRow, categories.Length - tabIndex);

            for (int i = 0; i < rowCount; i++)
            {
                int idx = tabIndex + i;
                Rect r = new Rect(x + i * tabW, y, tabW - 6, tabH);
                bool isSel = (idx == selectedCategoryIndex);

                bool prev = GUI.enabled;
                GUI.enabled = !isSel;

                string label = categories[idx] != null ? categories[idx].name : "Null";
                if (GUI.Button(r, label))
                {
                    selectedCategoryIndex = idx;
                    scroll = Vector2.zero;
                }

                GUI.enabled = prev;
            }

            y += tabH + 6;
            tabIndex += rowCount;
        }

        selectedCategoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, categories.Length - 1);
        BuildCategory cat = categories[selectedCategoryIndex];

        GUI.Label(new Rect(x, y, panelRect.width - 20 * scale, 18 * scale), $"Category: {cat.name}");
        y += 20 * scale;

        Rect listRect = new Rect(x, y, panelRect.width - 20 * scale, panelRect.yMax - y - 10 * scale);
        GUI.Box(listRect, "");

        float innerX = listRect.x + 8;
        float innerY = listRect.y + 8;
        float innerW = listRect.width - 16;
        float innerH = listRect.height - 16;

        Rect viewRect = new Rect(0, 0, innerW - 18, Mathf.Max(innerH, 800));
        scroll = GUI.BeginScrollView(new Rect(innerX, innerY, innerW, innerH), scroll, viewRect);

        float iy = 0f;
        int drawn = 0;

        BuildItemDefinition[] items = (cat != null) ? cat.items : null;
        if (items == null || items.Length == 0)
        {
            GUI.Label(new Rect(0, iy, innerW - 18, 18), "No items in this category.");
            iy += 20;
        }
        else
        {
            for (int i = 0; i < items.Length; i++)
            {
                BuildItemDefinition item = items[i];
                if (item == null) continue;

                if (!string.IsNullOrEmpty(search))
                {
                    string dn = item.displayName ?? item.name;
                    if (dn == null) dn = "";
                    if (dn.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                }

                bool canAfford = true;
                if (showAffordability)
                {
                    if (TeamStorageManager.Instance != null)
                        canAfford = TeamStorageManager.Instance.CanAffordAvailable(playerTeamID, item.costs);
                    else if (TeamResources.Instance != null)
                        canAfford = TeamResources.Instance.CanAfford(playerTeamID, item.costs);
                }

                string title = item.displayName;
                if (string.IsNullOrEmpty(title)) title = item.name;

                sb.Length = 0;
                sb.Append(title);

                if (showCosts)
                {
                    sb.Append("  ");
                    sb.Append(CostString(item.costs));
                }

                string details = BuildDetailsString(item);
                if (!string.IsNullOrEmpty(details))
                {
                    sb.Append("  ");
                    sb.Append(details);
                }

                if (showAffordability && !canAfford)
                    sb.Append("  (Need deposited resources)");

                float rowH = 38f * scale;
                Rect btn = new Rect(0, iy, innerW - 18, rowH);

                bool prev = GUI.enabled;
                GUI.enabled = canAfford;

                bool clicked = GUI.Button(btn, sb.ToString());

                GUI.enabled = prev;

                if (clicked && BuildPlacementManager.Instance != null)
                    BuildPlacementManager.Instance.SetSelected(item);

                iy += rowH + 6 * scale;
                drawn++;
                if (drawn > 90) break;
            }

            if (drawn == 0)
                GUI.Label(new Rect(0, iy, innerW - 18, 18), "No items match your search.");
        }

        viewRect.height = Mathf.Max(innerH, iy + 10);
        GUI.EndScrollView();

        GUI.Label(new Rect(panelRect.x + 10 * scale, panelRect.yMax - 18 * scale, panelRect.width - 20 * scale, 18 * scale),
            "Tip: Select an item, hover grid to preview, press R to rotate, click to place.");
    }

    BuildCategory[] DiscoverFromResources(string path)
    {
        ClearRuntimeGeneratedItems();
        detailedByItem.Clear();

        BuildItemDefinition[] all = Resources.LoadAll<BuildItemDefinition>(path);
        if (all == null || all.Length == 0) return new BuildCategory[0];

        return BuildCategoriesFromItems(all, null);
    }

    BuildCategory[] BuildCategoriesFromItems(IEnumerable<BuildItemDefinition> sourceItems, List<string> preferredOrder)
    {
        if (sourceItems == null) return new BuildCategory[0];

        Dictionary<string, List<BuildItemDefinition>> grouped = new Dictionary<string, List<BuildItemDefinition>>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in sourceItems)
        {
            if (item == null) continue;

            string cat = GetCategoryName(item);
            if (string.IsNullOrEmpty(cat)) cat = "Uncategorized";

            if (!grouped.TryGetValue(cat, out var list))
            {
                list = new List<BuildItemDefinition>();
                grouped[cat] = list;
            }
            list.Add(item);
        }

        List<string> catNames = new List<string>();

        if (preferredOrder != null)
        {
            for (int i = 0; i < preferredOrder.Count; i++)
            {
                string name = preferredOrder[i];
                if (string.IsNullOrWhiteSpace(name)) continue;
                if (grouped.ContainsKey(name) && !catNames.Contains(name))
                    catNames.Add(name);
            }
        }

        List<string> remaining = new List<string>();
        foreach (var key in grouped.Keys)
        {
            if (!catNames.Contains(key))
                remaining.Add(key);
        }
        remaining.Sort(StringComparer.OrdinalIgnoreCase);
        catNames.AddRange(remaining);

        BuildCategory[] result = new BuildCategory[catNames.Count];

        for (int c = 0; c < catNames.Count; c++)
        {
            string name = catNames[c];
            var list = grouped[name];

            list.Sort((a, b) =>
            {
                string an = a != null ? (a.displayName ?? a.name) : "";
                string bn = b != null ? (b.displayName ?? b.name) : "";
                return StringComparer.OrdinalIgnoreCase.Compare(an, bn);
            });

            result[c] = new BuildCategory { name = name, items = list.ToArray() };
        }

        return result;
    }

    string GetCategoryName(BuildItemDefinition item)
    {
        string fromMeta = TryReadStringMember(item, "category")
                       ?? TryReadStringMember(item, "categoryName")
                       ?? TryReadStringMember(item, "Category")
                       ?? TryReadStringMember(item, "CategoryName");

        if (!string.IsNullOrEmpty(fromMeta))
            return fromMeta.Trim();

        string n = item.name ?? "";
        int us = n.IndexOf('_');
        if (us > 0) return n.Substring(0, us).Trim();

        string dn = item.displayName ?? "";
        int colon = dn.IndexOf(':');
        if (colon > 0) return dn.Substring(0, colon).Trim();

        return "Uncategorized";
    }

    string TryReadStringMember(object obj, string memberName)
    {
        if (obj == null) return null;
        Type t = obj.GetType();

        FieldInfo f = t.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (f != null && f.FieldType == typeof(string))
            return f.GetValue(obj) as string;

        PropertyInfo p = t.GetProperty(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (p != null && p.PropertyType == typeof(string) && p.GetIndexParameters().Length == 0)
        {
            try { return p.GetValue(obj, null) as string; } catch { }
        }

        return null;
    }

    string CostString(ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0) return "(Free)";

        costSb.Length = 0;
        costSb.Append("[");

        for (int i = 0; i < costs.Length; i++)
        {
            if (i > 0) costSb.Append(", ");
            costSb.Append(costs[i].resource);
            costSb.Append(" ");
            costSb.Append(costs[i].amount);
        }

        costSb.Append("]");
        return costSb.ToString();
    }

    string BuildDetailsString(BuildItemDefinition item)
    {
        if (item == null)
            return string.Empty;

        if (!detailedByItem.TryGetValue(item, out var def) || def == null)
            return string.Empty;

        detailsSb.Length = 0;

        if (def.buildTime > 0f)
            detailsSb.Append($"Time {def.buildTime:0.#}s");

        if (def.maxHealth > 0)
        {
            if (detailsSb.Length > 0) detailsSb.Append(" | ");
            detailsSb.Append($"HP {def.maxHealth}");
        }

        if (def.workerSlots > 0)
        {
            if (detailsSb.Length > 0) detailsSb.Append(" | ");
            detailsSb.Append($"Workers {def.workerSlots}");
        }

        if (def.isStorage && def.storageSettings != null && def.storageSettings.capacity > 0)
        {
            if (detailsSb.Length > 0) detailsSb.Append(" | ");
            detailsSb.Append($"Storage {def.storageSettings.capacity}");
        }

        return detailsSb.ToString();
    }

    List<RuntimeBuildEntry> BuildRuntimeEntries(BuildingsDatabase db)
    {
        ClearRuntimeGeneratedItems();
        detailedByItem.Clear();

        List<RuntimeBuildEntry> entries = new List<RuntimeBuildEntry>();

        if (db.buildings != null)
        {
            for (int i = 0; i < db.buildings.Count; i++)
            {
                BuildingDefinition def = db.buildings[i];
                if (def == null || def.prefab == null)
                    continue;

                BuildItemDefinition generated = CreateBuildItemFromDetailed(def);
                if (generated == null)
                    continue;

                entries.Add(new RuntimeBuildEntry { item = generated, detailedDefinition = def });
            }
        }

        if (entries.Count > 0)
            return entries;

        if (db.items != null)
        {
            for (int i = 0; i < db.items.Count; i++)
            {
                BuildItemDefinition item = db.items[i];
                if (item == null)
                    continue;
                entries.Add(new RuntimeBuildEntry { item = item, detailedDefinition = null });
            }
        }

        return entries;
    }

    BuildItemDefinition CreateBuildItemFromDetailed(BuildingDefinition def)
    {
        if (def == null || def.prefab == null)
            return null;

        BuildItemDefinition generated = ScriptableObject.CreateInstance<BuildItemDefinition>();
        generated.name = string.IsNullOrWhiteSpace(def.id) ? def.prefab.name : def.id;
        generated.displayName = string.IsNullOrWhiteSpace(def.displayName) ? generated.name : def.displayName;
        generated.icon = def.icon;
        generated.prefab = def.prefab;
        generated.buildTime = Mathf.Max(0f, def.buildTime);
        generated.category = GetDetailedCategoryName(def);
        generated.aiPriority = MapAIPriority(def.category);
        generated.costs = ConvertConstructionCosts(def.constructionCost);

        runtimeGeneratedItems.Add(generated);
        return generated;
    }

    string GetDetailedCategoryName(BuildingDefinition def)
    {
        if (def == null)
            return "Uncategorized";

        if (!string.IsNullOrWhiteSpace(def.subCategory))
            return def.subCategory.Trim();

        return def.category.ToString();
    }

    ResourceCost[] ConvertConstructionCosts(List<ResourceAmount> costs)
    {
        if (costs == null || costs.Count == 0)
            return Array.Empty<ResourceCost>();

        List<ResourceCost> converted = new List<ResourceCost>(costs.Count);
        for (int i = 0; i < costs.Count; i++)
        {
            ResourceAmount amount = costs[i];
            if (amount == null || amount.resource == null || amount.amount <= 0)
                continue;

            if (!TryMapResourceDefinition(amount.resource, out ResourceDefinition type))
                continue;

            converted.Add(new ResourceCost { resource = type, amount = amount.amount });
        }

        return converted.ToArray();
    }

    bool TryMapResourceDefinition(ResourceDefinition resource, out ResourceDefinition mapped)
    {
        mapped = resource;
        return mapped != null;
    }

    bool TryParseResourceToken(string token, out ResourceDefinition parsed)
    {
        parsed = null;

        if (string.IsNullOrEmpty(token) || ResourcesDatabase.Instance == null)
            return false;

        return ResourcesDatabase.Instance.TryGetById(token, out parsed);
    }

    string NormalizeResourceToken(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        return raw.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).Trim();
    }

AIBuildingPriority MapAIPriority(BuildingCategory category)
    {
        switch (category)
        {
            case BuildingCategory.Housing:
                return AIBuildingPriority.Housing;
            case BuildingCategory.Military:
                return AIBuildingPriority.Defense;
            case BuildingCategory.Utility:
            case BuildingCategory.Research:
                return AIBuildingPriority.Tech;
            case BuildingCategory.Production:
            case BuildingCategory.Farming:
            case BuildingCategory.Mining:
                return AIBuildingPriority.Industry;
            default:
                return AIBuildingPriority.Economy;
        }
    }

    void OnDestroy()
    {
        ClearRuntimeGeneratedItems();
    }

    void ClearRuntimeGeneratedItems()
    {
        for (int i = 0; i < runtimeGeneratedItems.Count; i++)
        {
            if (runtimeGeneratedItems[i] != null)
                Destroy(runtimeGeneratedItems[i]);
        }
        runtimeGeneratedItems.Clear();
    }

    void TrySyncGrid(bool visible)
    {
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < behaviours.Length; i++)
        {
            var mb = behaviours[i];
            if (mb == null) continue;

            Type t = mb.GetType();
            string n = t.Name;

            if (n.IndexOf("BuildGrid", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            string[] methods = { "SetGridVisible", "SetVisible", "SetShown", "SetShowGrid", "SetBuildMode", "SetEnabled" };

            for (int m = 0; m < methods.Length; m++)
            {
                var mi = t.GetMethod(methods[m], new Type[] { typeof(bool) });
                if (mi != null) { mi.Invoke(mb, new object[] { visible }); return; }
            }
        }
    }
}
