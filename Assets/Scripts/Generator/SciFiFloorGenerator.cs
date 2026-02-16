using UnityEngine;

[ExecuteInEditMode] // ← Bonus: now works in the Editor too (preview!)
public abstract class SciFiFloorGenerator : MonoBehaviour
{
    [Header("Target (auto-detected if left empty)")]
    public Renderer targetRenderer;
    public Material targetMaterial;

    public int textureSize = 512;

    private Texture2D generatedTexture;

    private void Awake()
    {
        GenerateAndApply();
    }

    // Also update in Editor when you change values
    private void OnValidate()
    {
        if (!Application.isPlaying && gameObject.activeInHierarchy)
            GenerateAndApply();
    }

    private void GenerateAndApply()
    {
        // Auto-detect if nothing is assigned
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        generatedTexture = GenerateTexture();

        if (targetMaterial != null)
        {
            targetMaterial.mainTexture = generatedTexture;
        }
        else if (targetRenderer != null)
        {
            // Creates a material instance so we don't modify the original asset
            targetRenderer.material.mainTexture = generatedTexture;
        }
        else
        {
            Debug.LogError("No Renderer or Material assigned on " + gameObject.name);
        }
    }

    private Texture2D GenerateTexture()
    {
        Texture2D tex = new(textureSize, textureSize, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float u = (float)x / textureSize;
                float v = (float)y / textureSize;
                pixels[y * textureSize + x] = GetPixelColor(u, v);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply(true, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Trilinear;
        tex.anisoLevel = 8;
        return tex;
    }

    protected abstract Color GetPixelColor(float u, float v);
}