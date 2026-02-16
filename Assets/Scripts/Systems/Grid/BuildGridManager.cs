// ============================================================================
// BuildGridManager.cs
//
// PURPOSE:
// - Generates and manages the build grid around each Headquarters.
// - Controls visibility of the grid for the player.
// - Handles cell selection, highlighting, and click routing.
// - Ensures non-player grids are hidden and unclickable.
// - Acts as the central controller for grid-based building placement.
//
// DEPENDENCIES:
// - BuildGridCell:
//      * Created for each grid tile.
//      * Stores teamID, worldCenter, occupancy, placedObject.
// - BuildCellReservation (optional):
//      * Attached to cells to restrict placement.
// - BuildPlacementManager:
//      * Called when a cell is clicked to attempt building placement.
// - BuildMenuUI:
//      * Syncs grid visibility with build menu visibility.
// - Headquarters:
//      * Grid is generated around each HQ.
//      * teamID is read from Building component on HQ.
// - Building:
//      * Used to determine team ownership of HQs.
// - TeamVisual (optional):
//      * Not directly used here, but placed buildings rely on teamID set by this system.
// - EventSystem:
//      * Used indirectly via BuildGridCell to prevent UI click-through.
//
// NOTES FOR FUTURE MAINTENANCE:
// - If you add multi-cell buildings, grid generation must support footprint checks.
// - If you add terrain height variation, adjust cell Y positions to match terrain.
// - If you add fog-of-war, grid visibility should be tied to player vision.
// - If you add AI building logic, ensure AI respects teamID and occupancy rules.
// - If you add rotation-based building placement, highlight should reflect orientation.
// - If you add snapping to roads or zones, integrate reservation logic here.
//
// INSPECTOR REQUIREMENTS:
// - cellSize: spacing between grid tiles.
// - halfExtent: grid radius around HQ.
// - cellMaterial: default tile material.
// - selectedMaterial: highlight material.
// - buildMenuUI: optional; used to sync visibility.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildGridManager : MonoBehaviour
{
    struct CellKey : System.IEquatable<CellKey>
    {
        public int teamID;
        public Vector2Int coord;

        public CellKey(int teamID, Vector2Int coord)
        {
            this.teamID = teamID;
            this.coord = coord;
        }

        public bool Equals(CellKey other) => teamID == other.teamID && coord == other.coord;
        public override bool Equals(object obj) => obj is CellKey other && Equals(other);
        public override int GetHashCode() => (teamID * 397) ^ coord.GetHashCode();
    }

    [Header("Player")]
    public int playerTeamID = 0;

    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.B;

    [Header("Grid Settings")]
    public float cellSize = 2f;
    public int halfExtent = 6;
    public float yOffset = 0.03f;

    [Header("Visuals")]
    public Material cellMaterial;          // Mat_GridCell (blue)
    public Material selectedMaterial;      // Mat_GridCell_Selected (optional)

    [Header("UI")]
    public BuildMenuUI buildMenuUI;

    private bool isVisible = false;

    private readonly List<GameObject> allCells = new();
    private readonly Dictionary<CellKey, BuildGridCell> cellLookup = new();
    private BuildGridCell selectedCell;
    private BuildGridCell hoveredCell;

    void Start()
    {
        AutoAssignPlayerTeam();
        StartCoroutine(DelayedBuild());
    }

    void AutoAssignPlayerTeam()
    {
        var gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.playerTeam != null)
            playerTeamID = gm.playerTeam.teamID;
    }

    IEnumerator DelayedBuild()
    {
        yield return null; // wait 1 frame so HQs exist
        BuildAllGrids();

        SetPlayerGridVisible(false);

        if (buildMenuUI != null)
            buildMenuUI.SetVisible(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (allCells.Count == 0)
                BuildAllGrids();

            isVisible = !isVisible;
            SetPlayerGridVisible(isVisible);

            if (buildMenuUI != null)
                buildMenuUI.SetVisible(isVisible);
        }
    }

    public void BuildAllGrids()
    {
        // Destroy old cells
        for (int i = 0; i < allCells.Count; i++)
            if (allCells[i] != null) Destroy(allCells[i]);

        allCells.Clear();
        cellLookup.Clear();
        selectedCell = null;
        hoveredCell = null;

        Headquarters[] hqs = FindObjectsByType<Headquarters>(FindObjectsSortMode.None);
        Debug.Log($"[BuildGridManager] Found HQs: {hqs.Length}");

        foreach (var hq in hqs)
        {
            int teamID = 0;

            Building b = hq.GetComponent<Building>();
            if (b != null) teamID = b.teamID;

            BuildGridAroundHQ(hq.transform.position, teamID);
        }

        EnforceNonPlayerHiddenAndUnclickable();
    }

    void BuildGridAroundHQ(Vector3 hqPos, int teamID)
    {
        Vector3 origin = new(hqPos.x, yOffset, hqPos.z);

        origin.x = Mathf.Round(origin.x / cellSize) * cellSize;
        origin.z = Mathf.Round(origin.z / cellSize) * cellSize;

        GameObject parent = new($"BuildGrid_Team{teamID}");
        parent.transform.SetParent(transform, false);

        for (int gx = -halfExtent; gx <= halfExtent; gx++)
        {
            for (int gz = -halfExtent; gz <= halfExtent; gz++)
            {
                Vector3 center = origin + new Vector3(gx * cellSize, 0f, gz * cellSize);

                GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cellObj.name = $"Cell_{teamID}_{gx}_{gz}";
                cellObj.transform.SetParent(parent.transform, false);

                cellObj.transform.position = center;
                cellObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                cellObj.transform.localScale = Vector3.one * (cellSize * 0.95f);

                Collider col = cellObj.GetComponent<Collider>();
                if (col == null) cellObj.AddComponent<MeshCollider>();

                Renderer r = cellObj.GetComponent<Renderer>();
                if (r != null && cellMaterial != null)
                    r.sharedMaterial = cellMaterial;

                BuildGridCell cell = cellObj.AddComponent<BuildGridCell>();
                cell.Init(this, teamID, new Vector2Int(gx, gz), center);

                allCells.Add(cellObj);
                cellLookup[new CellKey(teamID, cell.gridCoord)] = cell;
            }
        }
    }

    public bool TryGetCell(int teamID, Vector2Int coord, out BuildGridCell cell)
    {
        return cellLookup.TryGetValue(new CellKey(teamID, coord), out cell);
    }

    public List<BuildGridCell> GetFootprintCells(int teamID, Vector2Int anchor, Vector2Int dimensions)
    {
        List<BuildGridCell> result = new();

        int width = Mathf.Max(1, dimensions.x);
        int depth = Mathf.Max(1, dimensions.y);

        for (int dx = 0; dx < width; dx++)
        {
            for (int dz = 0; dz < depth; dz++)
            {
                Vector2Int coord = new(anchor.x + dx, anchor.y + dz);
                if (!TryGetCell(teamID, coord, out BuildGridCell cell))
                    return null;

                result.Add(cell);
            }
        }

        return result;
    }

    public bool TryGetCenteredFootprintCells(int teamID, Vector2Int anchor, Vector2Int dimensions, out List<BuildGridCell> cells, out Vector3 footprintCenter)
    {
        cells = new List<BuildGridCell>();
        footprintCenter = Vector3.zero;

        int width = Mathf.Max(1, dimensions.x);
        int depth = Mathf.Max(1, dimensions.y);

        int minX = anchor.x - Mathf.FloorToInt((width - 1) * 0.5f);
        int minZ = anchor.y - Mathf.FloorToInt((depth - 1) * 0.5f);

        Vector3 centerAccumulator = Vector3.zero;

        for (int dx = 0; dx < width; dx++)
        {
            for (int dz = 0; dz < depth; dz++)
            {
                Vector2Int coord = new(minX + dx, minZ + dz);
                if (!TryGetCell(teamID, coord, out BuildGridCell cell))
                {
                    cells = null;
                    return false;
                }

                cells.Add(cell);
                centerAccumulator += cell.worldCenter;
            }
        }

        footprintCenter = cells.Count > 0 ? centerAccumulator / cells.Count : Vector3.zero;
        return cells.Count > 0;
    }

    void EnforceNonPlayerHiddenAndUnclickable()
    {
        for (int i = 0; i < allCells.Count; i++)
        {
            var go = allCells[i];
            if (go == null) continue;

            var cell = go.GetComponent<BuildGridCell>();
            if (cell == null) continue;

            if (cell.teamID != playerTeamID)
            {
                var r = go.GetComponent<Renderer>();
                if (r != null) r.enabled = false;

                var c = go.GetComponent<Collider>();
                if (c != null) c.enabled = false;
            }
        }
    }

    void SetPlayerGridVisible(bool visible)
    {
        for (int i = 0; i < allCells.Count; i++)
        {
            var go = allCells[i];
            if (go == null) continue;

            var cell = go.GetComponent<BuildGridCell>();
            if (cell == null) continue;

            if (cell.teamID == playerTeamID)
            {
                ApplyCellVisibility(cell, visible);
            }
            else
            {
                var r = go.GetComponent<Renderer>();
                if (r != null) r.enabled = false;

                var c = go.GetComponent<Collider>();
                if (c != null) c.enabled = false;
            }
        }

        if (!visible && selectedCell != null)
        {
            SetCellMaterial(selectedCell, cellMaterial);
            selectedCell = null;
        }
    }

    void ApplyCellVisibility(BuildGridCell cell, bool gridVisible)
    {
        if (cell == null) return;

        bool showTile = gridVisible && !cell.isOccupied;

        var r = cell.GetComponent<Renderer>();
        if (r != null) r.enabled = showTile;

        var c = cell.GetComponent<Collider>();
        if (c != null) c.enabled = showTile;
    }

    public void OnCellClicked(BuildGridCell cell)
    {
        if (!isVisible) return;
        if (cell == null) return;

        if (cell.teamID != playerTeamID) return;

        if (cell.isOccupied) return;

        if (BuildPlacementManager.Instance != null &&
            BuildPlacementManager.Instance.selectedItem != null)
        {
            if (BuildPlacementManager.Instance.TryPlace(cell))
            {
                RefreshPlayerGridVisibility();

                if (selectedCell != null) SetCellMaterial(selectedCell, cellMaterial);
                selectedCell = null;
                return;
            }
        }

        if (selectedCell != null && selectedCell != cell)
            SetCellMaterial(selectedCell, cellMaterial);

        selectedCell = cell;

        if (selectedMaterial != null)
            SetCellMaterial(selectedCell, selectedMaterial);
    }

    public void RefreshPlayerGridVisibility()
    {
        SetPlayerGridVisible(isVisible);
    }

    public void OnCellHovered(BuildGridCell cell)
    {
        hoveredCell = cell;

        if (!isVisible || cell == null || cell.teamID != playerTeamID)
        {
            BuildPlacementManager.Instance?.ClearPreview();
            return;
        }

        BuildPlacementManager.Instance?.ShowPreviewAt(cell);
    }

    public void OnCellHoverExit(BuildGridCell cell)
    {
        if (hoveredCell == cell)
            hoveredCell = null;

        BuildPlacementManager.Instance?.ClearPreview();
    }

    public void RefreshHoveredPreview()
    {
        if (hoveredCell == null)
            return;

        OnCellHovered(hoveredCell);
    }

    void SetCellMaterial(BuildGridCell cell, Material mat)
    {
        if (cell == null || mat == null) return;
        var r = cell.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    void LateUpdate()
    {
        if (!isVisible)
            BuildPlacementManager.Instance?.ClearPreview();
    }
}
