using UnityEngine;

public class GatheringController : MonoBehaviour
{
    [SerializeField] private float gatherSpeed;

    public float GatherSpeed => gatherSpeed;

    public void SetGatherSpeed(float value)
    {
        gatherSpeed = value;
    }
}