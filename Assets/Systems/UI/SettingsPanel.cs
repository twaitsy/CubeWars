using UnityEngine;

public class SettingsPanel : UIPanel
{
    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
