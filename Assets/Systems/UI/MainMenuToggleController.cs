using UnityEngine;

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
