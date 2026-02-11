using UnityEngine;

public class CheatsPanel : UIPanel
{
    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
