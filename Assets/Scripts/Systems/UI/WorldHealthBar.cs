using UnityEngine;

public class WorldHealthBar : MonoBehaviour
{
    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 1.2f, 0f);

    [Header("Size")]
    public float width = 1.2f;
    public float height = 0.12f;
    public float depth = 0.08f;

    [Header("Display Rules")]
    public bool onlyShowForBuildings = true;
    public bool hideWhenFullHealth = true;

    [Header("Smoothing")]
    [Tooltip("Higher = snappier, lower = smoother. 0 disables smoothing.")]
    public float smoothSpeed = 18f;

    [Header("Sci-Fi Glow")]
    [Range(0f, 6f)] public float glowStrength = 1.8f;

    [Header("Billboard")]
    public bool billboardToCamera = true;

    private IHasHealth health;

    private Transform holder;
    private Transform barFill;
    private Transform barBack;

    private Renderer fillRenderer;
    private Renderer backRenderer;

    private float displayedT = 1f;

    private MaterialPropertyBlock mpbFill;
    private MaterialPropertyBlock mpbBack;

    static readonly int ColorID = Shader.PropertyToID("_Color");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        health = GetComponent<IHasHealth>();
        if (health == null)
        {
            enabled = false;
            return;
        }

        if (onlyShowForBuildings && GetComponent<Building>() == null)
        {
            enabled = false;
            return;
        }

        mpbFill = new MaterialPropertyBlock();
        mpbBack = new MaterialPropertyBlock();

        CreateBar();
    }

    void LateUpdate()
    {
        if (health == null || holder == null) return;

        float rawT = Mathf.Clamp01(health.CurrentHealth / Mathf.Max(0.01f, health.MaxHealth));

        // Smooth the displayed value (prevents shimmer)
        if (smoothSpeed > 0f)
            displayedT = Mathf.Lerp(displayedT, rawT, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            displayedT = rawT;

        holder.gameObject.SetActive(displayedT < 0.999f);

        // Update fill width + pivot so it shrinks from left-to-right
        if (barFill != null)
        {
            barFill.localScale = new Vector3(width * displayedT, height, depth);
            barFill.localPosition = new Vector3(-(width * 0.5f) + (width * displayedT * 0.5f), 0f, -0.001f);
        }

        // Color shifts Green -> Yellow -> Red
        Color fillColor = (displayedT > 0.5f)
            ? Color.Lerp(Color.yellow, Color.green, (displayedT - 0.5f) * 2f)
            : Color.Lerp(Color.red, Color.yellow, displayedT * 2f);

        ApplySciFiColor(fillRenderer, mpbFill, fillColor, glowStrength);

        // Stable billboard (no LookRotation jitter)
        if (billboardToCamera && Camera.main != null)
        {
            holder.rotation = Camera.main.transform.rotation;
        }
    }

    void CreateBar()
    {
        holder = new GameObject("HealthBar").transform;
        holder.SetParent(transform, false);
        holder.localPosition = offset;

        // Background (slightly behind fill to prevent z-fighting)
        barBack = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        barBack.name = "Back";
        barBack.SetParent(holder, false);
        barBack.localPosition = new Vector3(0f, 0f, +0.001f);
        barBack.localScale = new Vector3(width, height, depth);

        // Fill (slightly in front)
        barFill = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        barFill.name = "Fill";
        barFill.SetParent(holder, false);
        barFill.localPosition = new Vector3(0f, 0f, -0.001f);
        barFill.localScale = new Vector3(width, height, depth);

        // Remove colliders
        Destroy(barBack.GetComponent<Collider>());
        Destroy(barFill.GetComponent<Collider>());

        backRenderer = barBack.GetComponent<Renderer>();
        fillRenderer = barFill.GetComponent<Renderer>();

        // Dark sci-fi plate
        ApplySciFiColor(backRenderer, mpbBack, new Color(0.02f, 0.02f, 0.05f), 0.0f);

        // Start fill color
        ApplySciFiColor(fillRenderer, mpbFill, Color.green, glowStrength);
    }

    void ApplySciFiColor(Renderer r, MaterialPropertyBlock block, Color baseColor, float emissionStrength)
    {
        if (r == null) return;

        r.GetPropertyBlock(block);
        block.SetColor(ColorID, baseColor);
        block.SetColor(EmissionID, baseColor * emissionStrength);
        r.SetPropertyBlock(block);
    }
}
