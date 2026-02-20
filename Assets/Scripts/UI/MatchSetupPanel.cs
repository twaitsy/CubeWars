using UnityEngine;
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
