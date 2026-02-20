using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BuildingInteractionPointController : MonoBehaviour
{
    [SerializeField] private InteractionPoint[] interactionPoints;

    public IReadOnlyList<InteractionPoint> InteractionPoints
    {
        get
        {
            CachePointsIfNeeded();
            return interactionPoints;
        }
    }

    void Awake()
    {
        CachePointsIfNeeded();
    }

    [ContextMenu("Refresh Interaction Points")]
    public void RefreshInteractionPoints()
    {
        interactionPoints = GetComponentsInChildren<InteractionPoint>(true);
    }

    public bool TryGetClosestPoint(BuildingInteractionPointType pointType, Vector3 fromPosition, out Transform point)
    {
        point = null;
        CachePointsIfNeeded();

        float bestDistance = float.MaxValue;
        int bestPriority = int.MinValue;

        for (int i = 0; i < interactionPoints.Length; i++)
        {
            InteractionPoint candidate = interactionPoints[i];
            if (candidate == null || candidate.pointType != pointType)
                continue;

            float sqrDistance = (candidate.Position - fromPosition).sqrMagnitude;
            if (candidate.priority > bestPriority || (candidate.priority == bestPriority && sqrDistance < bestDistance))
            {
                bestPriority = candidate.priority;
                bestDistance = sqrDistance;
                point = candidate.transform;
            }
        }

        return point != null;
    }

    public List<Transform> GetPoints(BuildingInteractionPointType pointType)
    {
        CachePointsIfNeeded();
        var points = new List<Transform>();

        for (int i = 0; i < interactionPoints.Length; i++)
        {
            InteractionPoint candidate = interactionPoints[i];
            if (candidate != null && candidate.pointType == pointType)
                points.Add(candidate.transform);
        }

        return points;
    }

    void CachePointsIfNeeded()
    {
        if (interactionPoints == null || interactionPoints.Length == 0)
            RefreshInteractionPoints();
    }
}
