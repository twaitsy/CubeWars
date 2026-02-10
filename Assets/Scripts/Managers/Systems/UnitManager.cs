using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    [Header("Formation")]
    [Min(0.2f)] public float formationSpacing = 1.4f;

    readonly List<Unit> units = new List<Unit>(256);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshRegistry();
    }

    public void RefreshRegistry()
    {
        units.Clear();
        units.AddRange(FindObjectsOfType<Unit>());
    }

    public void Register(Unit unit)
    {
        if (unit != null && !units.Contains(unit))
            units.Add(unit);
    }

    public void Unregister(Unit unit)
    {
        units.Remove(unit);
    }

    public IReadOnlyList<Unit> GetUnits() => units;

    public Vector3 GetFormationOffset(int index, int count, Vector3 forward)
    {
        if (count <= 1)
            return Vector3.zero;

        int rowSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        int row = index / rowSize;
        int col = index % rowSize;

        float half = (rowSize - 1) * 0.5f;
        Vector3 right = Vector3.Cross(Vector3.up, forward.normalized);
        if (right.sqrMagnitude < 0.01f)
            right = Vector3.right;

        Vector3 lateral = right.normalized * ((col - half) * formationSpacing);
        Vector3 depth = -forward.normalized * (row * formationSpacing * 0.9f);
        return lateral + depth;
    }
}
