using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks the grid cells occupied by a placed object and releases them when destroyed.
/// </summary>
public class BuildGridOccupant : MonoBehaviour
{
    private readonly List<BuildGridCell> occupiedCells = new();

    public void SetOccupiedCells(List<BuildGridCell> cells, GameObject owner)
    {
        occupiedCells.Clear();
        if (cells == null) return;

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            if (cell == null) continue;

            cell.isOccupied = true;
            cell.placedObject = owner;
            occupiedCells.Add(cell);
        }
    }

    void OnDestroy()
    {
        for (int i = 0; i < occupiedCells.Count; i++)
        {
            var cell = occupiedCells[i];
            if (cell == null) continue;
            if (cell.placedObject == gameObject)
            {
                cell.isOccupied = false;
                cell.placedObject = null;
            }
        }
    }
}
