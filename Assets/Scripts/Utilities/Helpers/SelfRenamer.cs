using UnityEngine;

public class SelfRenamer : MonoBehaviour
{
    void Awake()
    {
        var assigner = GetComponent<UnitNameAssigner>();
        if (assigner == null) assigner = gameObject.AddComponent<UnitNameAssigner>();
    }
}
