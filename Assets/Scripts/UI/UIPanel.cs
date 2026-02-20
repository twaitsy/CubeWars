using UnityEngine;

public class UIPanel : MonoBehaviour
{
    [SerializeField] private GameObject root;

    public bool IsVisible => root != null && root.activeSelf;

    public virtual void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public virtual void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void SetRoot(GameObject r)
    {
        root = r;
    }
}
