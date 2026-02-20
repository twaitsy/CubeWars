using UnityEngine;

public class HexGridGenerator : SciFiFloorGenerator
{
    public Color hexColor = new(0.2f, 0.2f, 0.25f);
    public Color edgeColor = new(0.4f, 0.4f, 0.5f);
    public float hexScale = 8f;
    public float edgeThickness = 0.03f;

    protected override Color GetPixelColor(float u, float v)
    {
        float uu = u * hexScale;
        float vv = v * hexScale * (Mathf.Sqrt(3f) / 2f);  // Proper hex aspect

        Vector2 gv = new(Mathf.Floor(uu), Mathf.Floor(vv));  // Fixed: Vector2
        Vector2 f = new(Mathf.Repeat(uu, 1f), Mathf.Repeat(vv, 1f));  // Fixed: Vector2

        // Hex SDF (signed distance)
        Vector2 vertex = f - new Vector2(0.5f, Mathf.Sqrt(3f) / 6f);
        Vector2 offset = new Vector2(1f, Mathf.Sqrt(3f) / 3f) * 0.5f;
        float d1 = Mathf.Sqrt(vertex.x * vertex.x + vertex.y * vertex.y);
        vertex -= offset;
        float d2 = Mathf.Sqrt(vertex.x * vertex.x + vertex.y * vertex.y);
        float hexDist = Mathf.Max(d1, d2) - 0.5f;

        // Edge highlight (fixed falloff)
        float edge = Mathf.Abs(hexDist);
        float edgeMix = 1f - Mathf.SmoothStep(0f, edgeThickness, edge);  // Fixed
        Color col = Color.Lerp(hexColor, edgeColor, edgeMix);

        // Inner noise (tileable)
        col *= 0.9f + 0.1f * Mathf.PerlinNoise(gv.x * 0.5f, gv.y * 0.5f);  // Fixed: no .rgb

        return col;
    }
}