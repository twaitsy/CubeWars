using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public UnitInspectorUI inspectorUI;

    [Header("Selection")]
    [Min(4f)] public float dragThreshold = 8f;

    readonly List<GameObject> selected = new List<GameObject>(32);
    Vector2 dragStart;
    bool isDragging;
    Selectable hovered;

    public IReadOnlyList<GameObject> CurrentSelection => selected;
    public GameObject PrimarySelection => selected.Count > 0 ? selected[0] : null;

    void Awake()
    {
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update()
    {
        if (mainCamera == null)
            return;

        UpdateHover();

        if (Input.GetMouseButtonDown(0))
        {
            dragStart = Input.mousePosition;
            isDragging = false;
        }

        if (Input.GetMouseButton(0) && !isDragging)
            isDragging = Vector2.Distance(dragStart, Input.mousePosition) > dragThreshold;

        if (Input.GetMouseButtonUp(0))
        {
            if (IMGUIInputBlocker.IsMouseOverUI(Input.mousePosition))
                return;

            if (isDragging) HandleDragSelection();
            else HandleClickSelection();

            isDragging = false;
        }

        if (Input.GetMouseButtonDown(1))
            HandleContextCommand();

        if (Input.GetKeyDown(KeyCode.R) && PrimarySelection != null && PrimarySelection.TryGetComponent<UnitCombatController>(out var combat))
            combat.ToggleRangeGizmos();
    }

    void UpdateHover()
    {
        var prev = hovered;
        hovered = null;

        if (IMGUIInputBlocker.IsMouseOverUI(Input.mousePosition))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 600f))
            return;

        hovered = hit.collider.GetComponentInParent<Selectable>();

        if (prev != hovered)
        {
            if (prev != null) prev.SetHovered(false);
            if (hovered != null) hovered.SetHovered(true);
        }
    }

    void HandleClickSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        var hitObject = GetSelectionObjectAtMouse();
        if (hitObject == null)
        {
            if (!shift)
                ClearSelection();
            return;
        }

        if (shift)
        {
            if (selected.Contains(hitObject)) RemoveFromSelection(hitObject);
            else AddToSelection(hitObject);
        }
        else
        {
            ReplaceSelection(hitObject);
        }
    }

    void HandleDragSelection()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Rect rect = BuildScreenRect(dragStart, Input.mousePosition);

        var allSelectables = FindObjectsOfType<Selectable>();
        var inside = new List<GameObject>();

        foreach (var sel in allSelectables)
        {
            if (sel == null) continue;
            Vector3 screen = mainCamera.WorldToScreenPoint(sel.transform.position);
            if (screen.z < 0f) continue;
            Vector2 p = new Vector2(screen.x, screen.y);
            if (rect.Contains(p))
                inside.Add(sel.gameObject);
        }

        if (!shift)
            ClearSelection();

        foreach (var go in inside)
        {
            if (selected.Contains(go))
            {
                if (shift)
                    RemoveFromSelection(go);
            }
            else
            {
                AddToSelection(go);
            }
        }

        SyncInspector();
    }

    void HandleContextCommand()
    {
        if (IMGUIInputBlocker.IsMouseOverUI(Input.mousePosition) || selected.Count == 0)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 600f))
            return;

        Attackable attackTarget = hit.collider.GetComponentInParent<Attackable>();
        ResourceNode resource = hit.collider.GetComponentInParent<ResourceNode>();
        bool issueAttackMove = Input.GetKey(KeyCode.A);

        Vector3 anchor = GetSelectionCenter();
        Vector3 forward = (hit.point - anchor);
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;

        for (int i = 0; i < selected.Count; i++)
        {
            var go = selected[i];
            if (go == null) continue;

            Vector3 targetPoint = hit.point;
            if (UnitManager.Instance != null)
                targetPoint += UnitManager.Instance.GetFormationOffset(i, selected.Count, forward);

            if (resource != null && go.TryGetComponent<Civilian>(out var civ))
            {
                civ.AssignPreferredNode(resource);
                continue;
            }

            if (attackTarget != null && go.TryGetComponent<UnitCombatController>(out var combat))
            {
                combat.SetManualTarget(attackTarget);
                continue;
            }

            if (go.TryGetComponent<UnitCombatController>(out var moveCombat))
            {
                if (issueAttackMove) moveCombat.IssueAttackMoveOrder(targetPoint);
                else moveCombat.IssueMoveOrder(targetPoint);
            }

            if (go.TryGetComponent<Unit>(out var unit))
                unit.MoveTo(targetPoint);

            if (go.TryGetComponent<Civilian>(out var civilian))
                civilian.IssueMoveCommand(targetPoint);
        }
    }

    GameObject GetSelectionObjectAtMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit, 600f))
            return null;

        var sel = hit.collider.GetComponentInParent<Selectable>();
        if (sel != null) return sel.gameObject;

        var civ = hit.collider.GetComponentInParent<Civilian>();
        if (civ != null) return civ.gameObject;

        var unit = hit.collider.GetComponentInParent<Unit>();
        if (unit != null) return unit.gameObject;

        var building = hit.collider.GetComponentInParent<Building>();
        if (building != null) return building.gameObject;

        var node = hit.collider.GetComponentInParent<ResourceNode>();
        if (node != null) return node.gameObject;

        return null;
    }

    Vector3 GetSelectionCenter()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var go in selected)
        {
            if (go == null) continue;
            sum += go.transform.position;
            count++;
        }
        return count == 0 ? Vector3.zero : sum / count;
    }

    static Rect BuildScreenRect(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    void ReplaceSelection(GameObject go)
    {
        ClearSelection();
        AddToSelection(go);
        SyncInspector();
    }

    void AddToSelection(GameObject go)
    {
        if (go == null || selected.Contains(go)) return;
        selected.Add(go);
        go.GetComponentInParent<Selectable>()?.SetSelected(true);
    }

    void RemoveFromSelection(GameObject go)
    {
        if (go == null) return;
        if (selected.Remove(go))
            go.GetComponentInParent<Selectable>()?.SetSelected(false);
    }

    void ClearSelection()
    {
        for (int i = 0; i < selected.Count; i++)
            selected[i]?.GetComponentInParent<Selectable>()?.SetSelected(false);
        selected.Clear();
        SyncInspector();
    }

    void SyncInspector()
    {
        if (inspectorUI != null)
            inspectorUI.SetSelected(PrimarySelection);
    }

    void OnGUI()
    {
        if (!isDragging) return;

        Rect rect = BuildScreenRect(dragStart, Input.mousePosition);
        rect.y = Screen.height - rect.y - rect.height;

        GUI.color = new Color(0.2f, 0.8f, 1f, 0.15f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = new Color(0.2f, 0.8f, 1f, 0.85f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;
    }
}
