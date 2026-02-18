using UnityEngine;

public class GatheringControl : MonoBehaviour
{
    [SerializeField] private float gatherTickSeconds = 1f;
    [SerializeField] private int harvestPerTick = 1;
    [SerializeField] private float searchRetrySeconds = 1.5f;

    public float GatherTickSeconds => Mathf.Max(0.01f, gatherTickSeconds);
    public int HarvestPerTick => Mathf.Max(1, harvestPerTick);
    public float SearchRetrySeconds => Mathf.Max(0.1f, searchRetrySeconds);

    public void SetGatherSpeed(float value)
    {
        gatherTickSeconds = Mathf.Max(0.01f, value);
    }

    public void SetHarvestPerTick(int value)
    {
        harvestPerTick = Mathf.Max(1, value);
    }

    public void SetSearchRetry(float value)
    {
        searchRetrySeconds = Mathf.Max(0.1f, value);
    }
}
