using UnityEngine;

public class CarryingController : MonoBehaviour
{
    [SerializeField] private int capacity = 30;

    private ResourceDefinition carriedResource;
    private int carriedAmount;

    public int Capacity => Mathf.Max(1, capacity);
    public ResourceDefinition CarriedResource => carriedResource;
    public int CarriedAmount => carriedAmount;
    public bool IsCarrying => carriedResource != null && carriedAmount > 0;

    public void SetCapacity(int value)
    {
        capacity = Mathf.Max(1, value);
        if (carriedAmount > capacity)
            carriedAmount = capacity;
    }

    public void SetCarried(ResourceDefinition resource, int amount)
    {
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
}
