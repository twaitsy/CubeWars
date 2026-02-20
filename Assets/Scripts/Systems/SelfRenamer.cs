using UnityEngine;

public class SelfRenamer : MonoBehaviour
{
    void Awake()
    {
        gameObject.name = UnitNamePool.GetRandomDisplayName();
    }
}
