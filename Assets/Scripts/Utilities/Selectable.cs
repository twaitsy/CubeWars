using UnityEngine;

public class Selectable : MonoBehaviour
{
    [Tooltip("Optional visual ring GameObject to toggle on selection.")]
    public GameObject selectionRing;
    [Tooltip("Optional hover visual.")]
    public GameObject hoverRing;

    [Header("Optional Range Visuals")]
    public GameObject attackRangeCircle;
    public GameObject visionRangeCircle;

    public void SetSelected(bool selected)
    {
        if (selectionRing != null)
            selectionRing.SetActive(selected);

        if (!selected && hoverRing != null)
            hoverRing.SetActive(false);

        if (attackRangeCircle != null)
            attackRangeCircle.SetActive(selected);

        if (visionRangeCircle != null && TryGetComponent<UnitCombatController>(out var combat))
            visionRangeCircle.SetActive(selected && combat.ShowRangeGizmos);
    }

    public void SetHovered(bool isHovered)
    {
        if (hoverRing != null)
            hoverRing.SetActive(isHovered);
    }
}
