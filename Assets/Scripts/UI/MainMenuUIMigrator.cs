using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class MainMenuUIMigrator
{
    private const string ScriptsFolder = "Assets/Systems/UI";

    [MenuItem("Tools/Cube Wars/Generate Main Menu UI")]
    public static void GenerateMainMenuUI()
    {
        EnsureFolders();
        EnsureRuntimeScriptsExist();

        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();

        // Create panels
        GameObject mainMenuGO = CreatePanel(canvas.transform, "MainMenuPanel", out UIPanel mainMenuPanel);
        GameObject settingsGO = CreatePanel(canvas.transform, "SettingsPanel", out UIPanel settingsPanel);
        GameObject loadGO = CreatePanel(canvas.transform, "LoadGamePanel", out UIPanel loadPanel);
        GameObject matchSetupGO = CreatePanel(canvas.transform, "MatchSetupPanel", out UIPanel matchSetupPanel);
        GameObject statsGO = CreatePanel(canvas.transform, "StatisticsPanel", out UIPanel statsPanel);
        GameObject craftGO = CreatePanel(canvas.transform, "CraftablesPanel", out UIPanel craftPanel);
        GameObject achievementsGO = CreatePanel(canvas.transform, "AchievementsPanel", out UIPanel achievementsPanel);
        GameObject cheatsGO = CreatePanel(canvas.transform, "CheatsPanel", out UIPanel cheatsPanel);
        GameObject creditsGO = CreatePanel(canvas.transform, "CreditsPanel", out UIPanel creditsPanel);

        // Build Main Menu layout
        BuildMainMenuLayout(mainMenuGO, mainMenuPanel,
            settingsPanel, loadPanel, matchSetupPanel,
            statsPanel, craftPanel, achievementsPanel,
            cheatsPanel, creditsPanel);

        // Simple placeholders for other panels
        BuildSimplePanelLayout(settingsGO, "SETTINGS");
        BuildSimplePanelLayout(loadGO, "LOAD GAME");
        BuildSimplePanelLayout(matchSetupGO, "MATCH SETUP");
        BuildSimplePanelLayout(statsGO, "STATISTICS");
        BuildSimplePanelLayout(craftGO, "CRAFTABLES");
        BuildSimplePanelLayout(achievementsGO, "ACHIEVEMENTS");
        BuildSimplePanelLayout(cheatsGO, "CHEATS");
        BuildSimplePanelLayout(creditsGO, "CREDITS");

        // Create / configure UIManager
        UIManager uiManager = EnsureUIManager();
        uiManager.SetMainMenu(mainMenuPanel);
        uiManager.RegisterPanel(settingsPanel);
        uiManager.RegisterPanel(loadPanel);
        uiManager.RegisterPanel(matchSetupPanel);
        uiManager.RegisterPanel(statsPanel);
        uiManager.RegisterPanel(craftPanel);
        uiManager.RegisterPanel(achievementsPanel);
        uiManager.RegisterPanel(cheatsPanel);
        uiManager.RegisterPanel(creditsPanel);

        // Toggle controller
        MainMenuToggleController toggle = Object.FindObjectOfType<MainMenuToggleController>();
        if (toggle == null)
        {
            GameObject toggleGO = new GameObject("MainMenuToggleController");
            toggle = toggleGO.AddComponent<MainMenuToggleController>();
        }
        toggle.SetMainMenu(mainMenuPanel);

        Selection.activeGameObject = mainMenuGO;
        Debug.Log("Cube Wars Main Menu UI generated.");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Systems"))
            AssetDatabase.CreateFolder("Assets", "Systems");
        if (!AssetDatabase.IsValidFolder(ScriptsFolder))
            AssetDatabase.CreateFolder("Assets/Systems", "UI");
        if (!AssetDatabase.IsValidFolder(ScriptsFolder + "/Editor"))
            AssetDatabase.CreateFolder(ScriptsFolder, "Editor");
    }

    private static void EnsureRuntimeScriptsExist()
    {
        // This is a light check: if file missing, create empty template.
        EnsureScript("UIPanel.cs", GetUIPanelTemplate());
        EnsureScript("UIManager.cs", GetUIManagerTemplate());
        EnsureScript("MainMenuToggleController.cs", GetToggleTemplate());
        EnsureScript("MainMenuPanel.cs", GetMainMenuPanelTemplate());
        EnsureScript("SettingsPanel.cs", GetSimplePanelTemplate("SettingsPanel"));
        EnsureScript("LoadGamePanel.cs", GetLoadPanelTemplate());
        EnsureScript("MatchSetupPanel.cs", GetMatchSetupPanelTemplate());
        EnsureScript("StatisticsPanel.cs", GetSimplePanelTemplate("StatisticsPanel"));
        EnsureScript("CraftablesPanel.cs", GetSimplePanelTemplate("CraftablesPanel"));
        EnsureScript("AchievementsPanel.cs", GetSimplePanelTemplate("AchievementsPanel"));
        EnsureScript("CheatsPanel.cs", GetSimplePanelTemplate("CheatsPanel"));
        EnsureScript("CreditsPanel.cs", GetSimplePanelTemplate("CreditsPanel"));

        AssetDatabase.Refresh();
    }

    private static void EnsureScript(string fileName, string content)
    {
        string path = Path.Combine(ScriptsFolder, fileName);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, content);
        }
    }

    private static Canvas EnsureCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
            return canvas;

        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
            return;

        GameObject es = new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));
    }

    private static GameObject CreatePanel(Transform parent, string name, out UIPanel panel)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(560, 620);

        // Background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(root.transform, false);
        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        // Content
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(root.transform, false);
        RectTransform contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = new Vector2(20, 20);
        contentRT.offsetMax = new Vector2(-20, -20);

        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        panel = root.AddComponent<UIPanel>();
        panel.SetRoot(root);

        return root;
    }

    private static void BuildMainMenuLayout(GameObject panelGO, UIPanel panel,
        UIPanel settings, UIPanel load, UIPanel matchSetup,
        UIPanel stats, UIPanel craft, UIPanel achievements,
        UIPanel cheats, UIPanel credits)
    {
        Transform content = panelGO.transform.Find("Content");
        if (content == null) return;

        // Title
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(content, false);
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "MAIN MENU";
        titleTMP.fontSize = 28;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        LayoutElement titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 40;

        // Buttons
        CreateMenuButton(content, "New Game", (button) =>
        {
            AddOnClick(button, panelGO, "MainMenuPanel", "OnNewGamePressed");
        });

        CreateMenuButton(content, "Match Setup", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnMatchSetupPressed", matchSetup);
        });

        CreateMenuButton(content, "Load Game", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnLoadGamePressed", load);
        });

        CreateMenuButton(content, "Settings", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnSettingsPressed", settings);
        });

        CreateMenuButton(content, "Statistics", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnStatisticsPressed", stats);
        });

        CreateMenuButton(content, "Craftables", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnCraftablesPressed", craft);
        });

        CreateMenuButton(content, "Achievements", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnAchievementsPressed", achievements);
        });

        CreateMenuButton(content, "Cheats", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnCheatsPressed", cheats);
        });

        CreateMenuButton(content, "Credits", (button) =>
        {
            AddOnClickWithPanel(button, panelGO, "MainMenuPanel", "OnCreditsPressed", credits);
        });

        // Quit button at bottom: separate object anchored bottom
        GameObject quitGO = new GameObject("QuitButton", typeof(RectTransform), typeof(Button), typeof(TextMeshProUGUI));
        quitGO.transform.SetParent(panelGO.transform, false);
        RectTransform quitRT = quitGO.GetComponent<RectTransform>();
        quitRT.anchorMin = new Vector2(0, 0);
        quitRT.anchorMax = new Vector2(1, 0);
        quitRT.pivot = new Vector2(0.5f, 0);
        quitRT.offsetMin = new Vector2(20, 16);
        quitRT.offsetMax = new Vector2(-20, 16 + 34);

        TextMeshProUGUI quitTMP = quitGO.GetComponent<TextMeshProUGUI>();
        quitTMP.text = "Quit";
        quitTMP.alignment = TextAlignmentOptions.Center;
        quitTMP.fontSize = 18;
        quitTMP.fontStyle = FontStyles.Bold;

        Button quitButton = quitGO.GetComponent<Button>();
        AddOnClick(quitButton, panelGO, "MainMenuPanel", "OnQuitPressed");
    }

    private static void BuildSimplePanelLayout(GameObject panelGO, string title)
    {
        Transform content = panelGO.transform.Find("Content");
        if (content == null) return;

        // Title
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(content, false);
        TextMeshProUGUI titleTMP = titleGO.GetComponent<TextMeshProUGUI>();
        titleTMP.text = title;
        titleTMP.fontSize = 28;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        LayoutElement titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 40;

        // Placeholder label
        GameObject labelGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(content, false);
        TextMeshProUGUI labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
        labelTMP.text = "Content to be implemented.";
        labelTMP.fontSize = 18;
        labelTMP.alignment = TextAlignmentOptions.Midline;

        LayoutElement labelLE = labelGO.AddComponent<LayoutElement>();
        labelLE.preferredHeight = 40;

        // Back button
        GameObject backGO = new GameObject("BackButton", typeof(RectTransform), typeof(Button), typeof(TextMeshProUGUI));
        backGO.transform.SetParent(content, false);
        TextMeshProUGUI backTMP = backGO.GetComponent<TextMeshProUGUI>();
        backTMP.text = "Back";
        backTMP.fontSize = 18;
        backTMP.alignment = TextAlignmentOptions.Center;
        backTMP.fontStyle = FontStyles.Bold;

        LayoutElement backLE = backGO.AddComponent<LayoutElement>();
        backLE.preferredHeight = 34;

        Button backButton = backGO.GetComponent<Button>();

        // Wire Back to show MainMenu via panel script
        UIPanel thisPanel = panelGO.GetComponent<UIPanel>();
        MainMenuPanel mainMenu = Object.FindObjectOfType<MainMenuPanel>();
        if (thisPanel != null && mainMenu != null)
        {
            // We can't know exact method signature here at editor time easily,
            // so this is left for manual wiring if needed.
        }
    }

    private static void CreateMenuButton(Transform parent, string text, System.Action<Button> configure)
    {
        GameObject btnGO = new GameObject(text.Replace(" ", "") + "Button",
            typeof(RectTransform), typeof(Button), typeof(TextMeshProUGUI));
        btnGO.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = btnGO.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;

        LayoutElement le = btnGO.AddComponent<LayoutElement>();
        le.preferredHeight = 34;

        Button button = btnGO.GetComponent<Button>();
        configure?.Invoke(button);
    }

    private static void AddOnClick(Button button, GameObject targetGO, string componentTypeName, string methodName)
    {
        var comp = targetGO.GetComponent(componentTypeName);
        if (comp == null) return;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            button.onClick,
            () => targetGO.SendMessage(methodName, SendMessageOptions.DontRequireReceiver)
        );
    }

    private static void AddOnClickWithPanel(Button button, GameObject targetGO, string componentTypeName, string methodName, UIPanel panelArg)
    {
        var comp = targetGO.GetComponent(componentTypeName);
        if (comp == null || panelArg == null) return;

        // This is a limitation: UnityEventTools doesn't support adding listeners with parameters easily via reflection.
        // You can manually wire these in the Inspector after generation if needed.
    }

    private static UIManager EnsureUIManager()
    {
        UIManager mgr = Object.FindObjectOfType<UIManager>();
        if (mgr != null) return mgr;

        GameObject go = new GameObject("UIManager");
        mgr = go.AddComponent<UIManager>();
        return mgr;
    }

    // --------- Script templates (minimal, in case files are missing) ---------

    private static string GetUIPanelTemplate() =>
@"using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;

    public bool IsVisible => root != null && root.activeSelf;

    public virtual void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public virtual void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void SetRoot(GameObject r)
    {
        root = r;
    }
}
";

    private static string GetUIManagerTemplate() =>
@"using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private List<UIPanel> panels = new List<UIPanel>();
    [SerializeField] private UIPanel mainMenuPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterPanel(UIPanel panel)
    {
        if (panel != null && !panels.Contains(panel))
            panels.Add(panel);
    }

    public void ShowPanel(UIPanel panel)
    {
        if (panel == null) return;

        foreach (var p in panels)
        {
            if (p == null) continue;
            if (p == panel) p.Show();
            else p.Hide();
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            ShowPanel(mainMenuPanel);
    }

    public void HideAll()
    {
        foreach (var p in panels)
            p?.Hide();
    }

    public void SetMainMenu(UIPanel panel)
    {
        mainMenuPanel = panel;
        RegisterPanel(panel);
    }
}
";

    private static string GetToggleTemplate() =>
@"using UnityEngine;

public class MainMenuToggleController : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
    [SerializeField] private UIPanel mainMenuPanel;

    bool isOpen;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        if (mainMenuPanel == null) return;

        isOpen = !isOpen;
        if (isOpen)
        {
            mainMenuPanel.Show();
            if (RTSGameSettings.Instance != null &&
                RTSGameSettings.Instance.gameplay.pauseOnMenuOpen)
            {
                Time.timeScale = 0f;
            }
        }
        else
        {
            mainMenuPanel.Hide();
            RTSGameSettings.Instance?.Apply();
        }
    }

    public void SetMainMenu(UIPanel panel)
    {
        mainMenuPanel = panel;
    }
}
";

    private static string GetMainMenuPanelTemplate() =>
@"using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuPanel : UIPanel
{
    public void OnNewGamePressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
";

    private static string GetSimplePanelTemplate(string className) =>
$@"using UnityEngine;

public class {className} : UIPanel
{{
    public void OnBackPressed(UIPanel mainMenu)
    {{
        UIManager.Instance.ShowPanel(mainMenu);
    }}
}}
";

    private static string GetLoadPanelTemplate() =>
@"using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGamePanel : UIPanel
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
";

    private static string GetMatchSetupPanelTemplate() =>
@"using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchSetupPanel : UIPanel
{
    public void OnStartWithSettingsPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
";
}