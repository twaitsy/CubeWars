using UnityEngine;

public class ProductionStatusVisualizer : MonoBehaviour
{
    public CraftingBuilding building;
    public GameObject missingInputIcon;
    public GameObject outputFullIcon;
    public GameObject noWorkerIcon;

    void LateUpdate()
    {
        if (building == null)
            return;

        bool missingInputs = building.State == CraftingBuilding.ProductionState.WaitingForInputs;
        bool outputBlocked = building.State == CraftingBuilding.ProductionState.WaitingForPickup;
        bool noWorkers = building.State == CraftingBuilding.ProductionState.InputsReady && building.AssignedWorkers.Count == 0;

        if (missingInputIcon != null) missingInputIcon.SetActive(missingInputs);
        if (outputFullIcon != null) outputFullIcon.SetActive(outputBlocked);
        if (noWorkerIcon != null) noWorkerIcon.SetActive(noWorkers);
    }
}
