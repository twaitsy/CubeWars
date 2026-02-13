using UnityEngine;

public class WorldProgressBar : MonoBehaviour
{
    [Header("Position")]
    public Vector3 offset = new Vector3(0f, 1.4f, 0f);

    [Header("Size")]
    public float width = 1.2f;
    public float height = 0.12f;
    public float depth = 0.08f;

    [Header("Display")]
    [Range(0f, 1f)] public float progress01 = 0f;
    public bool hideWhenZero = true;

    [Header("Smoothing")]
    [Tooltip("Higher = snappier, lower = smoother. 0 disables smoothing.")]
    public float smoothSpeed = 18f;

    [Header("Sci-Fi Glow")]
    [Range(0f, 6f)] public float glowStrength = 1.8f;

    [Header("Billboard")]
    public bool billboardToCamera = true;

    Transform holder;
    Transform barFill;
    Transform barBack;

    Renderer fillRenderer;
    Renderer backRenderer;

    float displayedT;

    MaterialPropertyBlock mpbFill;
    MaterialPropertyBlock mpbBack;

    static readonly int ColorID = Shader.PropertyToID("_Color");
    static readonly int EmissionID = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        displayedT = Mathf.Clamp01(progress01);
        mpbFill = new MaterialPropertyBlock();
        mpbBack = new MaterialPropertyBlock();
        CreateBar();
    }

    void LateUpdate()
    {
        if (holder == null)
            return;

        float rawT = Mathf.Clamp01(progress01);
        if (smoothSpeed > 0f)
            displayedT = Mathf.Lerp(displayedT, rawT, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        else
            displayedT = rawT;

        holder.gameObject.SetActive(!hideWhenZero || displayedT > 0.001f);

        if (barFill != null)
        {
            barFill.localScale = new Vector3(width * displayedT, height, depth);
            barFill.localPosition = new Vector3(-(width * 0.5f) + (width * displayedT * 0.5f), 0f, -0.001f);
        }

        Color fillColor = Color.Lerp(new Color(0.2f, 0.7f, 1f), new Color(0.55f, 0.2f, 1f), displayedT);
        ApplySciFiColor(fillRenderer, mpbFill, fillColor, glowStrength);

        if (billboardToCamera && Camera.main != null)
            holder.rotation = Camera.main.transform.rotation;
    }

    void CreateBar()
    {
        holder = new GameObject("ProgressBar").transform;
        holder.SetParent(transform, false);
        holder.localPosition = offset;

        barBack = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        barBack.name = "Back";
        barBack.SetParent(holder, false);
        barBack.localPosition = new Vector3(0f, 0f, +0.001f);
        barBack.localScale = new Vector3(width, height, depth);

        barFill = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        barFill.name = "Fill";
        barFill.SetParent(holder, false);
        barFill.localPosition = new Vector3(0f, 0f, -0.001f);
        barFill.localScale = new Vector3(width, height, depth);

        Destroy(barBack.GetComponent<Collider>());
        Destroy(barFill.GetComponent<Collider>());

        backRenderer = barBack.GetComponent<Renderer>();
        fillRenderer = barFill.GetComponent<Renderer>();

        ApplySciFiColor(backRenderer, mpbBack, new Color(0.02f, 0.02f, 0.05f), 0f);
        ApplySciFiColor(fillRenderer, mpbFill, new Color(0.35f, 0.7f, 1f), glowStrength);
    }

    void ApplySciFiColor(Renderer r, MaterialPropertyBlock block, Color baseColor, float emissionStrength)
    {
        if (r == null)
            return;

        r.GetPropertyBlock(block);
        block.SetColor(ColorID, baseColor);
        block.SetColor(EmissionID, baseColor * emissionStrength);
        r.SetPropertyBlock(block);
    }
}
