using UnityEngine;

public class StatisticsPanel : UIPanel
{
    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
