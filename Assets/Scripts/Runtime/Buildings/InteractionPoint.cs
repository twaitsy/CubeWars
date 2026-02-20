using UnityEngine;

public enum BuildingInteractionPointType
{
    Default,
    Storage,
    CraftInput,
    CraftOutput,
    CraftWork,
    House
}

[DisallowMultipleComponent]
public class InteractionPoint : MonoBehaviour
{
    public BuildingInteractionPointType pointType = BuildingInteractionPointType.Default;
    [Min(0)] public int priority = 0;

    public Vector3 Position => transform.position;
}
