using UnityEngine;

public class AchievementsPanel : UIPanel
{
    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
