using System;
using System.Collections.Generic;
using UnityEngine;

public class RTSGameSettings : MonoBehaviour
{
    [Serializable]
    public class DisplayOptions
    {
        [Range(0.8f, 1.6f)] public float uiScale = 1f;
        public bool showMinimap = true;
        public bool showTaskBoard = true;
        public bool showInspector = true;
    }

    [Serializable]
    public class GameplayOptions
    {
        [Range(0.5f, 3f)] public float gameSpeed = 1f;
        public bool pauseOnMenuOpen = false;
    }

    [Serializable]
    public class CheatOptions
    {
        public bool enabled;
        public bool infiniteResources;
        public bool fastSimulation;
        public bool unlockAllBuilds;
    }

    public static RTSGameSettings Instance;

    public DisplayOptions display = new DisplayOptions();
    public GameplayOptions gameplay = new GameplayOptions();
    public CheatOptions cheats = new CheatOptions();

    float applyTimer;

    public static float UIScale => Instance != null ? Instance.display.uiScale : 1f;

    public static bool IsCheatActive(Func<CheatOptions, bool> predicate)
    {
        return Instance != null && Instance.cheats.enabled && predicate(Instance.cheats);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Apply();
    }

    void Update()
    {
        applyTimer -= Time.unscaledDeltaTime;
        if (applyTimer <= 0f)
        {
            applyTimer = 0.25f;
            Apply();
        }
    }

    public void Apply()
    {
        float speed = gameplay.gameSpeed;
        if (IsCheatActive(c => c.fastSimulation))
            speed = Mathf.Max(speed, 2f);

        Time.timeScale = Mathf.Clamp(speed, 0.1f, 5f);

        ToggleComponent<Minimap>(display.showMinimap);
        ToggleComponent<TaskBoardUI>(display.showTaskBoard);
        ToggleComponent<UnitInspectorUI>(display.showInspector);
    }

    public Dictionary<string, string> BuildLiveStats()
    {
        var stats = new Dictionary<string, string>();
        stats["Units"] = FindObjectsOfType<Unit>().Length.ToString();
        stats["Civilians"] = FindObjectsOfType<Civilian>().Length.ToString();
        stats["Buildings"] = FindObjectsOfType<Building>().Length.ToString();
        stats["Construction Sites"] = FindObjectsOfType<ConstructionSite>().Length.ToString();
        stats["Stored Notifications"] = AlertManager.Instance != null
            ? AlertManager.Instance.GetRecent(10).Count.ToString()
            : "0";
        return stats;
    }

    public List<string> GetCraftablesSummary()
    {
        var lines = new List<string>();
        var buildings = FindObjectsOfType<CraftingBuilding>();
        for (int i = 0; i < buildings.Length; i++)
        {
            var b = buildings[i];
            if (b == null || b.recipe == null) continue;
            lines.Add($"{b.recipe.recipeName} @ {SanitizeName(b.name)} - {b.State} ({b.CraftProgress01:P0})");
        }

        if (lines.Count == 0)
            lines.Add("No active crafting stations discovered.");

        return lines;
    }

    public List<string> GetAchievementsSummary()
    {
        var achievements = new List<string>();
        int civCount = FindObjectsOfType<Civilian>().Length;
        int buildingCount = FindObjectsOfType<Building>().Length;

        achievements.Add((civCount >= 10 ? "[Unlocked]" : "[Locked]") + " Community Hub (10+ civilians)");
        achievements.Add((buildingCount >= 6 ? "[Unlocked]" : "[Locked]") + " Settlement Core (6+ buildings)");

        bool hasCrafting = FindObjectsOfType<CraftingBuilding>().Length > 0;
        achievements.Add((hasCrafting ? "[Unlocked]" : "[Locked]") + " Industrial Start (place a crafting station)");

        return achievements;
    }

    static void ToggleComponent<T>(bool show) where T : MonoBehaviour
    {
        var all = FindObjectsOfType<T>();
        for (int i = 0; i < all.Length; i++)
        {
            var component = all[i];
            if (component == null) continue;

            var type = component.GetType();
            var field = type.GetField("show");
            if (field != null && field.FieldType == typeof(bool))
                field.SetValue(component, show);
        }
    }

    static string SanitizeName(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return raw;
        return raw.Replace("(Clone)", "").Trim();
    }
}
