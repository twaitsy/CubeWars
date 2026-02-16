using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    enum MenuScreen { Main, Load, Credits, MatchSetup, Settings, Statistics, Craftables, Achievements, Cheats }
    enum DiplomacyStance { Ally, Neutral, Enemy }

    [Serializable]
    class TeamSetupEntry
    {
        public int teamID;
        public Color color;
        public int aiDifficulty;
        public int startingResources = 500;
        public DiplomacyStance diplomacy = DiplomacyStance.Enemy;
    }

    [Header("Toggle")]
    public bool show = true;
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("Panel Layout")]
    public int panelWidth = 560;
    public int buttonHeight = 34;
    public int buttonSpacing = 8;

    [Header("Styles")]
    public int fontSize = 16;
    GUIStyle buttonStyle;
    GUIStyle boxStyle;
    GUIStyle labelStyle;
    bool stylesInitialized;

    [Header("Load Menu")]
    public string[] loadableScenes;

    [Header("Credits")]
    [TextArea(5, 10)] public string creditsText = "Twaitsy";

    [Header("Match Setup")]
    [SerializeField] List<TeamSetupEntry> teams = new();
    int teamCount = 6;

    MenuScreen screen = MenuScreen.Main;
    Vector2 matchScroll;
    Vector2 longListScroll;

    public bool IsVisible => show;

    void Awake()
    {
        EnsureDefaultTeams();
        EnsureSettingsInstance();
    }

    void EnsureSettingsInstance()
    {
        if (RTSGameSettings.Instance == null)
            new GameObject("RTSGameSettings").AddComponent<RTSGameSettings>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            show = !show;

        if (show && RTSGameSettings.Instance != null && RTSGameSettings.Instance.gameplay.pauseOnMenuOpen)
            Time.timeScale = 0f;
        else
            RTSGameSettings.Instance?.Apply();
    }

    void OnGUI()
    {
        if (!show) return;

        if (!stylesInitialized)
        {
            InitStyles();
            stylesInitialized = true;
        }

        switch (screen)
        {
            case MenuScreen.Main: DrawMainMenu(); break;
            case MenuScreen.Load: DrawLoadMenu(); break;
            case MenuScreen.Credits: DrawCredits(); break;
            case MenuScreen.MatchSetup: DrawMatchSetup(); break;
            case MenuScreen.Settings: DrawSettings(); break;
            case MenuScreen.Statistics: DrawSimpleListScreen("STATISTICS", RTSGameSettings.Instance?.BuildLiveStats()); break;
            case MenuScreen.Craftables: DrawSimpleListScreen("CRAFTABLES", RTSGameSettings.Instance?.GetCraftablesSummary()); break;
            case MenuScreen.Achievements: DrawSimpleListScreen("ACHIEVEMENTS", RTSGameSettings.Instance?.GetAchievementsSummary()); break;
            case MenuScreen.Cheats: DrawCheats(); break;
        }
    }

    void InitStyles()
    {
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        boxStyle = new GUIStyle(GUI.skin.box) { fontSize = fontSize + 3, alignment = TextAnchor.UpperCenter, fontStyle = FontStyle.Bold };
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true };
    }

    Rect PanelRect(float width, float height)
    {
        float scale = RTSGameSettings.UIScale;
        width *= scale;
        height *= scale;
        return new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);
    }

    void DrawMainMenu()
    {
        Rect rect = PanelRect(panelWidth, 620);
        GUI.Box(rect, "MAIN MENU", boxStyle);

        float x = rect.x + 20;
        float y = rect.y + 44;
        float w = rect.width - 40;

        DrawMainButton(ref y, x, w, "New Game", () => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        DrawMainButton(ref y, x, w, "Match Setup", () => screen = MenuScreen.MatchSetup);
        DrawMainButton(ref y, x, w, "Load Game", () => screen = MenuScreen.Load);
        DrawMainButton(ref y, x, w, "Settings", () => screen = MenuScreen.Settings);
        DrawMainButton(ref y, x, w, "Statistics", () => screen = MenuScreen.Statistics);
        DrawMainButton(ref y, x, w, "Craftables", () => screen = MenuScreen.Craftables);
        DrawMainButton(ref y, x, w, "Achievements", () => screen = MenuScreen.Achievements);
        DrawMainButton(ref y, x, w, "Cheats", () => screen = MenuScreen.Cheats);
        DrawMainButton(ref y, x, w, "Credits", () => screen = MenuScreen.Credits);

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 16, w, buttonHeight), "Quit", buttonStyle))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    void DrawMainButton(ref float y, float x, float w, string text, Action action)
    {
        if (GUI.Button(new Rect(x, y, w, buttonHeight), text, buttonStyle)) action();
        y += buttonHeight + buttonSpacing;
    }

    void DrawSettings()
    {
        var settings = RTSGameSettings.Instance;
        if (settings == null)
        {
            screen = MenuScreen.Main;
            return;
        }

        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, "SETTINGS", boxStyle);

        float x = rect.x + 20;
        float y = rect.y + 50;
        float w = rect.width - 40;

        GUI.Label(new Rect(x, y, 260, 22), $"UI Scale: {settings.display.uiScale:0.0}", labelStyle);
        settings.display.uiScale = GUI.HorizontalSlider(new Rect(x + 180, y + 6, w - 190, 20), settings.display.uiScale, 0.8f, 1.6f);
        y += 32;

        GUI.Label(new Rect(x, y, 260, 22), $"Game Speed: {settings.gameplay.gameSpeed:0.0}x", labelStyle);
        settings.gameplay.gameSpeed = GUI.HorizontalSlider(new Rect(x + 180, y + 6, w - 190, 20), settings.gameplay.gameSpeed, 0.5f, 3f);
        y += 32;

        settings.display.showMinimap = GUI.Toggle(new Rect(x, y, w, 22), settings.display.showMinimap, "Show Minimap"); y += 24;
        settings.display.showTaskBoard = GUI.Toggle(new Rect(x, y, w, 22), settings.display.showTaskBoard, "Show Task Board"); y += 24;
        settings.display.showInspector = GUI.Toggle(new Rect(x, y, w, 22), settings.display.showInspector, "Show Inspector"); y += 24;
        settings.gameplay.pauseOnMenuOpen = GUI.Toggle(new Rect(x, y, w, 22), settings.gameplay.pauseOnMenuOpen, "Pause when menu is open");

        settings.Apply();

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 16, w, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void DrawCheats()
    {
        var settings = RTSGameSettings.Instance;
        if (settings == null) return;

        Rect rect = PanelRect(panelWidth, 380);
        GUI.Box(rect, "CHEATS", boxStyle);

        float x = rect.x + 20;
        float y = rect.y + 50;
        float w = rect.width - 40;

        settings.cheats.enabled = GUI.Toggle(new Rect(x, y, w, 22), settings.cheats.enabled, "Enable Cheats"); y += 26;

        GUI.enabled = settings.cheats.enabled;
        settings.cheats.infiniteResources = GUI.Toggle(new Rect(x, y, w, 22), settings.cheats.infiniteResources, "Infinite Resources"); y += 24;
        settings.cheats.fastSimulation = GUI.Toggle(new Rect(x, y, w, 22), settings.cheats.fastSimulation, "Fast Simulation"); y += 24;
        settings.cheats.unlockAllBuilds = GUI.Toggle(new Rect(x, y, w, 22), settings.cheats.unlockAllBuilds, "Unlock All Builds"); y += 28;
        GUI.enabled = true;

        settings.Apply();

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 16, w, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void DrawSimpleListScreen(string title, Dictionary<string, string> values)
    {
        var lines = new List<string>();
        if (values != null)
            foreach (var kv in values) lines.Add($"{kv.Key}: {kv.Value}");
        DrawSimpleListScreen(title, lines);
    }

    void DrawSimpleListScreen(string title, List<string> lines)
    {
        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, title, boxStyle);

        Rect scrollArea = new(rect.x + 20, rect.y + 50, rect.width - 40, rect.height - 110);
        Rect content = new(0, 0, scrollArea.width - 20, Mathf.Max(320, (lines != null ? lines.Count : 1) * 24));

        longListScroll = GUI.BeginScrollView(scrollArea, longListScroll, content);
        if (lines == null || lines.Count == 0)
        {
            GUI.Label(new Rect(0, 0, content.width, 24), "No data available.", labelStyle);
        }
        else
        {
            for (int i = 0; i < lines.Count; i++)
                GUI.Label(new Rect(0, i * 24, content.width, 22), lines[i], labelStyle);
        }
        GUI.EndScrollView();

        if (GUI.Button(new Rect(rect.x + 20, rect.yMax - buttonHeight - 16, rect.width - 40, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void DrawLoadMenu()
    {
        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, "LOAD GAME", boxStyle);

        float x = rect.x + 20;
        float y = rect.y + 44;
        float w = rect.width - 40;

        foreach (var sceneName in loadableScenes)
        {
            if (GUI.Button(new Rect(x, y, w, buttonHeight), $"Load {sceneName}", buttonStyle))
                SceneManager.LoadScene(sceneName);
            y += buttonHeight + buttonSpacing;
        }

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 16, w, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void DrawCredits()
    {
        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, "CREDITS", boxStyle);

        GUI.Label(new Rect(rect.x + 20, rect.y + 44, rect.width - 40, rect.height - 100), creditsText, labelStyle);

        if (GUI.Button(new Rect(rect.x + 20, rect.yMax - buttonHeight - 16, rect.width - 40, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void DrawMatchSetup()
    {
        Rect rect = PanelRect(panelWidth, 520);
        GUI.Box(rect, "MATCH SETUP", boxStyle);

        float x = rect.x + 12;
        float y = rect.y + 36;
        float w = rect.width - 24;

        GUI.Label(new Rect(x, y, w, 20), "Configure teams, AI, resources, and diplomacy before starting.", labelStyle);
        y += 28;

        GUI.Label(new Rect(x, y, 110, 20), "Team Count", labelStyle);
        string teamStr = GUI.TextField(new Rect(x + 112, y, 50, 20), teamCount.ToString());
        if (int.TryParse(teamStr, out int parsed)) teamCount = Mathf.Clamp(parsed, 2, 8);

        if (GUI.Button(new Rect(x + 170, y, 120, 22), "Apply Count"))
            EnsureDefaultTeams();

        y += 32;

        Rect scrollRect = new(x, y, w, rect.yMax - y - buttonHeight - 24);
        float contentH = teams.Count * 56 + 10;
        matchScroll = GUI.BeginScrollView(scrollRect, matchScroll, new Rect(0, 0, w - 20, contentH));

        float sy = 0;
        for (int i = 0; i < teams.Count; i++)
        {
            var t = teams[i];
            GUI.Label(new Rect(0, sy, 40, 20), $"T{t.teamID}");
            t.color = RGBField(new Rect(40, sy, 110, 20), t.color);
            GUI.Label(new Rect(155, sy, 30, 20), "AI:");
            t.aiDifficulty = IntField(new Rect(185, sy, 40, 20), t.aiDifficulty, 0, 3);
            GUI.Label(new Rect(230, sy, 40, 20), "Res:");
            t.startingResources = IntField(new Rect(270, sy, 60, 20), t.startingResources, 100, 10000);
            sy += 24;

            GUI.Label(new Rect(0, sy, 90, 20), "Diplomacy:");
            t.diplomacy = (DiplomacyStance)GUI.SelectionGrid(new Rect(90, sy, 200, 20), (int)t.diplomacy, new[] { "Ally", "Neutral", "Enemy" }, 3);
            sy += 30;
        }

        GUI.EndScrollView();

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 12, 180, buttonHeight), "Start With Settings", buttonStyle))
        {
            ApplySetupToDiplomacy();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (GUI.Button(new Rect(rect.xMax - 190, rect.yMax - buttonHeight - 12, 180, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    void EnsureDefaultTeams()
    {
        if (teamCount < 2) teamCount = 2;

        while (teams.Count < teamCount)
        {
            int id = teams.Count;
            teams.Add(new TeamSetupEntry
            {
                teamID = id,
                color = Color.HSVToRGB((id * 0.17f) % 1f, 0.8f, 0.95f),
                aiDifficulty = 1,
                startingResources = 500,
                diplomacy = id == 0 ? DiplomacyStance.Ally : DiplomacyStance.Enemy
            });
        }

        if (teams.Count > teamCount)
            teams.RemoveRange(teamCount, teams.Count - teamCount);

        for (int i = 0; i < teams.Count; i++)
            teams[i].teamID = i;
    }

    void ApplySetupToDiplomacy()
    {
        var dip = DiplomacyManager.Instance;
        if (dip == null) return;

        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                bool atWar = teams[i].diplomacy == DiplomacyStance.Enemy || teams[j].diplomacy == DiplomacyStance.Enemy;
                if (teams[i].diplomacy == DiplomacyStance.Ally && teams[j].diplomacy == DiplomacyStance.Ally) atWar = false;
                dip.SetWarState(teams[i].teamID, teams[j].teamID, atWar);
            }
        }
    }

    static int IntField(Rect r, int value, int min, int max)
    {
        string s = GUI.TextField(r, value.ToString());
        return int.TryParse(s, out int parsed) ? Mathf.Clamp(parsed, min, max) : value;
    }

    static Color RGBField(Rect r, Color c)
    {
        float cell = r.width / 3f;
        string rs = GUI.TextField(new Rect(r.x, r.y, cell - 2, r.height), Mathf.RoundToInt(c.r * 255f).ToString());
        string gs = GUI.TextField(new Rect(r.x + cell, r.y, cell - 2, r.height), Mathf.RoundToInt(c.g * 255f).ToString());
        string bs = GUI.TextField(new Rect(r.x + cell * 2f, r.y, cell - 2, r.height), Mathf.RoundToInt(c.b * 255f).ToString());

        int rV = byte.TryParse(rs, out var rr) ? rr : (byte)Mathf.RoundToInt(c.r * 255f);
        int gV = byte.TryParse(gs, out var gg) ? gg : (byte)Mathf.RoundToInt(c.g * 255f);
        int bV = byte.TryParse(bs, out var bb) ? bb : (byte)Mathf.RoundToInt(c.b * 255f);
        return new Color32((byte)rV, (byte)gV, (byte)bV, 255);
    }
}
