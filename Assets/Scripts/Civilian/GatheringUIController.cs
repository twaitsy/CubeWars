using UnityEngine;

[RequireComponent(typeof(Transform))]
public class GatheringUIController : MonoBehaviour
{
    [Header("Gather UI")]
    [SerializeField] private bool showGatherProgressBar = true;

    [SerializeField] private Vector3 gatherProgressBarOffset = new(0f, 2f, 0f);
    [SerializeField] private float gatherProgressBarWidth = 1f;
    [SerializeField] private float gatherProgressBarHeight = 0.15f;

    [Header("Gather State (Driven Externally)")]
    public float gatherTickSeconds = 2f;
    public float gatherTimer = 0f;
    public enum State
    {
        Idle,
        Gathering
    }

    public State state = State.Idle;

    private WorldProgressBar gatherProgressBar;

    void Start()
    {
        EnsureGatherProgressBar();
    }

    void Update()
    {
        UpdateGatherProgressBar();
    }

    void EnsureGatherProgressBar()
    {
        if (!showGatherProgressBar)
            return;

        if (gatherProgressBar == null)
            gatherProgressBar = GetComponent<WorldProgressBar>();

        if (gatherProgressBar == null)
            gatherProgressBar = gameObject.AddComponent<WorldProgressBar>();

        if (gatherProgressBar == null)
            return;

        gatherProgressBar.offset = gatherProgressBarOffset;
        gatherProgressBar.width = Mathf.Max(0.1f, gatherProgressBarWidth);
        gatherProgressBar.height = Mathf.Max(0.02f, gatherProgressBarHeight);
        gatherProgressBar.hideWhenZero = true;
        gatherProgressBar.progress01 = 0f;
    }

    void UpdateGatherProgressBar()
    {
        if (!showGatherProgressBar)
            return;

        if (gatherProgressBar == null)
            EnsureGatherProgressBar();

        if (gatherProgressBar == null)
            return;

        if (state != State.Gathering || gatherTickSeconds <= 0f)
        {
            gatherProgressBar.progress01 = 0f;
            return;
        }

        gatherProgressBar.progress01 =
            Mathf.Clamp01(gatherTimer / Mathf.Max(0.01f, gatherTickSeconds));
    }
}