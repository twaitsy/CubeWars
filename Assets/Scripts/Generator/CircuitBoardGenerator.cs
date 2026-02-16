using UnityEngine;

public class CircuitBoardGenerator : SciFiFloorGenerator
{
    public Color boardColor = new Color(0.05f, 0.2f, 0.05f);
    public Color traceColor = new Color(0.1f, 0.8f, 0.4f);
    public float traceDensity = 12f;
    public float traceWidth = 0.008f;

    protected override Color GetPixelColor(float u, float v)
    {
        Color col = boardColor;

        // Horizontal traces (fixed falloff)
        float hSin = Mathf.Sin(u * traceDensity * Mathf.PI * 2f + v * 3f) * 0.5f + 0.5f;
        float hTrace = Mathf.Abs(hSin - 0.5f) * 2f;
        float hAmount = 1f - Mathf.SmoothStep(0f, traceWidth, hTrace);
        col = Color.Lerp(col, traceColor, hAmount);

        // Vertical traces (fixed falloff)
        float vSin = Mathf.Sin(v * traceDensity * Mathf.PI * 2f + u * 2f) * 0.5f + 0.5f;
        float vTrace = Mathf.Abs(vSin - 0.5f) * 2f;
        float vAmount = 1f - Mathf.SmoothStep(0f, traceWidth, vTrace);
        col = Color.Lerp(col, traceColor, vAmount);

        // Pads (circles at intersections) - already correct
        float padU = Mathf.Repeat(u * traceDensity, 1f) - 0.5f;
        float padV = Mathf.Repeat(v * traceDensity, 1f) - 0.5f;
        float padDist = Mathf.Sqrt(padU * padU + padV * padV);
        if (padDist < 0.04f)
        {
            float padGlow = 1f - Mathf.SmoothStep(0f, 0.04f, padDist);
            col = Color.Lerp(col, traceColor, padGlow);
        }

        return col;
    }
}