using UnityEngine;

public class ResourceVisualBuilder : MonoBehaviour
{
    public ResourceDefinition resource;
    public Color defaultColor = Color.white;

    public Color GetColor()
    {
        if (resource == null || string.IsNullOrWhiteSpace(resource.id))
            return defaultColor;

        string id = resource.id.ToLowerInvariant();
        if (id.Contains("wood")) return new Color(0.4f, 0.2f, 0.1f);
        if (id.Contains("stone")) return new Color(0.5f, 0.5f, 0.5f);
        if (id.Contains("gold")) return Color.yellow;
        return defaultColor;
    }
}
