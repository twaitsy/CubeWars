using UnityEngine;

public class ConstructionWorkerControl : MonoBehaviour
{
    [SerializeField] private float buildSpeed = 1f;
    [SerializeField] private bool canHaulMaterials = true;
    [SerializeField] private float retargetSeconds = 0.6f;

    public float BuildSpeed => Mathf.Max(0.01f, buildSpeed);
    public bool CanHaulMaterials => canHaulMaterials;
    public float RetargetSeconds => Mathf.Max(0.05f, retargetSeconds);

    public void SetBuildSpeed(float value)
    {
        buildSpeed = Mathf.Max(0.01f, value);
    }

    public void SetHaulCapability(bool canHaul)
    {
        canHaulMaterials = canHaul;
    }

    public void SetRetargetSeconds(float value)
    {
        retargetSeconds = Mathf.Max(0.05f, value);
    }
}
