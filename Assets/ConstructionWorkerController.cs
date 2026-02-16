using UnityEngine;

public class ConstructionWorkerController : MonoBehaviour
{
    [SerializeField] private float buildSpeed;

    public float BuildSpeed => buildSpeed;

    public void SetBuildSpeed(float value)
    {
        buildSpeed = value;
    }
}