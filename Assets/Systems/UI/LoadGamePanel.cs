using UnityEngine;
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
