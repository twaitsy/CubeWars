using UnityEngine;

public class UpgradeController : MonoBehaviour
{
    [SerializeField] private UnitDefinition upgradeTarget;

    public void SetUpgradeTarget(UnitDefinition def)
    {
        upgradeTarget = def;
    }
}