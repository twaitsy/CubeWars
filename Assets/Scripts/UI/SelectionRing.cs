using UnityEngine;

public class SelectionRing : MonoBehaviour
{
    public int teamID;
    public VisualKind kind = VisualKind.Unit;

    [Header("Ring")]
    public float radius = 0.75f;
    public float height = 0.05f;
    public float yOffset = -0.45f;

    void Start()
    {
        CreateRing();
    }

    void CreateRing()
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(transform, false);

        ring.transform.localPosition = new Vector3(0f, yOffset, 0f);
        ring.transform.localScale = new Vector3(radius, height, radius);

        // Remove collider (important)
        var col = ring.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Color by team + type
        if (TeamColorManager.Instance != null)
        {
            Color baseColor = TeamColorManager.Instance.GetTeamColor(teamID);
            Color final = baseColor;

            if (kind == VisualKind.Civilian) final = Color.Lerp(baseColor, Color.white, 0.55f);
            if (kind == VisualKind.Building) final = Color.Lerp(baseColor, Color.black, 0.15f);

            var rend = ring.GetComponent<Renderer>();
            if (rend != null && rend.material != null) rend.material.color = final;
        }
    }
}
