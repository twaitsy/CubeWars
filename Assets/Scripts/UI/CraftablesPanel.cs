using UnityEngine;

public class CraftablesPanel : UIPanel
{
    public void OnBackPressed(UIPanel mainMenu)
    {
        UIManager.Instance.ShowPanel(mainMenu);
    }
}
