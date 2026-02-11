using UnityEngine;
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
