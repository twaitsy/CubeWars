using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Toggle")]
    public bool show = true;
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("Panel Layout (Centered)")]
    public int panelWidth = 300;
    public int panelHeight = 400;
    public int buttonHeight = 40;
    public int buttonSpacing = 10;

    [Header("Styles")]
    public int fontSize = 18;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private bool stylesInitialized = false;

    [Header("Load Menu")]
    public string[] loadableScenes;
    private bool showLoadMenu = false;

    [Header("Credits")]
    [TextArea(5, 10)]
    public string creditsText =
        "Twaitsy";
    private bool showCredits = false;

    public bool IsVisible => show;
    public void SetVisible(bool visible) => show = visible;
    public void ToggleVisible() => show = !show;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            ToggleVisible();
    }

    void OnGUI()
    {
        if (!show)
            return;

        // Lazy initialization — safe in Unity 2019
        if (!stylesInitialized)
        {
            InitStyles();
            stylesInitialized = true;
        }

        if (showLoadMenu)
        {
            DrawLoadMenu();
            return;
        }

        if (showCredits)
        {
            DrawCredits();
            return;
        }

        DrawMainMenu();
    }

    private void InitStyles()
    {
        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = fontSize,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        boxStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = fontSize + 4,
            alignment = TextAnchor.UpperCenter,
            fontStyle = FontStyle.Bold
        };

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            wordWrap = true,
            alignment = TextAnchor.UpperLeft
        };
    }

    private void DrawMainMenu()
    {
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "MAIN MENU", boxStyle);

        float x = panelRect.x + 20;
        float y = panelRect.y + 40;
        float btnWidth = panelWidth - 40;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "New Game", buttonStyle))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Load Game", buttonStyle))
        {
            showLoadMenu = true;
        }
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Settings", buttonStyle))
        {
            Debug.Log("Settings selected");
        }
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Credits", buttonStyle))
        {
            showCredits = true;
        }
        y += buttonHeight + buttonSpacing;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Quit", buttonStyle))
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void DrawLoadMenu()
    {
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "LOAD GAME", boxStyle);

        float x = panelRect.x + 20;
        float y = panelRect.y + 40;
        float btnWidth = panelWidth - 40;

        foreach (string sceneName in loadableScenes)
        {
            if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), $"Load {sceneName}", buttonStyle))
            {
                SceneManager.LoadScene(sceneName);
            }
            y += buttonHeight + buttonSpacing;
        }

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Back", buttonStyle))
        {
            showLoadMenu = false;
        }
    }

    private void DrawCredits()
    {
        float left = (Screen.width - panelWidth) / 2f;
        float top = (Screen.height - panelHeight) / 2f;
        Rect panelRect = new Rect(left, top, panelWidth, panelHeight);

        GUI.Box(panelRect, "CREDITS", boxStyle);

        float x = panelRect.x + 20;
        float y = panelRect.y + 40;
        float contentHeight = panelHeight - 80;
        float btnWidth = panelWidth - 40;

        GUI.Label(new Rect(x, y, btnWidth, contentHeight - buttonHeight - buttonSpacing),
                  creditsText, labelStyle);

        y += contentHeight - buttonHeight;

        if (GUI.Button(new Rect(x, y, btnWidth, buttonHeight), "Back", buttonStyle))
        {
            showCredits = false;
        }
    }
}