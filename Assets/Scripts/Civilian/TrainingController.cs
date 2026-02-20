using UnityEngine;
using System.Collections.Generic;

public class TrainingController : MonoBehaviour
{
    [SerializeField] private List<ResourceAmount> trainingCost;
    [SerializeField] private float trainingTime;
    [SerializeField] private BuildingDefinition trainedAt;

    public void SetTraining(List<ResourceAmount> cost, float time, BuildingDefinition building)
    {
        trainingCost = cost;
        trainingTime = time;
        trainedAt = building;
    }
}