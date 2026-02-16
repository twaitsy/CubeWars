using UnityEngine;

public class VentGrateGenerator : SciFiFloorGenerator
{
    public Color metalColor = new Color(0.35f, 0.35f, 0.4f);
    public Color holeColor = new Color(0.1f, 0.1f, 0.15f);
    public float grateSize = 0.22f;
    public float holeRadius = 0.035f;

    protected override Color GetPixelColor(float u, float v)
    {
        // Grate bars (crosshatch)
        float gu = Mathf.Repeat(u / grateSize, 1f);
        float gv = Mathf.Repeat(v / grateSize, 1f);
        float barU = Mathf.Min(gu, 1f - gu);
        float barV = Mathf.Min(gv, 1f - gv);
        float bar = Mathf.Max(barU, barV);
        bool isBar = bar > 0.08f;

        // Vent holes
        float holeU = Mathf.Repeat(u / grateSize - 0.5f, 1f);
        float holeV = Mathf.Repeat(v / grateSize - 0.5f, 1f);
        float holeDist = Mathf.Sqrt(holeU * holeU + holeV * holeV);
        bool isHole = holeDist < holeRadius;

        Color col = isHole ? holeColor : metalColor;
        if (isBar) col *= 1.2f;  // Fixed: no .rgb

        // Edge bevel (recessed centers)
        float edgeDist = Mathf.Min(Mathf.Abs(Mathf.Repeat(u, grateSize) - grateSize / 2f),
                                   Mathf.Abs(Mathf.Repeat(v, grateSize) - grateSize / 2f));
        col *= 1f - 0.1f * (1f - Mathf.SmoothStep(0f, 0.01f, edgeDist));  // Fixed: SmoothStep, no .rgb

        return col;
    }
}