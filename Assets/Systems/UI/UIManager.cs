using UnityEngine;
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
