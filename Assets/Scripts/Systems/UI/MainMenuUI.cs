using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    enum MenuScreen { Main, Load, Credits, MatchSetup }
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
    public int panelWidth = 520;
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
    [SerializeField] List<TeamSetupEntry> teams = new List<TeamSetupEntry>();
    int teamCount = 6;

    MenuScreen screen = MenuScreen.Main;
    Vector2 matchScroll;

    public bool IsVisible => show;

    void Awake()
    {
        EnsureDefaultTeams();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            show = !show;
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
        }
    }

    void InitStyles()
    {
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = fontSize + 3,
            alignment = TextAnchor.UpperCenter,
            fontStyle = FontStyle.Bold
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            wordWrap = true
        };
    }

    Rect PanelRect(float width, float height) =>
        new Rect((Screen.width - width) / 2f,
                 (Screen.height - height) / 2f,
                 width,
                 height);

    // ---------------------------------------------------------
    // MAIN MENU
    // ---------------------------------------------------------
    void DrawMainMenu()
    {
        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, "MAIN MENU", boxStyle);

        float x = rect.x + 20;
        float y = rect.y + 44;
        float w = rect.width - 40;

        if (GUI.Button(new Rect(x, y, w, buttonHeight), "New Game", buttonStyle))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, w, buttonHeight), "Match Setup (AoE Style)", buttonStyle))
            screen = MenuScreen.MatchSetup;
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, w, buttonHeight), "Load Game", buttonStyle))
            screen = MenuScreen.Load;
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, w, buttonHeight), "Credits", buttonStyle))
            screen = MenuScreen.Credits;
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, w, buttonHeight), "Quit", buttonStyle))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    // ---------------------------------------------------------
    // LOAD MENU
    // ---------------------------------------------------------
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

    // ---------------------------------------------------------
    // CREDITS
    // ---------------------------------------------------------
    void DrawCredits()
    {
        Rect rect = PanelRect(panelWidth, 500);
        GUI.Box(rect, "CREDITS", boxStyle);

        GUI.Label(new Rect(rect.x + 20, rect.y + 44,
            rect.width - 40, rect.height - 100), creditsText, labelStyle);

        if (GUI.Button(new Rect(rect.x + 20, rect.yMax - buttonHeight - 16,
            rect.width - 40, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    // ---------------------------------------------------------
    // MATCH SETUP — TWO‑ROW LAYOUT
    // ---------------------------------------------------------
    void DrawMatchSetup()
    {
        float headerHeight = 36 + 24 + 32;
        float rowHeight = 52; // two rows per team
        float tableHeaderHeight = 28;

        float contentHeight =
            headerHeight +
            tableHeaderHeight +
            teams.Count * rowHeight +
            80;

        float minHeight = 450;
        float maxHeight = Screen.height * 0.9f;

        float panelHeight = Mathf.Clamp(contentHeight, minHeight, maxHeight);
        float width = Mathf.Clamp(panelWidth, 520, Screen.width * 0.9f);

        Rect rect = PanelRect(width, panelHeight);
        GUI.Box(rect, "MATCH SETUP", boxStyle);

        float x = rect.x + 12;
        float y = rect.y + 36;
        float w = rect.width - 24;

        GUI.Label(new Rect(x, y, w, 20),
            "Configure teams, AI, resources, and diplomacy before starting.",
            labelStyle);
        y += 28;

        GUI.Label(new Rect(x, y, 110, 20), "Team Count", labelStyle);
        string teamStr = GUI.TextField(new Rect(x + 112, y, 50, 20), teamCount.ToString());
        if (int.TryParse(teamStr, out int parsed)) teamCount = Mathf.Clamp(parsed, 2, 8);

        if (GUI.Button(new Rect(x + 170, y, 120, 22), "Apply Count"))
            EnsureDefaultTeams();

        y += 32;

        GUI.Box(new Rect(x, y, w, 24),
            "Team Setup", boxStyle);
        y += 28;

        float scrollAreaHeight = rect.yMax - y - buttonHeight - 20;
        Rect scrollRect = new Rect(x, y, w, scrollAreaHeight);

        float contentW = w - 20;
        float contentH = teams.Count * rowHeight + 10;

        matchScroll = GUI.BeginScrollView(scrollRect, matchScroll,
            new Rect(0, 0, contentW, contentH));

        float sy = 0;

        for (int i = 0; i < teams.Count; i++)
        {
            var t = teams[i];

            // --- ROW 1 ---
            GUI.Label(new Rect(0, sy, 40, 20), $"T{t.teamID}");

            t.color = RGBField(new Rect(40, sy, 110, 20), t.color);

            GUI.Label(new Rect(155, sy, 30, 20), "AI:");
            t.aiDifficulty = IntField(new Rect(185, sy, 40, 20), t.aiDifficulty, 0, 3);

            GUI.Label(new Rect(230, sy, 40, 20), "Res:");
            t.startingResources = IntField(new Rect(270, sy, 60, 20),
                t.startingResources, 100, 10000);

            sy += 24;

            // --- ROW 2 (Diplomacy) ---
            GUI.Label(new Rect(0, sy, 90, 20), "Diplomacy:");

            int stance = GUI.SelectionGrid(
                new Rect(90, sy, 200, 20),
                (int)t.diplomacy,
                new[] { "Ally", "Neutral", "Enemy" },
                3
            );
            t.diplomacy = (DiplomacyStance)stance;

            sy += 28;
        }

        GUI.EndScrollView();

        if (GUI.Button(new Rect(x, rect.yMax - buttonHeight - 12,
            160, buttonHeight), "Start With Settings", buttonStyle))
        {
            ApplySetupToDiplomacy();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (GUI.Button(new Rect(rect.xMax - 170, rect.yMax - buttonHeight - 12,
            160, buttonHeight), "Back", buttonStyle))
            screen = MenuScreen.Main;
    }

    // ---------------------------------------------------------
    // TEAM SETUP HELPERS
    // ---------------------------------------------------------
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
                bool atWar =
                    teams[i].diplomacy == DiplomacyStance.Enemy ||
                    teams[j].diplomacy == DiplomacyStance.Enemy;

                if (teams[i].diplomacy == DiplomacyStance.Ally &&
                    teams[j].diplomacy == DiplomacyStance.Ally)
                    atWar = false;

                dip.SetWarState(teams[i].teamID, teams[j].teamID, atWar);
            }
        }
    }

    static int IntField(Rect r, int value, int min, int max)
    {
        string s = GUI.TextField(r, value.ToString());
        if (int.TryParse(s, out int parsed))
            return Mathf.Clamp(parsed, min, max);
        return value;
    }

    static Color RGBField(Rect r, Color c)
    {
        float cell = r.width / 3f;
        string rs = GUI.TextField(new Rect(r.x, r.y, cell - 2, r.height),
            Mathf.RoundToInt(c.r * 255f).ToString());
        string gs = GUI.TextField(new Rect(r.x + cell, r.y, cell - 2, r.height),
            Mathf.RoundToInt(c.g * 255f).ToString());
        string bs = GUI.TextField(new Rect(r.x + cell * 2f, r.y, cell - 2, r.height),
            Mathf.RoundToInt(c.b * 255f).ToString());

        int rV = byte.TryParse(rs, out var rr) ? rr : (byte)Mathf.RoundToInt(c.r * 255f);
        int gV = byte.TryParse(gs, out var gg) ? gg : (byte)Mathf.RoundToInt(c.g * 255f);
        int bV = byte.TryParse(bs, out var bb) ? bb : (byte)Mathf.RoundToInt(c.b * 255f);

        return new Color32((byte)rV, (byte)gV, (byte)bV, 255);
    }
}