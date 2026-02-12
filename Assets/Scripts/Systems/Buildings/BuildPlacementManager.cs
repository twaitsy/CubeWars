// =============================================================
// BuildPlacementManager.cs
//
// PURPOSE:
// - Handles ALL building placement logic (player + AI).
// - Supports grid-based and world-space placement.
// - Creates ConstructionSite objects or instantly places buildings.
//
// DEPENDENCIES:
// - BuildItemDefinition:
//      * Prefab, costs, offsets.
// - BuildGridCell:
//      * Grid-based placement.
// - BuildCellReservation:
//      * Blocks placement when reserved.
// - ConstructionSite:
//      * Created when useConstructionSites = true.
// - TeamStorageManager / TeamResources:
//      * Cost checks + resource spending.
// - Building / Unit / Civilian:
//      * teamID applied to placed objects.
// - ResourceStorageProvider / ResourceDropoff:
//      * teamID applied.
// - TeamVisual:
//      * Applies team colors.
// - Physics:
//      * Used for world-space collision checks.
//
// NOTES FOR FUTURE MAINTENANCE:
// - Add multi-cell footprints if needed.
// - Add rotation support.
// - Add terrain height adaptation.
// - Add blueprint cancellation tracking.
// - Add MaterialPropertyBlock for preview performance.
//
// IMPORTANT:
// - This script does NOT delete teams.
// - It ONLY deletes duplicate BuildPlacementManager components,
//   NOT Team GameObjects.
// =============================================================

using UnityEngine;
using System.Collections.Generic;

public class BuildPlacementManager : MonoBehaviour
{
    public static BuildPlacementManager Instance;

    [Header("Selected (Player)")]
    public BuildItemDefinition selectedItem;

    [Header("Construction")]
    public bool useConstructionSites = true;
    public GameObject constructionSitePrefab; // MUST have ConstructionSite component

    [Tooltip("If true, you can only place blueprints when you can afford the full cost right now.")]
    public bool requireAffordToPlace = true;

    [Tooltip("If true, reserves required resources from building storage when placing a construction site.")]
    public bool reserveResourcesForSites = true;

    [Header("Placement Rules")]
    [Tooltip("If true, blocks placement on cells with BuildCellReservation.blockBuildingPlacement.")]
    public bool respectCellReservations = true;

    [Header("Preview (Optional)")]
    public bool showPreview = true;
    public Material previewMaterial;
    [Range(0.05f, 1f)] public float previewAlpha = 0.35f;

    private GameObject previewObj;
    private BuildItemDefinition previewItem;
    private int placementQuarterTurns;

    // DEPENDENCIES:
    // - BuildItemDefinition: uses prefab, costs, yOffset
    // - ConstructionSite: must implement Init(cell, teamID, item, reserveResources)
    // - TeamResources / TeamStorageManager: used for cost checks
    // - BuildGridCell: grid-based placement for player/AI grid builds

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetSelected(BuildItemDefinition item)
    {
        selectedItem = item;
        placementQuarterTurns = 0;
        ClearPreview();
        Debug.Log($"[BuildPlacementManager] Selected: {(item != null ? item.displayName : "None")}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && selectedItem != null)
        {
            placementQuarterTurns = (placementQuarterTurns + 1) % 4;

            var grid = FindObjectOfType<BuildGridManager>();
            if (grid != null)
                grid.RefreshHoveredPreview();
        }
    }

    public bool TryPlace(BuildGridCell cell)
    {
        if (selectedItem == null) return false;
        return TryPlace(cell, selectedItem);
    }

    public bool CanPlaceAt(Vector3 pos)
    {
        return !Physics.CheckSphere(pos, 1.5f);
    }

    // ---------- GRID-BASED PLACEMENT (PLAYER / TEAM AIBuild) ----------

    public bool TryPlace(BuildGridCell cell, BuildItemDefinition item)
    {
        if (cell == null || item == null) return false;
        if (item.prefab == null)
        {
            Debug.LogWarning("[BuildPlacementManager] Build item has no prefab assigned.");
            return false;
        }

        BuildingFootprint footprint = BuildingFootprint.FindOnPrefab(item.prefab);
        Vector2Int dimensions = footprint != null
            ? footprint.GetDimensionsForQuarterTurns(placementQuarterTurns)
            : Vector2Int.one;

        BuildGridManager grid = FindObjectOfType<BuildGridManager>();
        Vector3 footprintCenter = cell.worldCenter;
        List<BuildGridCell> cells = null;

        if (grid != null)
        {
            if (!grid.TryGetCenteredFootprintCells(cell.teamID, cell.gridCoord, dimensions, out cells, out footprintCenter))
                return false;
        }
        else
        {
            cells = new List<BuildGridCell> { cell };
        }

        if (cells == null || cells.Count == 0)
            return false;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i] == null || cells[i].isOccupied)
                return false;
        }

        if (respectCellReservations)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                var res = cells[i].GetComponent<BuildCellReservation>();
                if (res != null && res.blockBuildingPlacement)
                    return false;
            }
        }

        // Always enforce "deposited in buildings" if storage manager exists
        if (requireAffordToPlace)
        {
            bool canAfford = false;

            if (TeamStorageManager.Instance != null)
                canAfford = TeamStorageManager.Instance.CanAffordAvailable(cell.teamID, item.costs);
            else if (TeamResources.Instance != null)
                canAfford = TeamResources.Instance.CanAfford(cell.teamID, item.costs);

            if (!canAfford)
                return false;
        }

        Vector3 pos = GetPlacementPosition(footprintCenter, item, footprint);
        Quaternion rot = GetPlacementRotation(footprint);

        GameObject placed;

        if (useConstructionSites && constructionSitePrefab != null)
        {
            placed = Instantiate(constructionSitePrefab, pos, rot);

            EnsureBuildItemInstance(placed, item);

            var site = placed.GetComponent<ConstructionSite>();
            if (site == null)
            {
                Debug.LogWarning("[BuildPlacementManager] constructionSitePrefab is missing ConstructionSite component.");
                Destroy(placed);
                return false;
            }

            site.Init(cell, cell.teamID, item, reserveResourcesForSites, cells);

            if (!site.InitOK)
            {
                Destroy(placed);
                return false;
            }

            var tv = placed.GetComponent<TeamVisual>();
            if (tv != null)
            {
                tv.teamID = cell.teamID;
                tv.kind = VisualKind.Building;
                tv.Apply();
            }
        }
        else
        {
            // Instant placement (spend immediately from building storage)
            if (TeamResources.Instance != null)
            {
                if (!TeamResources.Instance.CanAfford(cell.teamID, item.costs))
                    return false;

                if (!TeamResources.Instance.Spend(cell.teamID, item.costs))
                    return false;
            }

            placed = Instantiate(item.prefab, pos, rot);

            EnsureBuildItemInstance(placed, item);
            ApplyTeamToPlacedObject(placed, cell.teamID);
        }

        AttachOccupancyTracker(placed, cells);

        return true;
    }

    // ---------- WORLD-SPACE PLACEMENT (AIBuilder / AIRebuildManager) ----------

    public bool PlaceBuild(BuildItemDefinition item, Vector3 pos, Quaternion rot, int teamID)
    {
        if (item == null || item.prefab == null)
            return false;

        if (!CanPlaceAt(pos))
            return false;

        // Cost check + spend
        if (TeamResources.Instance != null)
        {
            if (!TeamResources.Instance.CanAfford(teamID, item.costs))
                return false;

            if (!TeamResources.Instance.Spend(teamID, item.costs))
                return false;
        }

        Vector3 finalPos = pos
                           + new Vector3(item.placementOffset.x, item.yOffset + item.placementOffset.y, item.placementOffset.z);

        GameObject placed = Instantiate(item.prefab, finalPos, rot);

        EnsureBuildItemInstance(placed, item);
        ApplyTeamToPlacedObject(placed, teamID);

        return true;
    }

    void EnsureBuildItemInstance(GameObject go, BuildItemDefinition item)
    {
        if (go == null || item == null) return;

        BuildItemInstance bii = go.GetComponent<BuildItemInstance>();
        if (bii == null) bii = go.AddComponent<BuildItemInstance>();
        bii.itemId = item.name;
    }

    void ApplyTeamToPlacedObject(GameObject placed, int teamID)
    {
        if (placed == null) return;
        TeamAssignmentUtility.ApplyTeamToHierarchy(placed, teamID);
    }

    // ---------- Optional Preview ----------

    public void ShowPreviewAt(BuildGridCell cell)
    {
        if (!showPreview) return;
        if (cell == null) return;
        if (selectedItem == null) return;
        if (previewMaterial == null) return;

        if (selectedItem.prefab == null)
        {
            ClearPreview();
            return;
        }

        BuildingFootprint footprint = BuildingFootprint.FindOnPrefab(selectedItem.prefab);
        Vector2Int dimensions = footprint != null
            ? footprint.GetDimensionsForQuarterTurns(placementQuarterTurns)
            : Vector2Int.one;

        var grid = FindObjectOfType<BuildGridManager>();
        Vector3 footprintCenter = cell.worldCenter;
        List<BuildGridCell> cells = null;

        if (grid != null)
        {
            if (!grid.TryGetCenteredFootprintCells(cell.teamID, cell.gridCoord, dimensions, out cells, out footprintCenter))
            {
                ClearPreview();
                return;
            }
        }
        else
        {
            cells = new List<BuildGridCell> { cell };
        }

        if (cells == null || cells.Count == 0)
        {
            ClearPreview();
            return;
        }

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].isOccupied)
            {
                ClearPreview();
                return;
            }

            if (respectCellReservations)
            {
                var res = cells[i].GetComponent<BuildCellReservation>();
                if (res != null && res.blockBuildingPlacement)
                {
                    ClearPreview();
                    return;
                }
            }
        }

        Vector3 pos = GetPlacementPosition(footprintCenter, selectedItem, footprint);
        Quaternion rot = GetPlacementRotation(footprint);

        if (previewObj == null || previewItem != selectedItem)
        {
            if (previewObj != null)
                Destroy(previewObj);

            previewObj = Instantiate(selectedItem.prefab);
            previewObj.name = "BuildPreview";
            DisableGameplayComponents(previewObj);
            ApplyPreviewMaterial(previewObj);
            previewItem = selectedItem;
        }

        previewObj.transform.SetPositionAndRotation(pos, rot);
        previewObj.SetActive(true);
    }

    public void ClearPreview()
    {
        if (previewObj != null)
            previewObj.SetActive(false);
    }

    Vector3 GetPlacementPosition(Vector3 footprintCenter, BuildItemDefinition item, BuildingFootprint footprint)
    {
        Vector3 pos = footprintCenter
                      + new Vector3(item.placementOffset.x, item.yOffset + item.placementOffset.y, item.placementOffset.z);

        if (footprint != null)
            pos += Quaternion.Euler(0f, placementQuarterTurns * 90f, 0f) * footprint.worldOffset;

        return pos;
    }

    Quaternion GetPlacementRotation(BuildingFootprint footprint)
    {
        float extra = footprint != null ? footprint.extraYRotation : 0f;
        return Quaternion.Euler(0f, (placementQuarterTurns * 90f) + extra, 0f);
    }

    void AttachOccupancyTracker(GameObject placed, List<BuildGridCell> cells)
    {
        if (placed == null || cells == null) return;

        BuildGridOccupant occ = placed.GetComponent<BuildGridOccupant>();
        if (occ == null)
            occ = placed.AddComponent<BuildGridOccupant>();

        occ.SetOccupiedCells(cells, placed);
    }

    void DisableGameplayComponents(GameObject go)
    {
        foreach (var c in go.GetComponentsInChildren<Collider>(true))
            c.enabled = false;

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            if (mb is BuildPlacementManager) continue;
            mb.enabled = false;
        }
    }

    void ApplyPreviewMaterial(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r == null) continue;

            Material m = new Material(previewMaterial);
            Color c = m.color;
            c.a = previewAlpha;
            m.color = c;

            r.material = m;
        }
    }
}
