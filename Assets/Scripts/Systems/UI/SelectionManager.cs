using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    [Header("References")]
    public Camera mainCamera;
    public UnitInspectorUI inspectorUI;

    GameObject currentSelection;

    void Awake()
    {
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TrySelect();

        if (Input.GetMouseButtonDown(1))
            TryIssueAttackOrder();
    }

    // -------------------------------------------------
    // SELECTION
    // -------------------------------------------------
    void TrySelect()
    {
        if (IMGUIInputBlocker.IsMouseOverUI(Input.mousePosition))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Selectable sel = hit.collider.GetComponentInParent<Selectable>();
            if (sel != null)
            {
                SetSelection(sel.gameObject);
                sel.SetSelected(true);
                return;
            }
        }

        ClearSelection();
    }

    // -------------------------------------------------
    // ATTACK ORDERS
    // -------------------------------------------------
    void TryIssueAttackOrder()
    {
        if (IMGUIInputBlocker.IsMouseOverUI(Input.mousePosition))
            return;

        if (currentSelection == null)
            return;

        if (!currentSelection.TryGetComponent<UnitCombatController>(out var combat))
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            Attackable atk = hit.collider.GetComponentInParent<Attackable>();

            if (atk != null)
            {
                combat.SetManualTarget(atk);
                return;
            }
        }

        // Right-clicked ground → clear order
        combat.ClearManualTarget();
    }

    // -------------------------------------------------
    // INTERNAL
    // -------------------------------------------------
    void SetSelection(GameObject go)
    {
        if (currentSelection == go) return;

        if (currentSelection != null)
        {
            var prev = currentSelection.GetComponentInParent<Selectable>();
            if (prev != null) prev.SetSelected(false);
        }

        currentSelection = go;
        inspectorUI.SetSelected(go);
    }

    void ClearSelection()
    {
        if (currentSelection != null)
        {
            var prev = currentSelection.GetComponentInParent<Selectable>();
            if (prev != null) prev.SetSelected(false);
        }

        currentSelection = null;
        inspectorUI.SetSelected(null);
    }
}
