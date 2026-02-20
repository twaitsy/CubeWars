using UnityEngine;
using UnityEngine.UIElements;

public class UnitInspectorUIController : MonoBehaviour
{
    public UIDocument doc;

    VisualElement root;
    Label header;

    void OnEnable()
    {
        root = doc.rootVisualElement;

        foreach (var e in root.Query<Label>().ToList())
        {
            Debug.Log("Found Label: " + e.name + " | text=" + e.text);
        }

        header = root.Q<Label>("header");
        Debug.Log("Header element text BEFORE: " + header.text);

        header.text = "Hello UI Toolkit!";
        Debug.Log("Header element text AFTER: " + header.text);
    }

}