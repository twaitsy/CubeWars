using UnityEngine;

public class CarryingController : MonoBehaviour
{
    [SerializeField] private int capacity = 30;

    private ResourceDefinition carriedResource;
    private int carriedAmount;

    // ---------------------------------------------------------
    // Public Properties
    // ---------------------------------------------------------
    public int Capacity => Mathf.Max(1, capacity);
    public ResourceDefinition CarriedResource => carriedResource;
    public int CarriedAmount => carriedAmount;

    public bool IsCarrying => carriedResource != null && carriedAmount > 0;
    public bool IsFull => carriedAmount >= Capacity;

    // ---------------------------------------------------------
    // Capacity Management
    // ---------------------------------------------------------
    public void SetCapacity(int value)
    {
        capacity = Mathf.Max(1, value);

        // Clamp carried amount if new capacity is smaller
        if (carriedAmount > capacity)
            carriedAmount = capacity;

        // If amount drops to zero, clear resource
        if (carriedAmount <= 0)
            carriedResource = null;
    }

    // ---------------------------------------------------------
    // Inventory Operations
    // ---------------------------------------------------------
    public int AddToInventory(ResourceDefinition type, int amount)
    {
        if (type == null || amount <= 0)
            return 0;

        int space = Capacity - carriedAmount;
        if (space <= 0)
            return 0;

        int accepted = Mathf.Min(space, amount);

        // If we weren't carrying anything, set the type
        if (carriedResource == null)
            carriedResource = type;

        // If we ARE carrying something, ensure it's the same type
        // (Optional: enforce single-resource carrying)
        if (carriedResource != type)
        {
            // Different resource type — cannot mix
            return 0;
        }

        carriedAmount += accepted;
        return accepted;
    }

    public void SetCarried(ResourceDefinition resource, int amount)
    {
        if (resource == null || amount <= 0)
        {
            ClearCarried();
            return;
        }

        carriedResource = resource;
        carriedAmount = Mathf.Clamp(amount, 0, Capacity);

        if (carriedAmount <= 0)
            carriedResource = null;
    }

    public void ClearCarried()
    {
        carriedResource = null;
        carriedAmount = 0;
    }

    // ---------------------------------------------------------
    // Withdrawal (optional helper)
    // ---------------------------------------------------------
    public int RemoveFromInventory(int amount)
    {
        if (carriedAmount <= 0 || amount <= 0)
            return 0;

        int removed = Mathf.Min(carriedAmount, amount);
        carriedAmount -= removed;

        if (carriedAmount <= 0)
            carriedResource = null;

        return removed;
    }
}