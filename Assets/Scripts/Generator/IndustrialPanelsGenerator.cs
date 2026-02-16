using UnityEngine;

public class IndustrialPanelsGenerator : SciFiFloorGenerator
{
    public Color baseColor = new Color(0.25f, 0.25f, 0.3f);
    public Color seamColor = new Color(0.6f, 0.6f, 0.7f);
    public float panelSize = 0.18f;
    public float seamWidth = 0.015f;
    public float grunge = 0.15f;

    protected override Color GetPixelColor(float u, float v)
    {
        // Panel seams
        float du = Mathf.Abs(Mathf.Repeat(u, panelSize) - panelSize * 0.5f) / (panelSize * 0.5f);
        float dv = Mathf.Abs(Mathf.Repeat(v, panelSize) - panelSize * 0.5f) / (panelSize * 0.5f);
        float seam = Mathf.Min(du, dv);
        float mixAmt = Mathf.SmoothStep(0f, seamWidth, 1f - seam);   // ← Fixed

        Color col = Color.Lerp(baseColor, seamColor, mixAmt);

        // Grunge noise
        float noise = (Mathf.PerlinNoise(u * 8f, v * 8f) - 0.5f) * grunge;
        col *= (1f + noise);   // ← Fixed (no .rgb)

        // Rivets at corners
        float rivetU = Mathf.Repeat(u / panelSize, 1f);
        float rivetV = Mathf.Repeat(v / panelSize, 1f);
        float rivetDist = Mathf.Sqrt((rivetU - 0.1f) * (rivetU - 0.1f) + (rivetV - 0.1f) * (rivetV - 0.1f));
        if (rivetDist < 0.015f)
            col = seamColor;   // ← Fixed

        return col;
    }
}