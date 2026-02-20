using UnityEngine;

public enum BuildingStopDistanceType
{
    Default,
    House,
    Construction,
    Storage,
    CraftInput,
    CraftOutput,
    CraftWork
}

public class BuildingInteractionSettings : MonoBehaviour
{
    [Header("Civilian Stop Distances")]
    [Min(0.1f)] public float defaultStopDistance = 1.2f;
    [Min(0.1f)] public float houseStopDistance = 1f;
    [Min(0.1f)] public float constructionStopDistance = 1.25f;
    [Min(0.1f)] public float storageStopDistance = 1.1f;
    [Min(0.1f)] public float craftInputStopDistance = 0.8f;
    [Min(0.1f)] public float craftOutputStopDistance = 0.8f;
    [Min(0.1f)] public float craftWorkStopDistance = 0.6f;

    public float GetStopDistance(BuildingStopDistanceType type, float fallback)
    {
        switch (type)
        {
            case BuildingStopDistanceType.House: return houseStopDistance;
            case BuildingStopDistanceType.Construction: return constructionStopDistance;
            case BuildingStopDistanceType.Storage: return storageStopDistance;
            case BuildingStopDistanceType.CraftInput: return craftInputStopDistance;
            case BuildingStopDistanceType.CraftOutput: return craftOutputStopDistance;
            case BuildingStopDistanceType.CraftWork: return craftWorkStopDistance;
            default: return Mathf.Max(0.1f, defaultStopDistance > 0f ? defaultStopDistance : fallback);
        }
    }
}
