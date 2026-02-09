using UnityEngine;

public class ParentRenamer : MonoBehaviour
{
    void Awake()
    {
        Transform parent = transform.parent;
        if (parent == null) return;

        var assigner = parent.GetComponent<UnitNameAssigner>();
        if (assigner == null) assigner = parent.gameObject.AddComponent<UnitNameAssigner>();
    }
}
