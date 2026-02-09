using UnityEngine;

public class ParentRenamer : MonoBehaviour
{
    void Awake()
    {
        Transform parentTransform = transform.parent;
        if (parentTransform == null) return;

        parentTransform.name = UnitNamePool.GetRandomDisplayName();
    }
}
